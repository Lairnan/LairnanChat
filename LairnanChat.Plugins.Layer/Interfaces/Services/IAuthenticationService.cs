using LairnanChat.Plugins.Layer.Implements;
using LairnanChat.Plugins.Layer.Implements.Models;

namespace LairnanChat.Plugins.Layer.Interfaces.Services;

public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new authUser asynchronously.
    /// </summary>
    /// <param name="authUser">The authUser for registration.</param>
    /// <returns>A task that represents the asynchronous registration operation.</returns>
    Task<ActionResult> RegisterAsync(AuthUser authUser);

    /// <summary>
    /// Logs in a authUser asynchronously.
    /// </summary>
    /// <param name="authUser">The authUser for authorization.</param>
    /// <returns>A task that represents the asynchronous login operation.</returns>
    Task<ActionResult> LoginAsync(AuthUser authUser);
}