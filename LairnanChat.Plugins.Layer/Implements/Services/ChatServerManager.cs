using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
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
        private readonly List<ChatServerInfo> _servers;
        private readonly List<IChatService> _chatServices = [];

        public ChatServerManager(ILogger<ChatServerManager> logger, IConfiguration config, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
            _servers = config.GetSection("ServersIp").Get<List<ChatServerInfo>>() ?? [];
            ActiveChatServiceChanged += chatService => ActiveChatService = chatService;
        }

        public IEnumerable<ChatServerInfo> AvailableServers => _servers.AsReadOnly();

        public void SetActiveServer(ChatServerInfo serverInfo)
        {
            if (serverInfo == null)
            {
                ActiveChatServiceChanged(null);
                return;
            }

            if (_servers.All(s => s.Url != serverInfo.Url))
                _servers.Add(serverInfo);

            if (ActiveChatService?.Url == serverInfo.Url)
                return;

            var chatService = _chatServices.FirstOrDefault(s => s.Url == serverInfo.Url);
            if (chatService == null)
            {
                chatService = _provider.GetRequiredService<IChatService>();
                chatService.SetUrlServer(serverInfo.Url);
                _chatServices.Add(chatService);
            }

            ActiveChatServiceChanged(chatService);
        }

        public async Task<IChatService?> ConnectAsync(AuthUser authUser, bool registerNewUser = false)
        {
            var server = _servers.LastOrDefault();
            if (server == null)
                return null;

            return await ConnectAsync(server.Url, authUser, registerNewUser);
        }

        public async Task<IChatService?> ConnectAsync(string url, AuthUser authUser, bool registerNewUser = false)
        {
            if (_servers.All(s => s.Url != url))
                _servers.Add(new ChatServerInfo("Custom", url, false));

            var chatService = _chatServices.FirstOrDefault(s => s.Url == url);
            if (chatService == null)
            {
                chatService = _provider.GetRequiredService<IChatService>();
                chatService.SetUrlServer(url);
                _chatServices.Add(chatService);
            }

            var result = await chatService.ConnectAsync(authUser, registerNewUser);
            if (result.ResultType != ResultType.Connect &&
                result.ResultType != ResultType.SuccessAuthorized &&
                result.ResultType == ResultType.SuccessRegistered)
                return null;

            ActiveChatServiceChanged(chatService);
            return chatService;
        }

        public async Task<IChatService?> ConnectAsync(IChatService? chatService, AuthUser authUser, bool registerNewUser = false)
        {
            if (chatService == null)
                return null;

            var result = await chatService.ConnectAsync(authUser, registerNewUser);
            if (result.ResultType != ResultType.Connect &&
                result.ResultType != ResultType.SuccessAuthorized &&
                result.ResultType == ResultType.SuccessRegistered)
                return null;

            if (!_chatServices.Contains(chatService))
                _chatServices.Add(chatService);

            ActiveChatServiceChanged(chatService);
            return chatService;
        }

        public async Task DisconnectActiveAsync()
        {
            if (ActiveChatService != null)
            {
                await ActiveChatService.DisconnectAsync();
                ActiveChatServiceChanged(null);
                if (AutoRedirectOnDisconnect)
                    RedirectLastServer();
            }
        }

        public async Task DisconnectAsync(string url)
        {
            var chatService = _chatServices.FirstOrDefault(s => s.Url == url);
            if (chatService != null)
            {
                await chatService.DisconnectAsync();
                if (ActiveChatService?.Url == url)
                {
                    ActiveChatServiceChanged(null);
                    if (AutoRedirectOnDisconnect)
                        RedirectLastServer();
                }
            }
        }


        public void AddServer(string name, string url)
        {
            if (_servers.All(s => s.Url != url))
                _servers.Add(new ChatServerInfo(name, url, false));
        }

        public void RemoveServer(string url)
        {
            var server = _servers.FirstOrDefault(s => s.Url == url);
            if (server != null)
            {
                _servers.Remove(server);
                if (ActiveChatService?.Url == server.Url)
                    RedirectLastServer();
            }
        }

        private void RedirectLastServer()
        {
            var fallback = _servers.LastOrDefault();
            if (fallback != null)
                SetActiveServer(fallback);
        }
    }
}
