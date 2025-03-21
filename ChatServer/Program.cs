using ChatServer.Services;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using LairnanChat.PluginsSetup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ChatServer
{
    internal class Program
    {
        private static IServiceProvider _serviceProvider;
        private static ILogger<Program> _logger;

        public static async Task Main(string[] args)
        {
            LoadIoc();

            _logger.LogInformation("Starting ChatServer...");
            try
            {
                await StartServer();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Fatal error occurred while starting the server.");
            }
        }

        private static void LoadIoc()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var pluginPatterns = configuration.GetValue<string[]>("PluginsAssemblyNamePatterns") 
                                 ?? ["LairnanChat.Plugins."];

            services.AddSingleton(configuration);
            services.LoadPlugins(pluginPatterns);
            services.AddTransient<IAuthenticationService, AuthenticationService>();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(logger);
            });

            _serviceProvider = services.BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<Program>>();
        }

        private static async Task StartServer()
        {
            var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
            _logger.LogInformation("Server initialized with authentication service: {Service}", authService.GetType().Name);

            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            var url = configuration.GetValue<string?>("WebSocketServerURL");
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogError("WebSocketServerURL cannot be null");
                return;
            }
            var server = _serviceProvider.GetRequiredService<IChatServer>();
            await server.StartAsync(url);
            if (!server.IsServerStarted()) return;
            await server.WaitingCompleteServerListening();
        }
    }
}
