using Microsoft.Extensions.DependencyInjection;

namespace LairnanChat.PluginsSetup;

public interface IPlugin
{
    /// <summary>
    /// Register interfaces of models in service collection main program
    /// </summary>
    /// <param name="serviceCollection">Getting service collection from program</param>
    void Setup(IServiceCollection serviceCollection);
}