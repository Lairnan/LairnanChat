using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Implements.Services;
using LairnanChat.Plugins.Layer.Interfaces;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LairnanChat.Plugins.Layer.Implements;

public class ChatServiceFactory : IChatServiceFactory
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<ChatServiceFactory> _logger;

    public ChatServiceFactory(ILogger<ChatServiceFactory> logger, IServiceProvider provider)
    {
        _provider = provider;
        _logger = logger;
        _logger.LogInformation("{className} initialized", nameof(ChatServiceFactory));
    }

    public IChatService Create(ChatServerInfo serverInfo)
    {
        _logger.LogInformation("[{methodName}] Trying create chat service with server info: {serverInfo}", nameof(Create), serverInfo.DisplayName);
        var logger = _provider.GetRequiredService<ILogger<ChatService>>();
        return new ChatService(logger, serverInfo);
    }
}