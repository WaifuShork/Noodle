using System;
using Discord;
using Discord.WebSocket;

namespace Noodle.Hosting
{
    public class DiscordHostConfiguration
    {
        public string Token { get; set; } = string.Empty;
        public Func<LogMessage, Exception, string> LogFormat { get; set; } = (message, _) => $"{message.Source}: {message.Message}";
        public DiscordSocketConfig SocketConfig { get; set; } = new();
    }
}