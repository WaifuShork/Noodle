using System.Threading.Tasks;
using Discord;
using Discord.Rest;

namespace Noodle.Extensions
{
    public static class IMessageExtensions
    {
        public static async Task<IMessage> NotifyEmoteCapReachedAsync(this IMessage message, int normalCount, int animatedCount)
        {
            if (message is not RestUserMessage restMessage)
            {
                return null;
            }
            
            await restMessage.ModifyAsync(m =>
            {
                m.Embed = new EmbedBuilder()
                    .WithTitle("Emote Limit Reached")
                    .AddField("Normal Emotes", normalCount)
                    .AddField("Animated Emotes", animatedCount)
                    .Build();
            });
            
            return restMessage;
        }
    }
}