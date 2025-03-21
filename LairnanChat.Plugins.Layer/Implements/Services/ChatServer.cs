using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LairnanChat.Plugins.Layer.Implements.Services;

public partial class ChatServer : IChatServer
{
    private readonly ILogger<ChatServer> _logger;
    private readonly ConcurrentDictionary<WebSocket, User> _clients = [];
    private readonly ILanguageTranslationService _languagesService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IChatRoomsDatabase _chatRoomsDatabase;
    private readonly int _bufferSize;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    private event Action<ActionRequest, User> RequestReceiver;
    private HttpListener? _listener = null;
    private Task? _listeningHttpClient = null;
    private CancellationTokenSource _cts = new();
    
    public ChatServer(ILogger<ChatServer> logger, ILanguageTranslationService languagesService,
        IAuthenticationService authenticationService, IChatRoomsDatabase chatRoomsDatabase,
        IConfiguration configuration)
    {
        _logger = logger;
        _languagesService = languagesService;
        _authenticationService = authenticationService;
        _chatRoomsDatabase = chatRoomsDatabase;
        _bufferSize = configuration.GetValue<int?>("BufferSize") ?? 1024 * 4;
        RequestReceiver += OnRequestReceiver;
    }

    public async Task StartAsync(string url, bool enableAuthentication = false)
    {
        ArgumentNullException.ThrowIfNull(url);
        if (_listener != null)
            throw new InvalidOperationException("Already listening");

        try
        {
            _clients.Clear();
            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _listener.Start();
            _logger.LogInformation("[{MethodName}] Listening on {url}", nameof(StartAsync), url);
            
            _listeningHttpClient = StartListeningHttpClient(enableAuthentication);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogCritical(ex, "[{MethodName}] Server is stopping...", nameof(StartAsync));
            await StopAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{MethodName}] Unhandled exception", nameof(StartAsync));
            await StopAsync();
        }
    }

    private async Task StartListeningHttpClient(bool enableAuthentication)
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var context = await _listener!.GetContextAsync().WaitAsync(_cts.Token);
                if (context.Request.IsWebSocketRequest)
                {
                    _ = HandleClient(context, enableAuthentication);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogCritical(ex, "[{MethodName}] Server is stopping...", nameof(StartAsync));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{MethodName}] Unhandled exception", nameof(StartAsync));
        }
        finally
        {
            if (_listener is { IsListening: true })
                _listener.Stop();
            
            _listener = null;
            _clients.Clear();
        }
    }

    public Task WaitingCompleteServerListening()
    {
        return _listeningHttpClient?.WaitAsync(_cts.Token) ?? Task.CompletedTask;
    }

    private async Task HandleClient(HttpListenerContext context, bool enableAuthentication)
    {
        WebSocket? ws = null;
        User? user = null;
        try
        {
            var wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
            ws = wsContext.WebSocket;

            user = await ProcessUserConnect(ws, enableAuthentication);
            if (user == null)
            {
                _logger.LogWarning("[{MethodName}] Error of getting connection user", nameof(HandleClient));
                await SendObjectToClient(ws, new ActionResult(ResultType.Error, "Error of getting connection user"));
                return;
            }

            if (!_clients.TryAdd(ws, user))
            {
                _logger.LogWarning("[{MethodName}] Client already connected: {Id} {UserName} ({Language})", nameof(HandleClient), user.Id,
                    user.UserName, user.Language);
                await SendObjectToClient(ws, new ActionResult(ResultType.Error, "Client already connected"));
                return;
            }
            
            _logger.LogInformation("[{MethodName}] Client connected: {Id} {UserName} ({Language})", nameof(HandleClient), user.Id,
                user.UserName, user.Language);
            var generalChatRoom = _chatRoomsDatabase.GetGeneralChatRoom();
            generalChatRoom?.AddParticipant(user);
            await SendChatsToClient(ws, user);
            await ListeningClientRequest(ws, user);
        }
        catch (Exception ex)
        {
            _logger.LogError("[{MethodName}] WebSocket error: {error}", nameof(HandleClient), ex.Message);
        }
        finally
        {
            if (ws != null)
            {
                _clients.TryRemove(ws, out user);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                _logger.LogInformation("[{MethodName}] Client disconnected. User id: {Id}, name: {UserName}", nameof(HandleClient), user?.Id ?? Guid.Empty, user?.UserName ?? "Anonymous");
                
                if (user != null)
                {
                    var message = new Message(user, receiver: null, $"User {user.UserName} disconnected", "en-US");
                    await BroadcastResult(new ActionResult(ResultType.Disconnect, message), user);
                    
                    foreach (var chatRoom in _chatRoomsDatabase.GetChatRooms().Where(s => s.ContainsUser(user.Id)))
                        chatRoom.RemoveParticipant(user);
                }
            }
        }
    }

    private async Task ListeningClientRequest(WebSocket ws, User user)
    {
        while (ws.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
        {
            try
            {
                var actionRequest = await ReceiveActionRequest(ws);
                RequestReceiver.Invoke(actionRequest, user);
                if (actionRequest.RequestType == RequestType.Disconnect) break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{MethodName}] Error receiving message", nameof(HandleClient));
            }
        }
    }

    private async Task SendChatsToClient(WebSocket ws, User user)
    {
        var chats = _chatRoomsDatabase.GetChatRoomsByParticipant(user);
        _logger.LogInformation("[{MethodName}] Count chats in server: {countChats}", nameof(SendChatsToClient), chats.Count);
        if (chats.Count < 1) return;

        await SendObjectToClient(ws, new ActionResult(ResultType.SendChats, chats));
    }

    private async Task<User?> ProcessUserConnect(WebSocket ws, bool enableAuthentication)
    {
        var actionRequest = await ReceiveActionRequest(ws);
        if (actionRequest.RequestType != RequestType.Connect)
            return null;
        
        ActionResult result;
        if (enableAuthentication) result = await AuthenticationRequest(ws);
        else result = await ConnectionRequest(ws);
        await SendObjectToClient(ws, result);
        if (IsActionResultError(result)
            || string.IsNullOrWhiteSpace(result.ResultData)
            || result.ResultDataType != typeof(User).FullName)
        {
            _logger.LogWarning("[{MethodName}] Error of processing connection user", nameof(ProcessUserConnect));
        }
        else
        {
            var user = JsonSerializer.Deserialize<User>(result.ResultData);
            return user;
        }

        return null;
    }

    private async Task<ActionResult> AuthenticationRequest(WebSocket ws)
    {
        var actionResult = new ActionResult(ResultType.NeedAuthentication);
        await SendObjectToClient(ws, actionResult);
        
        var actionRequest = await ReceiveActionRequest(ws);
        if (IsActionRequestError(actionRequest))
            return new ActionResult(actionRequest.RequestType == RequestType.Disconnect ? ResultType.Disconnect : ResultType.Error,
                actionRequest.RequestData);

        if (string.IsNullOrWhiteSpace(actionRequest.RequestData) ||
            actionRequest.RequestDataType != typeof(AuthUser).FullName)
            return new ActionResult(ResultType.Error, "Error convert auth user");
        
        var authUser = JsonSerializer.Deserialize<AuthUser>(actionRequest.RequestData);
        if (authUser == null)
            return new ActionResult(ResultType.Error, "Error convert auth user");

        ActionResult result;
        switch (actionRequest.RequestType)
        {
            case RequestType.Authorization:
            {
                var loginResult = await _authenticationService.LoginAsync(authUser);
                if (loginResult.ResultType == ResultType.SuccessAuthorized)
                {
                    result = string.IsNullOrWhiteSpace(loginResult.ResultData)
                        ? new ActionResult(ResultType.Error, "Error convert user")
                        : loginResult;
                }
                else
                {
                    result = new ActionResult(ResultType.Error, loginResult);
                }
                break;
            }
            case RequestType.Registration:
            {
                var registerResult = await _authenticationService.RegisterAsync(authUser);
                result = registerResult.ResultType != ResultType.SuccessRegistered
                    ? new ActionResult(ResultType.Error, registerResult.ResultData)
                    : registerResult;
                break;
            }
            default:
                result = new ActionResult(ResultType.Error, "Not supported request");
                break;
        }
        return result;
    }

    private async Task<ActionResult> ConnectionRequest(WebSocket ws)
    {
        var actionResult = new ActionResult(ResultType.NeedAuthentication);
        await SendObjectToClient(ws, actionResult);
        
        var actionRequest = await ReceiveActionRequest(ws);
        if (IsActionRequestError(actionRequest))
            return new ActionResult(actionRequest.RequestType == RequestType.Disconnect ? ResultType.Disconnect : ResultType.Error,
                actionRequest.RequestData);

        if (string.IsNullOrWhiteSpace(actionRequest.RequestData) ||
            actionRequest.RequestDataType != typeof(AuthUser).FullName)
            return new ActionResult(ResultType.Error, "Error convert auth user");

        ActionResult result;
        try
        {
            var authUser = JsonSerializer.Deserialize<AuthUser>(actionRequest.RequestData);
            if (authUser != null)
            {
                var user = new User(authUser.Login, authUser.Language);
                result = new ActionResult(ResultType.Connect, user);
            }
            else
            {
                result = new ActionResult(ResultType.Error, "Error convert user");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{MethodName}] Error convert auth user", nameof(ConnectionRequest));
            result = new ActionResult(ResultType.Error, "Error convert auth user");
        }
        return result;
    }

    private async Task<ActionRequest> ReceiveActionRequest(WebSocket ws)
    {
        var messageReceive = await ws.ReceiveFullMessageAsync(_bufferSize, _cts.Token);
        if (messageReceive.IsClosed)
            return new ActionRequest(RequestType.Disconnect, "User disconnected");
        if (string.IsNullOrWhiteSpace(messageReceive.Value))
            return new ActionRequest(RequestType.Error, "Error gets message");
        
        _logger.LogInformation("[{MethodName}] Received message: {json}", nameof(ReceiveActionRequest), messageReceive.Value);
        var actionRequest = JsonSerializer.Deserialize<ActionRequest>(messageReceive.Value);
        return actionRequest ?? new ActionRequest(RequestType.Error, "Error convert action request");
    }

    private static bool IsActionRequestError(ActionRequest request)
        => request.RequestType is RequestType.Error or RequestType.Disconnect;

    private static bool IsActionResultError(ActionResult result)
        => result.ResultType is ResultType.Error or ResultType.Disconnect;
    
    private async Task SendObjectToClient(WebSocket ws, object obj)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj, _jsonOptions);
            _logger.LogInformation("[{MethodName}] Sending action to client: {json}", nameof(SendObjectToClient), json);
            var buffer = Encoding.UTF8.GetBytes(json);
            await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{MethodName}] Error sending message", nameof(SendObjectToClient));
        }
    }
    
    public bool IsServerStarted() => _listener is { IsListening: true };

    public async Task StopAsync()
    {
        if (_listener == null) return;
        
        _logger.LogInformation("[{MethodName}] Stopping listening", nameof(StopAsync));
        await _cts.CancelAsync();

        if (_listener is { IsListening: true })
            _listener.Stop();
        
        _listener = null;

        foreach (var ws in _clients.Select(s => s.Key))
        {
            if (ws.State == WebSocketState.Open)
            {
                await SendObjectToClient(ws, new ActionResult(ResultType.Disconnect, "Server is shutting down"));
            }
        }

        _clients.Clear();
        _logger.LogInformation("[{MethodName}] Server stopped", nameof(StopAsync));
    }

    private async void OnRequestReceiver(ActionRequest actionRequest, User user)
    {
        if (IsActionRequestError(actionRequest))
            return;

        var actionResult = PrepareRequest(actionRequest, user);
        if (actionResult != null) await BroadcastResult(actionResult, user);
    }
    
    /// <summary>
    /// Get original message and if needed to translate original content to language of user 
    /// </summary>
    /// <param name="message">Message of user</param>
    /// <param name="user">User of websocket</param>
    /// <returns>Outgoing message that sending to clients</returns>
    private async Task<Message> GetOutgoingMessage(Message message, User user)
    {
        var outgoingMessage = message.Clone();
        if (message.OriginalContent != null)
        {
            var translatedContent = await _languagesService.TranslateAsync(message.OriginalContent, message.OriginalLanguage, user.Language);
            outgoingMessage.SetTranslatedContent(translatedContent);
        }
        return outgoingMessage;
    }
}