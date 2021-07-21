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
    }
}