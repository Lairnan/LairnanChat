using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces.Services;

namespace ChatServer.Services;

public class AuthenticationService : IAuthenticationService
{
    public async Task<ActionResult> RegisterAsync(AuthUser authUser)
    {
        var user = new User(authUser.Login, authUser.Language);
        return new ActionResult(ResultType.SuccessRegistered, user);
    }

    public async Task<ActionResult> LoginAsync(AuthUser authUser)
    {
        var user = new User(authUser.Login, authUser.Language);
        return new ActionResult(ResultType.SuccessAuthorized, user);
    }
}