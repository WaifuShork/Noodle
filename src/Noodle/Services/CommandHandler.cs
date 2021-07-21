using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Noodle.Extensions;
using Serilog;

namespace Noodle.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;
        
        public CommandHandler(DiscordSocketClient client, 
                              ILogger<DiscordClientService> logger,
                              CommandService commandService,
                              IServiceProvider provider,
                              IConfiguration configuration) : base(client, logger)
        {
            _commandService = commandService;
            _provider = provider;
            _config = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                Client.MessageReceived += async (message) => await OnMessageReceivedAsync(message);
                _commandService.CommandExecuted += async (command, context, result) => await OnCommandExecutedAsync(command, context, result);
            });
        }

        private async Task OnMessageReceivedAsync(IDeletable msg)
        {
            if (msg is not SocketUserMessage message)
            {
                return;
            }

            var prefix = _config["prefix"];
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "$";
            }

            var argPos = 0;
            if (!message.HasStringPrefix(prefix, ref argPos) &&
                !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(Client, message);
            
            await _commandService.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            var prefix = _config["prefix"];

            var content = context.Message.Content[prefix.Length..];
            var commandName = content.Split(' ')[0]; 

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case CommandError.UnknownCommand:
                        await context.Channel.SendMessageAsync($"Unknown command: **{commandName}**");
                        break;
                    case CommandError.ParseFailed:
                        await context.Channel.SendAsync(new EmbedBuilder()
                            .WithTitle("Parse Failed")
                            .WithColor(Color.Red)
                            .WithDescription(result.ErrorReason));                        
                        break;
                    case CommandError.BadArgCount:
                        await command.Value.DisplayCommandHelpAsync(context, prefix);
                        break;
                    case CommandError.ObjectNotFound:
                        await context.Channel.SendAsync(new EmbedBuilder()
                            .WithTitle("Object Not Found")
                            .WithColor(Color.Red)
                            .WithDescription(result.ErrorReason));
                        break;
                    case CommandError.MultipleMatches:
                        await context.Channel.SendAsync(new EmbedBuilder()
                            .WithTitle("Multiple Matches")
                            .WithColor(Color.Red)
                            .WithDescription(result.ErrorReason));
                        break;
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendAsync(new EmbedBuilder()
                            .WithColor(Color.Red)
                            .WithDescription("Access denied."));
                        break;
                    case CommandError.Exception:
                        await context.Channel.SendAsync(new EmbedBuilder()
                            .WithTitle("Exception")
                            .WithColor(Color.Red)
                            .WithDescription(result.ErrorReason));
                        break;
                    case CommandError.Unsuccessful:
                        await context.Channel.SendAsync(new EmbedBuilder()
                            .WithTitle("Unsuccessful")
                            .WithColor(Color.Red)
                            .WithDescription(result.ErrorReason));
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}