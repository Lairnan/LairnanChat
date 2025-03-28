using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace LairnanChat.Plugins.Layer.Implements.Services
{
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly Random _random = new();
        private const int BufferSize = 1024 * 8;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private ClientWebSocket? _webSocket = null;
        private Task? _listenerForMessagesTask = null;
        private CancellationTokenSource _cts = new();

        public ChatServerInfo ServerInfo { get; }
        
        public IList<ActionResult>? ReceivedResults { get; private set; }
        
        public ChatService(ILogger<ChatService> logger, ChatServerInfo serverInfo)
        {
            _logger = logger;
            _logger.LogInformation("Initialized ChatService");
            ServerInfo = serverInfo;
        }
        
        public event EventHandler<ActionResult>? MessageReceived;
        
        public User? ConnectedUser { get; private set; }
        
        public async Task SendRequestAsync(ActionRequest actionRequest)
        {
            if (_webSocket is not { State: WebSocketState.Open })
                return;
            
            await SendRequestAsync(_webSocket, actionRequest);
        }

        public void SetUrlServer(string url)
        {
            if (_webSocket is { State: WebSocketState.Open or WebSocketState.Connecting })
                throw new InvalidOperationException("It is not possible to change the server while the connection is active.");

            ServerInfo.Url = url;
        }
        private void ReceiveResultMessage(object? sender, ActionResult actionResult)
        {
            ReceivedResults?.Add(actionResult);
        }

        public async Task<ActionResult> ConnectAsync(AuthUser authUser, bool needRegistration = false)
        {
            if (string.IsNullOrWhiteSpace(ServerInfo.Url))
            {
                return new ActionResult(ResultType.Error, $"Use {nameof(SetUrlServer)} before try connect.");
            }

            _logger.LogInformation("[{MethodName}] Connecting to {Url}", nameof(ConnectAsync), ServerInfo.Url);
            ArgumentNullException.ThrowIfNull(authUser);

            if (_webSocket is { State: WebSocketState.Open or WebSocketState.Connecting })
            {
                _logger.LogWarning("[{MethodName}] Already connected", nameof(ConnectAsync));
                ServerInfo.IsConnected = true;
                return new ActionResult(ResultType.Error, "Already connected");
            }
            ServerInfo.IsConnected = false;

            var webSocket = new ClientWebSocket();
            try
            {
                await webSocket.ConnectAsync(new Uri(ServerInfo.Url), CancellationToken.None);
                var cts = new CancellationTokenSource();

                _webSocket = webSocket;
                _cts = cts;

                var requestMessage = new ActionRequest(RequestType.Connect);
                _logger.LogInformation("[{MethodName}] Sending initial Connect request", nameof(ConnectAsync));
                await SendRequestAsync(webSocket, requestMessage);

                var response = await WaitForResponseAsync(webSocket, cts, TimeSpan.FromSeconds(15));
                if (response == null)
                {
                    _logger.LogError("[{MethodName}] No response after initial Connect", nameof(ConnectAsync));
                    await DisconnectAsync();
                    return new ActionResult(ResultType.Error, "No response from server");
                }
                switch (response.ResultType)
                {
                    case ResultType.Error:
                        _logger.LogError("[{MethodName}] Server error: {Message}", nameof(ConnectAsync), response.ResultData);
                        await DisconnectAsync();
                        return response;
                    case ResultType.NeedAuthentication:
                    {
                        var secondRequestType = needRegistration ? RequestType.Registration : RequestType.Authorization;

                        _logger.LogInformation("[{MethodName}] Server requires authentication. Proceeding with {SecondRequestType}",
                            nameof(ConnectAsync), secondRequestType);
                        requestMessage = new ActionRequest(secondRequestType, authUser);
                        await SendRequestAsync(webSocket, requestMessage);

                        response = await WaitForResponseAsync(webSocket, cts, TimeSpan.FromSeconds(15));
                        if (response == null || response.ResultType == ResultType.Error)
                        {
                            _logger.LogError("[{MethodName}] No valid response after authentication. {ResponseMessage}",
                                nameof(ConnectAsync), response?.ResultData ?? "No response from server");
                            await DisconnectAsync();
                            return new ActionResult(ResultType.Error, "No response from server");
                        }
                        break;
                    }
                    default:
                    {
                        _logger.LogInformation("[{MethodName}] Server did not require authentication. Sending Connect with authUser.",
                            nameof(ConnectAsync));
                        requestMessage = new ActionRequest(RequestType.Connect, authUser);
                        await SendRequestAsync(webSocket, requestMessage);
                        response = await WaitForResponseAsync(webSocket, cts, TimeSpan.FromSeconds(15));
                        if (response == null || response.ResultType == ResultType.Error)
                        {
                            _logger.LogError("[{MethodName}] Error during connection: {ResponseMessage}",
                                nameof(ConnectAsync), response?.ResultData ?? "No response from server");
                            await DisconnectAsync();
                            return new ActionResult(ResultType.Error, "No response from server");
                        }
                        break;
                    }
                }

                if (response.ResultType is ResultType.Connect or ResultType.SuccessAuthorized or ResultType.SuccessRegistered)
                {
                    var user = JsonSerializer.Deserialize<User>(response.ResultData!);
                    ConnectedUser = user;
                }
                else
                {
                    _logger.LogError("[{MethodName}] Unexpected server response: {ResultType}",
                        nameof(ConnectAsync), response.ResultType);
                    await DisconnectAsync();
                    return new ActionResult(ResultType.Error, "Unexpected server response");
                }

                ReceivedResults = [];
                MessageReceived += ReceiveResultMessage;
                _listenerForMessagesTask = ListenForMessages(webSocket, cts);
            }
            catch (Exception ex)
            {
                if (webSocket.State == WebSocketState.Open)
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
                _webSocket = null;
                _listenerForMessagesTask = null;
                _logger.LogError(ex, "[{MethodName}] Failed to connect to {Url}", nameof(ConnectAsync), ServerInfo.Url);
                return new ActionResult(ResultType.Error, $"Failed to connect to {ServerInfo.Url}");
            }
            _logger.LogInformation("[{MethodName}] Connected to {Url}", nameof(ConnectAsync), ServerInfo.Url);
            if (ConnectedUser == null)
            {
                _logger.LogError("[{MethodName}] User not connected. Unexpected error", nameof(ConnectAsync));
                await DisconnectAsync();
                return new ActionResult(ResultType.Error, "User not connected");
            }
            ServerInfo.IsConnected = true;
            return new ActionResult(ResultType.Connect, ConnectedUser);
        }

        private async Task<ActionResult?> WaitForResponseAsync(WebSocket webSocket, CancellationTokenSource cts, TimeSpan timeout)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
            linkedCts.CancelAfter(timeout);
            try
            {
                while (!linkedCts.Token.IsCancellationRequested && webSocket.State == WebSocketState.Open)
                {
                    ActionResult result;
                    var messageReceived = await webSocket.ReceiveFullMessageAsync(BufferSize, linkedCts.Token);
                    if (messageReceived.IsClosed)
                    {
                        _logger.LogWarning("[{MethodName}] Received close message: {Message}", nameof(WaitForResponseAsync), messageReceived.Value);
                        var message = new Message(null!, receiver: null, "Server disconnected", "en-US");
                        result = new ActionResult(ResultType.Disconnect, message);
                        ServerInfo.IsConnected = false;
                    }
                    else
                    {
                        try
                        {
                            _logger.LogInformation("[{MethodName}] Received response: {Response}", nameof(WaitForResponseAsync), messageReceived.Value);
                            result = JsonSerializer.Deserialize<ActionResult>(messageReceived.Value!)!;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[{MethodName}] Failed to deserialize message: {Message}", nameof(WaitForResponseAsync), messageReceived.Value!);
                            var message = new Message(null!, receiver: null, "Failed to get message", "en-US");
                            result = new ActionResult(ResultType.Error, message);
                        }
                    }
                    return result;
                }
            }
            catch (OperationCanceledException) when (linkedCts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("[{MethodName}] Operation timed out", nameof(WaitForResponseAsync));
                return new ActionResult(ResultType.Error, new Message(null!, receiver: null, "Operation timed out", "en-US"));
            }
            return null;
        }

        private async Task SendRequestAsync(WebSocket webSocket, ActionRequest actionRequest)
        {
            _logger.LogInformation("[{MethodName}] Sending request", nameof(SendRequestAsync));
            var jsonRequest = JsonSerializer.Serialize(actionRequest, _jsonOptions);
            _logger.LogInformation("[{MethodName}] Request: {JsonRequest}", nameof(SendRequestAsync), jsonRequest);
            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonRequest)),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{MethodName}] Failed to send request to server {Url}", nameof(SendRequestAsync), ServerInfo!.Url);
            }
        }
        
        public WebSocketState GetConnectionStatus() => _webSocket?.State ?? WebSocketState.Closed;

        public async Task DisconnectAsync()
        {
            _logger.LogInformation("[{MethodName}] Disconnecting...", nameof(DisconnectAsync));
            ServerInfo!.IsConnected = false;
            MessageReceived -= ReceiveResultMessage;
            ReceivedResults = null;
            if (_webSocket is not { State: WebSocketState.Open })
            {
                _webSocket = null;
                _cts.TryReset();
                _listenerForMessagesTask = null;
                ConnectedUser = null;
                _logger.LogWarning("[{MethodName}] Already disconnected", nameof(DisconnectAsync));
                return;
            }
            
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await _cts.CancelAsync();

                _webSocket = null;
                _cts.TryReset();
                _listenerForMessagesTask = null;
                ConnectedUser = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{MethodName}] Error while disconnecting from {Url}", nameof(DisconnectAsync), ServerInfo!.Url ?? "Unknown address");
            }
        }

        private async Task ListenForMessages(ClientWebSocket webSocket, CancellationTokenSource ctx)
        {
            while (!ctx.IsCancellationRequested && webSocket.State == WebSocketState.Open)
            {
                ActionResult result;
                var messageReceived = await webSocket.ReceiveFullMessageAsync(BufferSize, ctx.Token);
                if (messageReceived.IsClosed)
                {
                    _logger.LogWarning("[{MethodName}] Received close message: {Message}", nameof(ListenForMessages), messageReceived.Value);
                    var message = new Message(null!, receiver: null, "Server disconnected", "en-US");
                    result = new ActionResult(ResultType.Disconnect, message);
                    ServerInfo.IsConnected = false;
                }
                else
                {
                    try
                    {
                        _logger.LogInformation("[{MethodName}] Received message: {Response}", nameof(ListenForMessages), messageReceived.Value);
                        result = JsonSerializer.Deserialize<ActionResult>(messageReceived.Value!)!;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{MethodName}] Failed to deserialize message: {Message}", nameof(ListenForMessages), messageReceived.Value!);
                        var message = new Message(null!, receiver: null, "Failed to get message", "en-US");
                        result = new ActionResult(ResultType.Error, message);
                    }
                }
                MessageReceived?.Invoke(this, result);
            }
        }

        private string GenerateRandomName()
        {
            var names = new[]
            {
                "James", "John", "Robert", "Michael", "William",
                "David", "Richard", "Joseph", "Charles", "Thomas",
                "Christopher", "Daniel", "Matthew", "Anthony", "Mark",
                "Donald", "Steven", "Paul", "Andrew", "Joshua",
                "Kevin", "Brian", "Edward", "Ronald", "Timothy",
                "Jason", "Jeffrey", "Ryan", "Jacob", "Gary",
                "Nicholas", "Eric", "Jonathan", "Stephen", "Larry",
                "Justin", "Scott", "Brandon", "Benjamin", "Samuel"
            };

            return names[_random.Next(names.Length)];
        }
    }
}
