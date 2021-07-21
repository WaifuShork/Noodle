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
        [Command("rename"), Alias("renmote")]
        [RequireContext(ContextType.Guild)]
        public async Task RenameEmoteAsync([OverrideTypeReader(typeof(EmoteTypeReader))] Emote emote, string newName)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                newName = newName.SanitizeEmoteName();
                var em = await Context.Guild.GetEmoteAsync(emote.Id);
                await Context.Guild.ModifyEmoteAsync(em, properties => properties.Name = newName);
            
                await SendSuccessEmbedAsync($"**{emote.Name}** -> **{newName}**");
            }
        }

        [Command("rotate"), Alias("rot")]
        [Summary("Rotate an image by degrees")]
        [Remarks("rotate <url> <degrees>")]
        [RequireContext(ContextType.Guild)]
        public async Task RotateEmoteAsync([Summary("Image/Gif url to rotate")] string url, 
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
        [RequireContext(ContextType.Guild)]
        public async Task FlipEmoteXAndY(string url)
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
        [RequireContext(ContextType.Guild)]
        public async Task FlipEmoteX(string url)
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
        [RequireContext(ContextType.Guild)]
        public async Task FlipEmoteY(string extension, string url)
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
        [RequireContext(ContextType.Guild)]
        public async Task ResizeImageAsync(string extension, string url, int width = 100, int height = 100, bool ignoreRatio = true)
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
        [RequireContext(ContextType.Guild)]
        public async Task InvertImageAsync(string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                url = url.SanitizeUrl();

                var extension = "png";
                if (url.Contains("gif"))
                {
                    extension = "gif";
                }
            
                var message = await Context.Channel.SendMessageAsync("This may take a minute...");
                var fileName = $"negated.{extension}";

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
    }
}