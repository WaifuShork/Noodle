using System;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Noodle.Hosting.Injectables;
using Noodle.Hosting.Util;
using Serilog;

namespace Noodle.Hosting
{
    public static class DiscordHostBuilderExtensions
    {
        public static IHostBuilder ConfigureDiscordHost(this IHostBuilder builder, Action<HostBuilderContext, DiscordHostConfiguration>? config = null)
        {
            return builder.ConfigureDiscordHostInternal<DiscordSocketClient>(config).ConfigureServices((_, collection) =>
            {
                if (collection.Any(x => x.ServiceType.BaseType == typeof(BaseSocketClient)))
                {
                    throw new InvalidOperationException("Cannot add more than one Discord Client to host");
                }

                collection.AddSingleton<DiscordSocketClient, InjectableDiscordSocketClient>();
            });
        }
        
        private static IHostBuilder ConfigureDiscordHostInternal<T>(this IHostBuilder builder, Action<HostBuilderContext, DiscordHostConfiguration>? config = null) where T: BaseSocketClient
        {
            return builder.ConfigureServices((context, collection) =>
            {
                collection.AddOptions<DiscordHostConfiguration>().Validate(x => ValidateToken(x.Token));

                if (config != null)
                { 
                    collection.Configure<DiscordHostConfiguration>(x => config(context, x));
                }
                
                collection.AddSingleton(typeof(LogAdapter<>));
                collection.AddHostedService<DiscordHostedService<T>>();
            });

            static bool ValidateToken(string token)
            {
                try
                {
                    TokenUtils.ValidateToken(TokenType.Bot, token);
                    return true;
                }
                catch (Exception e) when (e is ArgumentNullException || e is ArgumentException)
                {
                    Log.Fatal($"[{token}] is not a valid Discord bot token, terminating host.");
                    return false;
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
        }
        
        public static IHostBuilder ConfigureCommandService(this IHostBuilder builder, Action<HostBuilderContext, CommandServiceConfig> config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            builder.ConfigureServices((context, collection) =>
            {
                if (collection.Any(x => x.ServiceType == typeof(CommandService)))
                {
                    throw new InvalidOperationException("Cannot add more than one CommandService to host");
                }

                collection.Configure<CommandServiceConfig>(x => config(context, x));

                collection.AddSingleton(x=> new CommandService(x.GetRequiredService<IOptions<CommandServiceConfig>>().Value));
                collection.AddHostedService<CommandServiceRegistrationHost>();
            });

            return builder;
        }
    }
}
