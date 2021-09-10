using System.Threading.Tasks;
using Discord;
using Serilog;
using Serilog.Events;

namespace Noodle.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder WithColor(this EmbedBuilder builder, System.Drawing.Color color)
        {
            return builder.WithColor(color.R, color.G, color.B);
        }

        public static Task<IUserMessage> SendAsync(this EmbedBuilder builder, IChannel channel)
        {
            return channel.SendAsync(builder);
        }
    }
}