using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces.Services;

namespace LairnanChat.Plugins.Layer.Interfaces;

public interface IChatServiceFactory
{
    IChatService Create(ChatServerInfo serverInfo);
}