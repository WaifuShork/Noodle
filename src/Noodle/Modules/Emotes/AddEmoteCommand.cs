using System;
using Discord;
using System.Linq;
using System.Text;
using ImageMagick;
using Discord.Commands;
using Noodle.Extensions;
using Noodle.TypeReaders;
using Noodle.Common.Models;
using System.Threading.Tasks;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule    
    {
        [Command("addmote")]
        [Summary("Add an emote to the server via url")]
        [Remarks("addmote <extension> <url> <name> <width = 100> <height = 100>")]
        public async Task AddEmoteAsync([Summary("The extension of the emote to upload")] EmoteType extension,
                                        [Summary("Image/Gif url to upload as an emote")] string url,    
                                        [Summary("The name of the emote to upload")] string name,
                                        [Summary("The width of the image/gif")] int width = 100,
                                        [Summary("The height of the image/gif")] int height = 100)
        {
            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            name = name.SanitizeEmoteName();
            var message = await Context.Channel.SendMessageAsync("This may take a minute...");

            var (normalCap, animatedCap) = Context.Guild.GetEmoteCap();
            var emotes = await Context.Guild.GetEmotesAsync();

            var animated = emotes.Where(e => e.Animated).ToList();
            var normal = emotes.Where(e => !e.Animated).ToList();
            
            switch (extension)
            {
                case EmoteType.Gif or EmoteType.Hack:
                    if (animated.Count >= animatedCap)
                    {
                        await message.NotifyEmoteCapReachedAsync(animated.Count, animated.Count);
                        return;
                    }
                    break;
                case EmoteType.Png:
                    if (normal.Count >= normalCap)
                    {
                        await message.NotifyEmoteCapReachedAsync(normal.Count, animated.Count);
                        return;
                    }
                    break;
            }

            switch (extension)
            {
                case EmoteType.Png:
                {
                    await UploadEmoteAsync(url, name, width, height);
                    break;
                }
                // This is separated for now because of file sizing issues with gifs, we find that 50x50 is the best 
                // result to get the lowest file size and retain quality
                case EmoteType.Gif:
                {
                    width = 50;
                    height = 50;
                    await UploadEmoteAsync(url, name, width, height);
                    break;
                }
                case EmoteType.Hack:
                {
                    await UploadHackedAsync(url, name, width, height);
                    break;
                }
            }
            
            await message.DeleteAsync();
            await Context.Channel.SendSuccessAsync($"Added :{name}:");
        }
        
        private async Task UploadEmoteAsync(string url, string name, int width, int height)
        {
            using var magick =  await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, name);
            using var img = await magick.ToEmoteAsync(width, height);
            await Context.Guild.CreateEmoteAsync(name, img);
        }

        private async Task UploadHackedAsync(string url, string name, int width, int height)
        {
            using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, name);
            using var img = await magick.ToHackedAsync(width, height);
            await Context.Guild.CreateEmoteAsync(name, img);
        }
    }
}