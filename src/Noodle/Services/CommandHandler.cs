using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Noodle.Hosting;
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
        private readonly string _prefix;
        private IDisposable _typingState;
        
        public CommandHandler(DiscordSocketClient client, 
                              ILogger<CommandHandler> logger,
                              CommandService commandService,
                              IServiceProvider provider,
                              IConfiguration configuration) : base(client, logger)
        {
            _commandService = commandService;
            _provider = provider;
            _prefix = configuration.GetValue<string>("prefix");
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
                _commandService.Log += async (message) => await OnLogAsync(message);
            }, cancellationToken);
        }

        private async Task OnLogAsync(LogMessage message)
        {
            await Task.Run(() =>
            {
                if (message.Exception != null)
                {
                    Log.Information(message.Exception, "An unhandled exception was thrown during an execution cycle");
                }
            });
        }

        private async Task OnMessageReceivedAsync(IDeletable msg)
        {
            if (msg is not SocketUserMessage message)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_prefix))
            {
                await message.Channel.SendErrorAsync("Prefix was not defined in `appsettings.json`");
            }

            var argPos = 0;
            if (!message.HasStringPrefix(_prefix, ref argPos) &&
                !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(Client, message);
            _typingState = context.Channel.EnterTypingState();
            await _commandService.ExecuteAsync(context, argPos, _provider, MultiMatchHandling.Best);
            await msg.DeleteAsync();
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (_typingState != null)
            {
                _typingState.Dispose();
            }
            
            var content = context.Message.Content[_prefix.Length..];
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
                        await command.Value.DisplayCommandHelpAsync(context, _prefix);
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
                        var exception = result is ExecuteResult er ? er.Exception : null;
                        var error = ErrorFromException(exception);
                        await context.Channel.SendErrorAsync(error);
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
                        throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
        }
        
        private string ErrorFromException(Exception exception)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{exception.Message}\n");
            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                var lines = exception.StackTrace.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                sb.AppendLine("**Stack Trace**");
                foreach (var line in lines)
                {
                    sb.AppendLine($"• {line}");
                }
            }
            
            return sb.ToString();
        }
    }
}