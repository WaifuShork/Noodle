using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using ImageMagick;
using Noodle.Extensions;
using Noodle.Common.Models;
using Noodle.TypeReaders;

namespace Noodle.Modules
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed partial class EmoteModule   
    {
        [Command("renmote"), Alias("rename")]
        [Summary("Renames an emote")]
        [Remarks("renmote <emote> <newName>")]
        public async Task RenameEmoteAsync([Summary("The emote to rename"), OverrideTypeReader(typeof(EmoteTypeReader))] Emote emote, 
                                           [Summary("The new name of the emote")] string newName)
        {
            newName = newName.SanitizeEmoteName();
            var em = await Context.Guild.GetEmoteAsync(emote.Id);
            await Context.Guild.ModifyEmoteAsync(em, properties => properties.Name = newName);
            
            await Context.Channel.SendSuccessAsync($"**{emote.Name}** -> **{newName}**");
        }

        [Command("rotate"), Alias("rot")]
        [Summary("Rotate an image by degrees")]
        [Remarks("rotate <url> <degrees>")]
        public async Task RotateEmoteAsync([Summary("The extension of the image or gif")] EmoteType extension,
                                           [Summary("The url of the gif or image to rotate")] string url, 
                                           [Summary("Degrees to rotate by")] double degrees = 0)
        {
            var message = await Context.Channel.SendMessageAsync("This may take a minute...");

            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "rotated");
                    magick.Rotate(degrees);
                    magick.SetColorFuzz(new Percentage(4));
                    magick.SetTransparency(MagickColors.White);

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "rotated");
                    magick.Rotate(degrees);
                    magick.SetColorFuzz(new Percentage(4));
                    magick.SetTransparency(MagickColors.White);

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);                        
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(RotateEmoteAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }

            await message.DeleteAsync();
        }

        [Command("flipflop"), Alias("flopflip")]
        [Summary("Flips and flops an image or gif")]
        [Remarks("flipflop <url>")]
        public async Task FlipEmoteXAndY([Summary("The extension of the image or gif")] EmoteType extension,
                                         [Summary("The url of the gif or image to flipflop")] string url)
        {
            var message = await Context.Channel.SendMessageAsync("This may take a minute...");

            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "flipflop");
                    magick.Flip();
                    magick.Flop();

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "flipflop");
                    magick.Flip();
                    magick.Flop();

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);                     
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(FlipEmoteXAndY)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }

            await message.DeleteAsync();
        }
        
        [Command("xflip"), Alias("flipx", "flop")]
        [Summary("Fips an image horizontally")]
        [Remarks("xflip <url>")]
        public async Task FlipEmoteX([Summary("The extension of the image or gif")] EmoteType extension, 
                                     [Summary("The url of the gif or image to flip horizontally")] string url)
        {
            var message = await Context.Channel.SendMessageAsync("This may take a minute...");

            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "xflip");
                    magick.Flop();

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "xflip");
                    magick.Flop();

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);                   
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(FlipEmoteX)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }

            await message.DeleteAsync();
        }
        
        [Command("yflip"), Alias("flipy", "flip")]
        [Summary("The image to flip vertically")]
        [Remarks("yflip <url>")]
        public async Task FlipEmoteY([Summary("The extension of the image or gif")] EmoteType extension, 
                                     [Summary("The url of the gif or image to flip vertically")] string url)
        {
            var message = await Context.Channel.SendMessageAsync("This may take a minute...");

            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "yflip");
                        
                    magick.Flip();
                    await Context.Channel.SendFileAsync(magick.ToStream(), magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "yflip");
                        
                    magick.Flip();
                    await Context.Channel.SendFileAsync(magick.ToStream(), magick.FilePath);
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(FlipEmoteY)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }

            await message.DeleteAsync();
        }

        [Command("resize")]
        [Summary("Resize an image via url")]
        [Remarks("resize <extension> <flag> <width = 100> <height = 100>")]
        public async Task ResizeImageAsync([Summary("The extension of the gif or image to resize")] EmoteType extension, 
                                           [Summary("The url of the gif or image")] string url, 
                                           [Summary("The width of the gif or image")] int width = 100,
                                           [Summary("The height of the gif or image")] int height = 100,
                                           [Summary("Whether or not to ignore aspect ratio when resizing")] bool ignoreRatio = true)
        {
            var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                
            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "resized");
                    magick.Resize(width, height, ignoreRatio);

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "resized");
                    magick.Resize(width, height, ignoreRatio);

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(ResizeImageAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }
            
            await message.DeleteAsync();
        }

        [Command("invert"), Alias("negate")]
        [Summary("Invert an images colorspace via url")]
        [Remarks("invert <extension> <url>")]
        public async Task InvertImageAsync([Summary("The extension of the gif or image")] EmoteType extension, 
                                           [Summary("The url of the image or gif")] string url)
        {
            var message = await Context.Channel.SendMessageAsync("This may take a minute...");

            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "negated");
                    magick.Negate();

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "negated");
                    magick.Negate();

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(InvertImageAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }
            
            await message.DeleteAsync();
        }

        [Command("jpeg")]
        [Summary("Jpegify an image or gif")]
        [Remarks("jpeg <extension> <url>")]
        public async Task JpegAsync([Summary("The extension of the gif or image")] EmoteType extension, 
                                    [Summary("The url of an image or gif")] string url)
        {
            using var _ = Context.Channel.EnterTypingState();
            var message = await Context.Channel.SendMessageAsync("This may take a minute...");

            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "jpeg");
                    magick.SetQuality(-20000);
                    magick.SetFormat(MagickFormat.Jpeg);

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    await Context.Channel.SendErrorAsync("This feature is currently unavailable for animated gifs");
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(JpegAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }
            
            await message.DeleteAsync();
        }


        [Command("speed")]
        public async Task SetSpeedAsync(int speed, string url)
        {
            using var _ = Context.Channel.EnterTypingState();
            using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "speed");
            magick.SetSpeed(speed);
            await using var stream = magick.ToStream();
            await Context.Channel.SendFileAsync(stream, magick.FilePath);
        }

        [Command("update")]
        public async Task UpdateEmoteAsync(Emote emote, EmoteType extension, string url)
        {
            if (!Context.Guild.Emotes.Contains(emote))
            {
                await Context.Channel.SendErrorAsync($"Unable to locate **:{emote.Name}:** in the current guild");
                return;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var oldEmote = await MagickSystem.CreateAsync<MagickImage>(_httpClient, emote.Url, emote.Name);
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, emote.Name);
                    using var image = await magick.ToEmoteAsync(oldEmote.Width, oldEmote.Height);
                    await Context.Guild.DeleteEmoteAsync(await Context.Guild.GetEmoteAsync(emote.Id));
                    await Context.Guild.CreateEmoteAsync(emote.Name, image);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var oldEmote = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, emote.Url, emote.Name);
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, emote.Name);
                    using var image = await magick.ToEmoteAsync(oldEmote.Width, oldEmote.Height);
                    await Context.Guild.DeleteEmoteAsync(await Context.Guild.GetEmoteAsync(emote.Id));
                    await Context.Guild.CreateEmoteAsync(emote.Name, image);
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(UpdateEmoteAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }

            await Context.Channel.SendSuccessAsync($"Updated **{emote.Name}**");
        }
    }
}