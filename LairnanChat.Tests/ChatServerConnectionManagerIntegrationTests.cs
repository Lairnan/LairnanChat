using System.Net.WebSockets;
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
public class ChatServerConnectionManagerIntegrationTests
{
    private IServiceProvider _serviceProvider = null!;
    private IChatServerManager _connectionManager = null!;
    private IChatServer _chatServer1 = null!;
    private IChatServer _chatServer2 = null!;
    private const string ConnectServerUrl1 = "ws://localhost:8081/ws/";
    private const string ConnectServerUrl2 = "ws://localhost:8082/ws/";

    [TestInitialize]
    public async Task TestSetup()
    {
        const string json = """
                            {
                              "WebSocketServerURL": "http://localhost:8081/ws/",
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
        services.AddSingleton<IChatServerManager, ChatServerManager>();

        _serviceProvider = services.BuildServiceProvider();
        _connectionManager = _serviceProvider.GetRequiredService<IChatServerManager>();

        _chatServer1 = _serviceProvider.GetRequiredService<IChatServer>();
        _chatServer2 = _serviceProvider.GetRequiredService<IChatServer>();

        await _chatServer1.StartAsync("http://localhost:8081/ws/", true);
        await _chatServer2.StartAsync("http://localhost:8082/ws/", true);
    }

    [TestMethod]
    public async Task ConnectAsync_WithAuthentication_ShouldConnectSuccessfully()
    {
        var authUser = new AuthUser("TestUser", "TestPassword", "ru-RU");
        var service = await _connectionManager.ConnectAsync(ConnectServerUrl1, authUser, false);

        Assert.IsNotNull(service);
        Assert.AreEqual(WebSocketState.Open, service.GetConnectionStatus());
    }

    [TestMethod]
    public async Task ConnectAsync_MultipleServers_SendMessagesToDifferentServers()
    {
        var authUser1 = new AuthUser("User1", "Pass1", "ru-RU");
        var authUser2 = new AuthUser("User2", "Pass2", "ru-RU");

        var service1 = await _connectionManager.ConnectAsync(ConnectServerUrl1, authUser1, false);
        var service2 = await _connectionManager.ConnectAsync(ConnectServerUrl2, authUser2, false);

        Assert.IsNotNull(service1);
        Assert.IsNotNull(service2);

        var message1 = new Message(service1.ConnectedUser!, receiver: null, "Hello Server1", "ru-RU");
        var request1 = new ActionRequest(RequestType.SendMessage, message1);
        await service1.SendRequestAsync(request1);

        var message2 = new Message(service2.ConnectedUser!, receiver: null, "Hello Server2", "ru-RU");
        var request2 = new ActionRequest(RequestType.SendMessage, message2);
        await service2.SendRequestAsync(request2);

        Assert.AreEqual(WebSocketState.Open, service1.GetConnectionStatus());
        Assert.AreEqual(WebSocketState.Open, service2.GetConnectionStatus());
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await _connectionManager.DisconnectAsync(ConnectServerUrl1);
        await _connectionManager.DisconnectAsync(ConnectServerUrl2);

        await _chatServer1.StopAsync();
        await _chatServer2.StopAsync();
        
        _chatServer1 = null!;
        _chatServer2 = null!;
    }
}