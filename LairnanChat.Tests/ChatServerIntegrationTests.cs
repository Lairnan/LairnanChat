using System.Text;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Implements.Services;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;

namespace LairnanChat.Tests;

[TestClass]
public class ChatServerIntegrationTests
{
    private IServiceProvider _serviceProvider = null!;
    private IChatServerManager _connectionManager = null!;
    private readonly List<IChatServerManager> _serverManagers = [];
    private IChatServer _chatServer = null!;
    private const string ConnectServerUrl = "ws://localhost:8080/ws/";
    private IChatRoomsDatabase _chatRoomsDatabase;
    
    [TestInitialize]
    public async Task TestSetup()
    {
        const string json = """
                            {
                              "WebSocketServerURL": "http://localhost:8080/ws/",
                              "BufferSize": "8096",
                              "PluginsAssemblyNamePatterns": [ "LairnanChat.Plugins." ],
                              "GeneralChatName": "LairnanChat General"
                            }
                            """;
        
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        
        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<AuthUser>()))
            .ReturnsAsync((AuthUser authUser) => 
                new ActionResult(ResultType.SuccessAuthorized, new User(authUser.Login, authUser.Language))
            );
        mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<AuthUser>()))
            .ReturnsAsync((AuthUser authUser) => 
                new ActionResult(ResultType.SuccessRegistered, new User(authUser.Login, authUser.Language))
            );
        services.AddSingleton<IAuthenticationService>(mockAuthService.Object);

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(logger);
        });

        services.AddTransient<ILanguageTranslationService, LanguageTranslationService>();
        services.AddSingleton<IChatRoomsDatabase, ChatRoomsDatabase>();
        services.AddTransient<IChatService, ChatService>();
        services.AddTransient<IChatServer, ChatServer>();
        services.AddTransient<IChatServerManager, ChatServerManager>();

        _serviceProvider = services.BuildServiceProvider();
        _chatRoomsDatabase = _serviceProvider.GetRequiredService<IChatRoomsDatabase>();
        _connectionManager = _serviceProvider.GetRequiredService<IChatServerManager>();

        _chatServer = _serviceProvider.GetRequiredService<IChatServer>();

        await _chatServer.StartAsync("http://localhost:8080/ws/", true);
        for (var i = 0; i < 3; i++)
        {
            var connectionManager = _serviceProvider.GetRequiredService<IChatServerManager>();
            var chatService = await connectionManager.ConnectAsync(ConnectServerUrl, new AuthUser($"UserTest{i+2}", "password123"), false);
            Assert.IsNotNull(chatService);
            _serverManagers.Add(connectionManager);
        }
    }
    
    [TestCleanup]
    public async Task TestCleanup()
    {
        await _connectionManager.DisconnectAsync(ConnectServerUrl);
        foreach (var chatServerManager in _serverManagers)
        {
            await chatServerManager.DisconnectActiveAsync();
        }
        _serverManagers.Clear();

        await _chatServer.StopAsync();
        
        _chatServer = null!;
    }
    
    [TestMethod]
    [DataRow(false, RequestType.Connect, DisplayName = "ChatServer_Connect_WithoutAuthenticate")]
    [DataRow(true, RequestType.Authorization, DisplayName = "ChatServer_Connect_Authenticate_ConnectAuthorization")]
    [DataRow(true, RequestType.Registration, DisplayName = "ChatServer_Connect_Authenticate_ConnectRegistrationAndAuthentication")]
    public async Task ChatServer_Connect_Authenticate(bool enableAuth, RequestType requestType)
    {
        // Arrange
        var authUser = new AuthUser("UserTest1", "password123", "en-US");

        // Act
        var chatService = await _connectionManager.ConnectAsync(ConnectServerUrl, authUser, requestType == RequestType.Registration);
        Assert.IsNotNull(chatService);

        // Assert
        Assert.IsNotNull(chatService);
        Assert.IsNotNull(chatService.ConnectedUser);
    }
    
    [TestMethod]
    [DataRow("Hello Chat!", null, "PrivateChat", RequestType.SendMessage, DisplayName = "ChatServer_SendMessage_ToChat_Private")]
    [DataRow("Hello Public Chat!", null, "LairnanChat General", RequestType.SendMessage, DisplayName = "ChatServer_SendMessage_ToChat_Public")]
    [DataRow("Hello Receiver!", "UserTest2", null, RequestType.SendMessage, DisplayName = "ChatServer_SendMessage_Private_Message")]
    public async Task ChatServer_SendMessage(string messageText, string? receiverName, string? chatRoomName, RequestType requestType)
    {
        // Arrange
        ResultType expectedResultType = ResultType.SendMessage;
        var authUser = new AuthUser("UserTest1", "password123", "en-US");
        var chatService = await _connectionManager.ConnectAsync(ConnectServerUrl, authUser);
        Assert.IsNotNull(chatService);
        Assert.IsNotNull(chatService.ConnectedUser);

        var message = new Message(chatService.ConnectedUser!, receiver: null, "en-US", messageText);
        if (!string.IsNullOrWhiteSpace(receiverName))
        {
            
            var receiverServerManager = _serverManagers.FirstOrDefault(s => s.ActiveChatService?.ConnectedUser?.UserName == receiverName);
            Assert.IsNotNull(receiverServerManager);
            message.ChangeReceiver(receiverServerManager.ActiveChatService!.ConnectedUser!);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(chatRoomName)) chatRoomName = _chatRoomsDatabase.GetGeneralChatRoom()!.RoomName;
            var chatRoom = _chatRoomsDatabase.GetChatRooms().FirstOrDefault(s => s.RoomName == chatRoomName);
            if (chatRoom == null)
            {
                chatRoom = new ChatRoom(chatRoomName);
                _chatRoomsDatabase.AddChatRoom(chatRoom);
            }
            message.ChangeReceiver(chatRoom.Id);
        }

        var request = new ActionRequest(requestType, message);

        ActionResult? receivedResult = null;
        chatService.MessageReceived += (_, result) =>
        {
            if (result.ResultType == expectedResultType)
            {
                receivedResult = result;
            }
        };

        // Act
        await chatService.SendRequestAsync(request);

        // Wait for message to arrive
        await WaitUntilAsync(() => receivedResult != null, timeout: 5000);

        // Assert
        Assert.IsNotNull(receivedResult);
        Assert.AreEqual(expectedResultType, receivedResult.ResultType);
        AssertMessage(message, expectedResultType);
    }

    private void AssertMessage(Message message, ResultType expectedResultType)
    {
        ChatRoom? chatRoom = null;
        if (message.ChatRoomId != null)
        {
            chatRoom = _chatRoomsDatabase.GetChatRoomById(message.ChatRoomId.Value);
        }
        
        foreach (var serverManager in _serverManagers)
        {
            Assert.IsNotNull(serverManager.ActiveChatService);
            var chatService = serverManager.ActiveChatService;
            Assert.IsNotNull(chatService.ConnectedUser);
            Assert.IsNotNull(chatService.ReceivedResults);
            
            var actionResult = chatService.ReceivedResults.FirstOrDefault(s =>
                s.Data is Message mess && mess.MessageId == message.MessageId);
            if (message.Receiver != null)
            {
                if (message.Receiver.Id == chatService.ConnectedUser.Id)
                {
                    Assert.IsNotNull(actionResult);
                    Assert.AreEqual(expectedResultType, actionResult.ResultType);
                }
                else
                {
                    Assert.IsNull(actionResult);
                }
            }
            else
            {
                Assert.IsNotNull(chatRoom);
                if (chatRoom.ContainsUser(chatService.ConnectedUser.Id))
                {
                    Assert.IsNotNull(actionResult);
                    Assert.AreEqual(expectedResultType, actionResult.ResultType);
                }
                else
                {
                    Assert.IsNull(actionResult);
                }
            }
        }
    }
    
    private async Task WaitUntilAsync(Func<bool> condition, int timeout)
    {
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if ((DateTime.UtcNow - start).TotalMilliseconds > timeout)
                break;
            await Task.Delay(50);
        }
    }
}