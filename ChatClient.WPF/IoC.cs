using System.IO;
using ChatClient.WPF.Controls;
using ChatClient.WPF.Interfaces;
using ChatClient.WPF.Interfaces.ViewModels.Pages;
using ChatClient.WPF.Interfaces.ViewModels.Windows;
using ChatClient.WPF.Interfaces.Views.Pages;
using ChatClient.WPF.Interfaces.Views.Windows;
using ChatClient.WPF.Services;
using ChatClient.WPF.Services.Implementation;
using ChatClient.WPF.Services.Interface;
using ChatClient.WPF.ViewModels;
using ChatClient.WPF.ViewModels.Pages;
using ChatClient.WPF.Views;
using ChatClient.WPF.Views.Implementations;
using ChatClient.WPF.Views.Pages;
using LairnanChat.PluginsSetup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ChatClient.WPF;

public static class IoC
{
    private static readonly IServiceProvider _provider;

    static IoC()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true);

        var configuration = builder.Build();

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IConfiguration>(configuration);
        
        var pluginPatterns = configuration.GetValue<string[]>("PluginsAssemblyNamePatterns") 
                             ?? ["LairnanChat.Plugins."];

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(logger);
        });
        services.LoadPlugins(pluginPatterns);

        services.AddWindows()
                .AddPages();
        
        // Services
        services.AddTransient<IPageService, PageService>();
        services.AddSingleton<IViewFactory, ViewFactory>();

        _provider = services.BuildServiceProvider();
        //foreach (var service in services.Where(s => s.Lifetime == ServiceLifetime.Singleton))
            //_provider.GetRequiredService(service.ServiceType);
    }

    private static IServiceCollection AddWindows(this IServiceCollection services)
    {
        services.AddTransient<IMainWindowVm, MainViewModel>();
        services.AddTransient<IAdditionalWindowVm, AdditionalViewModel>();
        
        services.AddTransient<IWindowBase, WindowBase>();
        
        services.AddTransient<IMainWindow, MainWindow>();
        services.AddTransient<IAdditionalWindow, AdditionalWindow>();
        
        return services;
    }

    private static IServiceCollection AddPages(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationPageVm, AuthenticationPageVm>();
        services.AddTransient<IPageBase, PageBase>();
        services.AddTransient<IPopUp, PopUpControl>();
        services.AddTransient<IAuthenticationPage, AuthenticationPage>();

        return services;
    }

    public static T Resolve<T>() where T : notnull
        => _provider.GetRequiredService<T>();
}