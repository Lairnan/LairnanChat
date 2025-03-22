using System.Collections.ObjectModel;
using System.Net.WebSockets;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LairnanChat.Plugins.Layer.Implements.Services
{
    public class ChatServerManager : IChatServerManager
    {
        public IChatService? ActiveChatService { get; private set; }

        public event Action<IChatService?> ActiveChatServiceChanged = delegate { };

        public bool AutoRedirectOnDisconnect { get; set; } = true;

        private readonly ILogger<ChatServerManager> _logger;
        private readonly IServiceProvider _provider;
        private readonly List<IChatService> _chatServices = [];

        public ChatServerManager(ILogger<ChatServerManager> logger, IConfiguration config, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
            AvailableServers = new ReadOnlyObservableCollection<ChatServerInfo>(_availableServers);
            
            var servers = config.GetSection("ServersIp").Get<List<ChatServerInfo>>() ?? [];
            foreach (var server in servers)
            {
                TryGetOrAddServer(server.Name, server.Url);
            }
            
            ActiveChatServiceChanged += chatService => ActiveChatService = chatService;
        }

        private readonly ObservableCollection<ChatServerInfo> _availableServers = [];
        public ReadOnlyObservableCollection<ChatServerInfo> AvailableServers { get; }

        public void SetActiveServer(ChatServerInfo? serverInfo)
        {
            if (serverInfo == null)
            {
                ActiveChatServiceChanged(null);
                return;
            }

            if (ActiveChatService != null && (ActiveChatService.ServerInfo == serverInfo || ActiveChatService.ServerInfo.Url == serverInfo.Url))
                return;
            
            var chatService = TryGetOrAddServer(serverInfo);
            ActiveChatServiceChanged(chatService);
        }

        public async Task<IChatService?> ConnectAsync(AuthUser authUser, bool registerNewUser = false)
        {
            if (ActiveChatService != null) return await ConnectAsync(ActiveChatService, authUser, registerNewUser);
            
            var server = _availableServers.LastOrDefault();
            if (server == null)
            {
                _logger.LogError("[{methodName}] No servers in list", nameof(ConnectAsync));
                return null;
            }

            return await ConnectAsync(server.Url, authUser, registerNewUser);
        }

        public async Task<IChatService?> ConnectAsync(string url, AuthUser authUser, bool registerNewUser = false)
        {
            var serverInfo = _availableServers.FirstOrDefault(s => s.Url == url) ?? new ChatServerInfo("Custom", url, false);

            var chatService = TryGetOrAddServer(serverInfo);
            return await ConnectAsync(chatService, authUser, registerNewUser);
        }

        public async Task<IChatService?> ConnectAsync(IChatService? chatService, AuthUser authUser, bool registerNewUser = false)
        {
            if (chatService == null)
            {
                _logger.LogError("[{methodName}] Need select server for connect", nameof(ConnectAsync));
                return null;
            }

            var result = await chatService.ConnectAsync(authUser, registerNewUser);
            if (result.ResultType != ResultType.Connect &&
                result.ResultType != ResultType.SuccessAuthorized &&
                result.ResultType == ResultType.SuccessRegistered)
            {
                _logger.LogError("[{methodName}] Error in connection to server. {resultData}", nameof(ConnectAsync), result.ResultData);
                return null;
            }

            ActiveChatServiceChanged(chatService);
            return chatService;
        }

        public async Task DisconnectActiveAsync()
        {
            if (ActiveChatService != null)
            {
                await DisconnectAsync(ActiveChatService);
            }
        }

        public async Task DisconnectAsync(string url)
        {
            var chatService = _chatServices.FirstOrDefault(s => s.ServerInfo!.Url == url);
            if (chatService != null)
            {
                await DisconnectAsync(chatService);
            }
        }

        private async Task DisconnectAsync(IChatService chatService)
        {
            await chatService.DisconnectAsync();
            if (ActiveChatService == chatService)
            {
                if (AutoRedirectOnDisconnect)
                    RedirectLastServer();
                else 
                    ActiveChatServiceChanged(null);
            }
        }
        
        public IChatService TryGetOrAddServer(string name, string url)
        {
            var serverInfo = new ChatServerInfo(name, url, false);
            return TryGetOrAddServer(serverInfo);
        }
        
        public IChatService TryGetOrAddServer(ChatServerInfo serverInfo)
        {
            var chatService = _chatServices.FirstOrDefault(s => s.ServerInfo == serverInfo || s.ServerInfo.Url == serverInfo.Url);
            if (chatService != null) return chatService;
            
            chatService = _provider.GetRequiredService<IChatServiceFactory>().Create(serverInfo);
            _chatServices.Add(chatService);
            _availableServers.Add(serverInfo);
            return chatService;
        }

        public void RemoveServer(string url)
        {
            var chatService = _chatServices.FirstOrDefault(s => s.ServerInfo.Url == url);
            if (chatService != null)
            {
                if (chatService.GetConnectionStatus() == WebSocketState.Open)
                {
                    _logger.LogError("[{methodName}] Cannot remove connected server. {url}", nameof(RemoveServer), url);
                    return;
                }
                
                if (ActiveChatService == chatService)
                    ActiveChatServiceChanged(null);

                _chatServices.Remove(chatService);
                _availableServers.Remove(chatService.ServerInfo!);
            }
        }

        private void RedirectLastServer()
        {
            var serverInfo = _availableServers.LastOrDefault();
            SetActiveServer(serverInfo);
        }
    }
}
