using System.Threading.Tasks;
using Discord;
using Color = System.Drawing.Color;
namespace Noodle.Extensions
{
    public static class IChannelExtensions
    {
        public static Task<IUserMessage> SendAsync(this IChannel channel, EmbedBuilder builder)
        {
            return (channel as ITextChannel)?.SendMessageAsync(string.Empty, false, builder.Build());
        }

        public static Task<IUserMessage> SendUnsupportedAsync(this IChannel channel, string contents)
        {
            return new EmbedBuilder()
                .WithTitle("Unsupported")
                .WithColor(Color.DarkOrange)
                .WithDescription(contents)
                .SendAsync(channel);
        }
        
        public static Task<IUserMessage> SendSuccessAsync(this IChannel channel, string contents)
        {
            return new EmbedBuilder()
                .WithTitle("Success")
                .WithColor(Color.MediumSpringGreen)
                .WithDescription(contents)
                .SendAsync(channel);
        }

        public static Task<IUserMessage> SendErrorAsync(this IChannel channel, string contents)
        {
            return new EmbedBuilder()
                .WithTitle("Error")
                .WithColor(Color.DarkRed)
                .WithDescription(contents)
                .SendAsync(channel);
        }
    }
}