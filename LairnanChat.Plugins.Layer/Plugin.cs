using LairnanChat.Plugins.Layer.Implements.Services;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using LairnanChat.PluginsSetup;
using Microsoft.Extensions.DependencyInjection;

namespace LairnanChat.Plugins.Layer;

public class Plugin : IPlugin
{
    public void Setup(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IAuthenticationService, NoneAuthenticationService>();
        serviceCollection.AddSingleton<IChatRoomsDatabase, ChatRoomsDatabase>();
        serviceCollection.AddTransient<ILanguageTranslationService, LanguageTranslationService>();
        serviceCollection.AddSingleton<IChatServer, ChatServer>();
        serviceCollection.AddTransient<IChatService, ChatService>();
        serviceCollection.AddScoped<IChatServerManager, ChatServerManager>();
        serviceCollection.AddLogging();
    }
}