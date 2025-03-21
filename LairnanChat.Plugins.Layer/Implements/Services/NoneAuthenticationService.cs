using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces.Services;

namespace LairnanChat.Plugins.Layer.Implements.Services;

public class NoneAuthenticationService : IAuthenticationService
{
    public Task<ActionResult> RegisterAsync(AuthUser authUser)
    {
        var user = new User(authUser.Login, authUser.Language);
        return Task.FromResult(new ActionResult(ResultType.SuccessRegistered, user));
    }

    public Task<ActionResult> LoginAsync(AuthUser authUser)
    {
        var user = new User(authUser.Login, authUser.Language);
        return Task.FromResult(new ActionResult(ResultType.SuccessAuthorized, user));
    }
}