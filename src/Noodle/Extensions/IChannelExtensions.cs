using System.Threading.Tasks;
using Discord;

namespace Noodle.Extensions
{
    public static class IChannelExtensions
    {
        public static async Task<IMessage> SendAsync(this IChannel channel, EmbedBuilder builder)
        {
            if (channel is ITextChannel textChannel)
            {
                builder.WithCurrentTimestamp();
                return await textChannel.SendMessageAsync(string.Empty, false, builder.Build());
            }

            return null;
        }

        public static async Task<IMessage> SendSuccessEmbedAsync(this IChannel channel, string contents)
        {
            return await channel.SendAsync(new EmbedBuilder()
                .WithTitle("Success")
                .WithColor(Color.Green)
                .WithDescription(contents));
        }

        public static async Task<IMessage> SendErrorEmbedAsync(this IChannel channel, string contents)
        {
            return await channel.SendAsync(new EmbedBuilder()
                .WithTitle("Error")
                .WithColor(Color.Red)
                .WithDescription(contents));
        }
    }
}