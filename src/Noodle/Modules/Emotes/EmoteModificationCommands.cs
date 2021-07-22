using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Noodle.Extensions;
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
        public async Task RotateEmoteAsync([Summary("The url of the gif or image to rotate")] string url, 
                                           [Summary("Degrees to rotate by")] double degrees = 0)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                url = url.SanitizeUrl();

                var extension = "png";
                if (url.EndsWith("gif"))
                {
                    extension = "gif";
                }
            
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                var fileName = $"rotated.{extension}";

                switch (extension)
                {
                    case "png":
                    {
                        using var image = await GetAsMagickAsync<MagickImage>(url);
            
                        image.Rotate(degrees);
                        image.ColorFuzz = new Percentage(4);
                        image.Transparent(MagickColors.White);
                        await Context.Channel.SendFileAsync(new MemoryStream(image.ToByteArray()), fileName);
                        break;
                    }
                    case "gif":
                    {
                        using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
                        collection.Rotate(degrees);
                        await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), fileName);
                        break;
                    }
                }

                await message.DeleteAsync();
            }
        }

        [Command("flipflop"), Alias("flopflip")]
        [Summary("Flips and flops an image or gif")]
        [Remarks("flipflop <url>")]
        public async Task FlipEmoteXAndY([Summary("The url of the gif or image to flipflop")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                url = url.SanitizeUrl();

                var extension = "png";
                if (url.EndsWith("gif"))
                {
                    extension = "gif";
                }
            
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                var fileName = $"flipflop.{extension}";

                switch (extension)
                {
                    case "png":
                    {
                        using var image = await GetAsMagickAsync<MagickImage>(url);
                        image.Flip();
                        image.Flop();
                        await Context.Channel.SendFileAsync(new MemoryStream(image.ToByteArray()), fileName);
                        break;
                    }
                    case "gif":
                    {
                        using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
                        collection.Flip();
                        collection.Flop();
                        await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), fileName);
                        break;
                    }
                }

                await message.DeleteAsync();
            }
        }
        
        [Command("xflip"), Alias("flipx", "flop")]
        [Summary("Fips an image horizontally")]
        [Remarks("xflip <url>")]
        public async Task FlipEmoteX([Summary("The extension of the image or gif")] string extension, 
                                     [Summary("The url of the gif or image to flip horizontally")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                url = url.SanitizeUrl();

                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                var fileName = $"flipx.{extension}";

                switch (extension)
                {
                    case "png":
                    {
                        using var image = await GetAsMagickAsync<MagickImage>(url);
                        image.Flop();
                        await Context.Channel.SendFileAsync(new MemoryStream(image.ToByteArray()), fileName);
                        break;
                    }
                    case "gif":
                    {
                        using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
                        collection.Flip();
                        await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), fileName);
                        break;
                    }
                }

                await message.DeleteAsync();
            }
        }
        
        [Command("yflip"), Alias("flipy", "flip")]
        [Summary("The image to flip vertically")]
        [Remarks("yflip <url>")]
        public async Task FlipEmoteY([Summary("The extension of the image or gif")] string extension, 
                                     [Summary("The url of the gif or image to flip vertically")]  string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                url = url.SanitizeUrl();

                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                var fileName = $"flipy.{extension}";

                switch (extension)
                {
                    case "png":
                    {
                        using var image = await GetAsMagickAsync<MagickImage>(url);
            
                        image.Flip();
                        await Context.Channel.SendFileAsync(new MemoryStream(image.ToByteArray()), fileName);
                        break;
                    }
                    case "gif":
                    {
                        using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
                        collection.Flop();
                        await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), fileName);
                        break;
                    }
                }

                await message.DeleteAsync();
            }
        }

        [Command("resize")]
        [Summary("Resize an image via url")]
        [Remarks("resize <extension> <flag> <width = 100> <height = 100>")]
        public async Task ResizeImageAsync([Summary("The extension of the gif or image to resize")] string extension, 
                                           [Summary("The url of the gif or image")] string url, 
                                           [Summary("The width of the gif or image")] int width = 100,
                                           [Summary("The height of the gif or image")] int height = 100,
                                           [Summary("Whether or not to ignore aspect ratio when resizing")] bool ignoreRatio = true)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                url = url.SanitizeUrl();

                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
            
                var fileName = $"resized.{extension}";

                switch (extension)
                {
                    case "png":
                    {
                        using var image = await GetAsMagickAsync<MagickImage>(url);
                        var size = new MagickGeometry
                        {
                            Width = width,
                            Height = height,
                            IgnoreAspectRatio = ignoreRatio
                        };
                    
                        image.Resize(size);
                        await Context.Channel.SendFileAsync(new MemoryStream(image.ToByteArray()), fileName);
                        break;
                    }
                    case "gif":
                    {
                        using var collection = await GetAsMagickAsync<MagickImageCollection>(url); 
                        collection.Resize(width, height, ignoreRatio);
                        await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), fileName);
                        break;
                    }
                }
            
                await message.DeleteAsync();
            }
        }
        
        [Command("invert")]
        [Summary("Invert an images colorspace via url")]
        [Remarks("invert <url>")]
        public async Task InvertImageAsync([Summary("The extension of the gif or image")] string extension, 
                                           [Summary("The url of the image or gif")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                url = url.SanitizeUrl();

                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                var fileName = $"inverted.{extension}";

                switch (extension)
                {
                    case "png":
                    {
                        using var image = await GetAsMagickAsync<MagickImage>(url);
                        image.Negate(Channels.RGB);
                        await Context.Channel.SendFileAsync(new MemoryStream(image.ToByteArray()), fileName);
                        break;
                    }
                    case "gif":
                    {
                        using var collection = await GetAsMagickAsync<MagickImageCollection>(url); 
                        collection.Negate(Channels.RGB);
                        await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), fileName);
                        break;
                    }
                }
            
                await message.DeleteAsync();
            }
        }

        [Command("jpeg")]
        public async Task JpegAsync(string extension, string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                url = url.SanitizeUrl();

                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                var fileName = $"jpeg.{extension}";

                switch (extension)
                {
                    case "png":
                    {
                        using var image = await GetAsMagickAsync<MagickImage>(url);
                        image.Quality = -20000;
                        image.Format = MagickFormat.Jpeg;

                        await Context.Channel.SendFileAsync(new MemoryStream(image.ToByteArray()), fileName);
                        break;
                    }
                    case "gif":
                    {
                        using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
                        foreach (var image in collection)
                        {
                            image.Quality = -20000;
                            image.Format = MagickFormat.Jpeg;
                        }
                        await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), fileName);
                        break;
                    }
                }
            
                await message.DeleteAsync();
            }
        }
    }
}