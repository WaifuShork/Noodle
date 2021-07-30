using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noodle.Services;
using Serilog;
using Serilog.Events;

namespace Noodle
{
    internal static class NoodleHost
    {
        public static async Task<int> RunAsync()
        {
            var path = Path.Combine("assets", "logs", "log-.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .WriteTo.Async(x =>
                {
                    x.Console();
                    x.File(path, LogEventLevel.Verbose, shared: true, rollingInterval: RollingInterval.Day);
                })
                .CreateLogger();
            
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, args) => OnUnhandledException(sender, args);
                
                Log.Information("Starting Noodle");
                using var host = CreateDefaultBuilder().Build();
                await host.RunAsync();
                return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateDefaultBuilder()
        {
            return Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost((context, config) => 
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        RateLimitPrecision = RateLimitPrecision.Millisecond,
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                        ExclusiveBulkDelete = false
                    };
                    
                    config.Token = context.Configuration["token"];
                })
                .UseCommandService((_, config) =>
                {
                    config.ThrowOnError = true;
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Verbose;
                    config.DefaultRunMode = RunMode.Async;
                })
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddHostedService<StartupService>()
                        .AddHostedService<CommandHandler>();
                    
                    services
                        .AddSingleton<HttpClient>()
                        .AddSingleton<InteractiveService>();
                })
                .UseConsoleLifetime();
        }
        
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Log.Information("{Sender} thrown an unhandled exception", sender);

            var methods = args.ExceptionObject.GetType().GetMethods();
            var message = "Host is terminating";
            if (methods.FirstOrDefault(m => m.Name == "ToString") != null)
            {
                message = args.ExceptionObject.ToString();
            }

            if (args.ExceptionObject is Exception exception)
            {
                Log.Fatal(exception, "{Message}", message);
                return;
            }
            
            Log.Fatal("{Message}", message);
        }
    }
}