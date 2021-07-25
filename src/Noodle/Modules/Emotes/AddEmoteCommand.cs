using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Noodle.Extensions;
using Noodle.Models;
using Noodle.TypeReaders;
using Serilog;

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
            using (var _ = Context.Channel.EnterTypingState())
            {
                name = name.SanitizeEmoteName();
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");

                var (normalCap, animatedCap) = Context.Guild.GetEmoteCap();
                var emotes = await Context.Guild.GetEmotesAsync();

                var animated = emotes.Where(e => e.Animated).ToList();
                var normal = emotes.Where(e => !e.Animated).ToList();

                string error = null;
                
                switch (extension)
                {
                    case EmoteType.Gif or EmoteType.Hack:
                        if (animated.Count >= animatedCap)
                        {
                            await message.NotifyEmoteCapReachedAsync(normal.Count, animated.Count);
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
                        error = await UploadNormalAsync(url, name, width, height);
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        width = 50;
                        height = 50;
                        error = await UploadAnimatedAsync(url, name, width, height);
                        break;
                    }
                    case EmoteType.Hack:
                    {
                        error = await UploadHackedAsync(url, name, width, height);
                        break;
                    }
                }

                if (error != null)
                {
                    await Context.Channel.SendErrorEmbedAsync(error);
                    return;
                }

                await message.DeleteAsync();
                await Context.Channel.SendSuccessEmbedAsync($"Added :{name}:");
            }
        }
        
        private async Task<string> UploadNormalAsync(string url, string name, int width, int height)
        {
            try
            {
                using var magick =  await MagickSystem.CreateAsync<MagickImage>(_httpClient, url);
                using var img = magick.ToEmote(width, height);
                await Context.Guild.CreateEmoteAsync(name, img);
                return null;
            }
            catch (Exception exception)
            {
                return ErrorFromException(exception);
            }
        }

        private async Task<string> UploadAnimatedAsync(string url, string name, int width, int height)
        {
            try
            {
                await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url);
                using var img = magick.ToEmote(width, height);
                await Context.Guild.CreateEmoteAsync(name, img);
                return null;
            }
            catch (Exception exception)
            {
                return ErrorFromException(exception);        
            }
        }

        private async Task<string> UploadHackedAsync(string url, string name, int width, int height)
        {
            try
            {
                await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url);
                using var img = magick.ToHacked(name, width, height);
                await Context.Guild.CreateEmoteAsync(name, img);
                return null;
            }
            catch (Exception exception)
            {
                return ErrorFromException(exception);
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