using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Noodle.Extensions;
using Noodle.Models;
using Noodle.TypeReaders;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule   
    {
        [Command("renmote"), Alias("rename")]
        [Summary("Renames an emote")]
        [Remarks("renmote <emote> <newName>")]
        public async Task RenameEmoteAsync([Summary("The emote to rename"), OverrideTypeReader(typeof(EmoteTypeReader))] Emote emote, 
                                           [Summary("The new name of the emote")] string newName)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                newName = newName.SanitizeEmoteName();
                var em = await Context.Guild.GetEmoteAsync(emote.Id);
                await Context.Guild.ModifyEmoteAsync(em, properties => properties.Name = newName);
            
                await Context.Channel.SendSuccessEmbedAsync($"**{emote.Name}** -> **{newName}**");
            }
        }

        [Command("rotate"), Alias("rot")]
        [Summary("Rotate an image by degrees")]
        [Remarks("rotate <url> <degrees>")]
        public async Task RotateEmoteAsync([Summary("The extension of the image or gif")] EmoteType extension,
                                           [Summary("The url of the gif or image to rotate")] string url, 
                                           [Summary("Degrees to rotate by")] double degrees = 0)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");

                switch (extension)
                {
                    case EmoteType.Png:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "rotated");
                        magick.Rotate(degrees);
                        magick.SetColorFuzz(new Percentage(4));
                        magick.SetTransparency(MagickColors.White);

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "rotated");
                        magick.Rotate(degrees);
                        magick.SetColorFuzz(new Percentage(4));
                        magick.SetTransparency(MagickColors.White);

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);                        
                        break;
                    }
                }

                await message.DeleteAsync();
            }
        }

        [Command("flipflop"), Alias("flopflip")]
        [Summary("Flips and flops an image or gif")]
        [Remarks("flipflop <url>")]
        public async Task FlipEmoteXAndY([Summary("The extension of the image or gif")] EmoteType extension,
                                         [Summary("The url of the gif or image to flipflop")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");

                switch (extension)
                {
                    case EmoteType.Png:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "flipflop");
                        magick.Flip();
                        magick.Flop();

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "flipflop");
                        magick.Flip();
                        magick.Flop();

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);                     
                        break;
                    }
                }

                await message.DeleteAsync();
            }
        }
        
        [Command("xflip"), Alias("flipx", "flop")]
        [Summary("Fips an image horizontally")]
        [Remarks("xflip <url>")]
        public async Task FlipEmoteX([Summary("The extension of the image or gif")] EmoteType extension, 
                                     [Summary("The url of the gif or image to flip horizontally")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");

                switch (extension)
                {
                    case EmoteType.Png:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "xflip");
                        magick.Flop();

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "xflip");
                        magick.Flop();

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);                   
                        break;
                    }
                }

                await message.DeleteAsync();
            }
        }
        
        [Command("yflip"), Alias("flipy", "flip")]
        [Summary("The image to flip vertically")]
        [Remarks("yflip <url>")]
        public async Task FlipEmoteY([Summary("The extension of the image or gif")] EmoteType extension, 
                                     [Summary("The url of the gif or image to flip vertically")]  string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");

                switch (extension)
                {
                    case EmoteType.Png:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "yflip");
                        
                        magick.Flip();
                        await Context.Channel.SendFileAsync(magick.ToStream(), magick.FilePath);
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "yflip");
                        
                        magick.Flip();
                        await Context.Channel.SendFileAsync(magick.ToStream(), magick.FilePath);
                        break;
                    }
                }

                await message.DeleteAsync();
            }
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
            using (var _ = Context.Channel.EnterTypingState())
            {
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                
                switch (extension)
                {
                    case EmoteType.Png:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "resized");
                        magick.Resize(width, height);

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "resized");
                        magick.Resize(width, height);

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);
                        break;
                    }
                }
            
                await message.DeleteAsync();
            }
        }
        
        [Command("invert"), Alias("negate")]
        [Summary("Invert an images colorspace via url")]
        [Remarks("invert <extension> <url>")]
        public async Task InvertImageAsync([Summary("The extension of the gif or image")] EmoteType extension, 
                                           [Summary("The url of the image or gif")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");

                switch (extension)
                {
                    case EmoteType.Png:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "negated");
                        magick.Negate();

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "negated");
                        magick.Negate();

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);
                        break;
                    }
                }
            
                await message.DeleteAsync();
            }
        }

        [Command("reverse")]
        [Summary("Reverses a gif")]
        [Remarks("reverse <url>")]
        public async Task ReverseGifAsync([Summary("The url of the gif")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "reversed");
                magick.Reverse();
                using var stream = magick.ToStream();
                await Context.Channel.SendFileAsync(stream, magick.FilePath);
            }
        }

        [Command("jpeg")]
        [Summary("Jpegify an image or gif")]
        [Remarks("jpeg <extension> <url>")]
        public async Task JpegAsync([Summary("The extension of the gif or image")] EmoteType extension, 
                                    [Summary("The url of an image or gif")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");

                switch (extension)
                {
                    case EmoteType.Png:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "jpeg");
                        magick.SetQuality(-20000);
                        magick.SetFormat(MagickFormat.Jpeg);

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, magick.FilePath);
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await Context.Channel.SendErrorEmbedAsync("This feature is currently unavailable for animated gifs");
                        break;
                    }
                }
            
                await message.DeleteAsync();
            }
        }


        [Command("speed")]
        public async Task SetSpeedAsync(int speed, string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "speed");
                magick.SetSpeed(speed);
                using var stream = magick.ToStream();
                await Context.Channel.SendFileAsync(stream, magick.FilePath);
            }
        }
    }
}