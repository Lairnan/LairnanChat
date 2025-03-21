using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LairnanChat.PluginsSetup;

public static class PluginLoader
{
    public static IServiceCollection LoadPlugins(this IServiceCollection? serviceCollection, string[] pluginsPatterns)
    {
        serviceCollection ??= new ServiceCollection();

        var plugins = GetPlugins(pluginsPatterns);

        foreach (var plugin in plugins)
        {
            try
            {
                plugin.Setup(serviceCollection);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        return serviceCollection;
    }

    public static IEnumerable<IPlugin> GetPlugins(string[] pluginsPatterns)
    {
        var pluginType = typeof(IPlugin);
        var plugins = new List<IPlugin>();
        LoadDll(pluginsPatterns);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                     .Where(s => pluginsPatterns.Any(pattern => s.FullName?.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) ?? false)))
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types?.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (!pluginType.IsAssignableFrom(type) || type is not { IsInterface: false, IsAbstract: false })
                    continue;

                if (Activator.CreateInstance(type) is not IPlugin plugin)
                    continue;
                
                plugins.Add(plugin);
                break;
            }
        }
        
        return plugins;
    }

    private static void LoadDll(string[] pluginsPatterns)
    {
        foreach (var plugins in pluginsPatterns)
        {
            foreach (var dllPath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, plugins + "*.dll", SearchOption.AllDirectories))
            {
                Assembly.LoadFrom(dllPath);
            }
        }
    }
}