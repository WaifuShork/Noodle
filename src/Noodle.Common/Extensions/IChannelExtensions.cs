using System.Threading.Tasks;
using Discord;
using Color = System.Drawing.Color;
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

        public static Task<IMessage> SendUnsupportedAsync(this IChannel channel, string contents)
        {
            return new EmbedBuilder()
                .WithTitle("Unsupported")
                .WithColor(Color.DarkOrange)
                .WithDescription(contents)
                .SendAsync(channel);
        }
        
        public static Task<IMessage> SendSuccessAsync(this IChannel channel, string contents)
        {
            return new EmbedBuilder()
                .WithTitle("Success")
                .WithColor(Color.MediumSpringGreen)
                .WithDescription(contents)
                .SendAsync(channel);
        }

        public static Task<IMessage> SendErrorAsync(this IChannel channel, string contents)
        {
            return new EmbedBuilder()
                .WithTitle("Error")
                .WithColor(Color.DarkRed)
                .WithDescription(contents)
                .SendAsync(channel);
        }
    }
}