namespace LairnanChat.Plugins.Layer.Interfaces.Services;

public interface IChatServer
{
    /// <summary>
    /// Starts the chat server asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    Task StartAsync(string url, bool enableAuthenticationService = false);

    /// <summary>
    /// Waiting for complete server after starting.
    /// </summary>
    /// <returns>A task that represents the server operations. If server not starter, return Task.CompletedTask</returns>
    Task WaitingCompleteServerListening();

    /// <summary>
    /// Stops the chat server asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    Task StopAsync();

    /// <summary>
    /// Flag server is started
    /// </summary>
    /// <returns>True if server is started</returns>
    bool IsServerStarted();
}