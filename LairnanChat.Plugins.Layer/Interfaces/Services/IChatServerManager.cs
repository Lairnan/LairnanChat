using System.Collections.ObjectModel;
using LairnanChat.Plugins.Layer.Implements.Models;

namespace LairnanChat.Plugins.Layer.Interfaces.Services;

/// <summary>
/// Manages available chat servers, connections, and switching between active chat services.
/// </summary>
public interface IChatServerManager
{
    IChatService? ActiveChatService { get; }
    
    event Action<IChatService?> ActiveChatServiceChanged;
    
    bool AutoRedirectOnDisconnect { get; set; }
    
    /// <summary>
    /// Provides a read-only list of available chat servers.
    /// </summary>
    ReadOnlyObservableCollection<ChatServerInfo> AvailableServers { get; }
    
    /// <summary>
    /// Sets the active chat service using ChatServerInfo without connecting.
    /// </summary>
    void SetActiveServer(ChatServerInfo? serverInfo);
    
    /// <summary>
    /// Connects using the provided chat service instance and authenticates the user.
    /// Sets the chat service as active upon successful connection.
    /// </summary>
    Task<IChatService?> ConnectAsync(IChatService chatService, AuthUser authUser, bool registerNewUser = false);

    /// <summary>
    /// Connects to the chat server by URL and authenticates the user.
    /// Adds the server to the available servers list automatically if it's new.
    /// Sets the connection as active upon success.
    /// </summary>
    Task<IChatService?> ConnectAsync(string url, AuthUser authUser, bool registerNewUser = false);

    /// <summary>
    /// Connects and authenticates the user using the last active chat server.
    /// Sets the connection as active if the connection is successful.
    /// </summary>
    Task<IChatService?> ConnectAsync(AuthUser authUser, bool registerNewUser = false);
    
    /// <summary>
    /// Disconnects the active server and optionally redirects to the previously connected server.
    /// </summary>
    Task DisconnectActiveAsync();
    
    /// <summary>
    /// Disconnects the active server and optionally redirects to the previously connected server.
    /// </summary>
    Task DisconnectAsync(string url);
    
    /// <summary>
    /// Adds a new server.
    /// </summary>
    IChatService TryGetOrAddServer(string name, string url);
    IChatService TryGetOrAddServer(ChatServerInfo serverInfo);
    
    /// <summary>
    /// Removes a server by URL.
    /// </summary>
    void RemoveServer(string url);
}