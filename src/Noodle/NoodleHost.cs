using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Discord;
using Noodle.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                
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
            var emoteDirectory = Path.Combine("assets", "emotes");
            if (!Directory.Exists(emoteDirectory))
            {
                try
                {
                    Log.Information("Creating '{EmoteDirectory}'", emoteDirectory);
                    Directory.CreateDirectory(emoteDirectory);
                }
                catch (Exception exception)
                {
                    Log.Fatal(exception, "Unable to create '{EmoteDirectory}', terminating...", emoteDirectory);
                }
            }

            return Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration(x =>
                {
                    // TODO: Saving (The configuration file 'appsettings.json' was not found and is not optional. The physical path is '\Noodle\appsettings.json')
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
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };
                    
                    config.Token = context.Configuration["token"];
                })
                .ConfigureCommandService((_, config) =>
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
                        .AddSingleton(services)
                        .AddSingleton<HttpClient>();

                    // This is added so I have access to all the services in 'NamorokaModuleBase', allowing me to pull a service at will,
                    // mostly pointless but a small QoL feature
                    services.AddSingleton(services.BuildServiceProvider());
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