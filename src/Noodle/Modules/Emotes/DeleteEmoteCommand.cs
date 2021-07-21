using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Noodle.TypeReaders;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule    
    {
        [Command("delmote")]
        [RequireContext(ContextType.Guild)]
        public async Task DeleteEmoteAsync([OverrideTypeReader(typeof(EmoteTypeReader))] Emote emote)
        {
            var em = await Context.Guild.GetEmoteAsync(emote.Id);
            try
            {
                await Context.Guild.DeleteEmoteAsync(em);
                await SendSuccessEmbedAsync($"Deleted **{emote.Name}**");
            }
            catch
            {
                await SendErrorEmbedAsync($"Unable to delete **{emote.Name}**");
            }
        }
    }
}