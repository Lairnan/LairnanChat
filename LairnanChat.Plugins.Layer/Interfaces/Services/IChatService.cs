using System.Net.WebSockets;
using LairnanChat.Plugins.Layer.Implements;
using LairnanChat.Plugins.Layer.Implements.Models;

namespace LairnanChat.Plugins.Layer.Interfaces.Services;

public interface IChatService
{
    /// <summary>
    /// Gets url address of connected server
    /// </summary>
    string? Url { get; }
    
    IList<ActionResult>? ReceivedResults { get; }
    
    /// <summary>
    /// Occurs when a new message is received.
    /// </summary>
    event EventHandler<ActionResult> MessageReceived;
    
    /// <summary>
    /// Gets the connected user to server from this chat service
    /// </summary>
    User? ConnectedUser { get; }

    /// <summary>
    /// Sends the specified request asynchronously.
    /// </summary>
    /// <param name="actionRequest">The request to send.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendRequestAsync(ActionRequest actionRequest);

    /// <summary>
    /// Gets connection status
    /// </summary>
    /// <returns>Connection status</returns>
    WebSocketState GetConnectionStatus();

    /// <summary>
    /// Connects to the chat server asynchronously.
    /// </summary>
    /// <param name="url">Url to connect to chat server.</param>
    /// <param name="authUser">User that connect to server.</param>
    /// <param name="needRegistration">
    /// If authUser already registered, then authorization to server.
    /// If server not accept authorization system, this flag not included
    /// </param>
    /// <returns>A task that represents the asynchronous connect operation.</returns>
    Task<ActionResult> ConnectAsync(string url, AuthUser authUser, bool needRegistration = false);
    Task<ActionResult> ConnectAsync(AuthUser authUser, bool needRegistration = false);

    void SetUrlServer(string url);

    /// <summary>
    /// Disconnects from the chat server asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous disconnect operation.</returns>
    Task DisconnectAsync();
}