﻿using System;
using System.IO;
using System.Linq;
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
                url = url.SanitizeUrl();

                var message = await Context.Channel.SendMessageAsync("This may take a minute...");

                var (normalCap, animatedCap) = Context.Guild.GetEmoteCap();
                var emotes = await Context.Guild.GetEmotesAsync();

                var animated = emotes.Where(e => e.Animated).ToList();
                var normal = emotes.Where(e => !e.Animated).ToList();
                
                var error = string.Empty;

                switch (extension)
                {
                    case EmoteType.Gif or EmoteType.Hack:
                        if (animated.Count >= animatedCap)
                        {
                            await message.ModifyAsync(m =>
                            {
                                m.Embed = CreateEmbed()
                                    .WithTitle("Emote Limit Reached")
                                    .AddField("Normal Emotes", normal.Count)
                                    .AddField("Animated Emotes", animated.Count)
                                    .Build();
                            });
                            return;
                        }
                        break;
                    case EmoteType.Png:
                        if (normal.Count >= normalCap)
                        {
                            await message.ModifyAsync(m =>
                            {
                                m.Embed = CreateEmbed()
                                    .WithTitle("Emote Limit Reached")
                                    .AddField("Normal Emotes", normal.Count)
                                    .AddField("Animated Emotes", animated.Count)
                                    .Build();
                            });
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
                        error = await UploadAnimatedAsync(url, name, width, height);
                        break;
                    }
                    case EmoteType.Hack:
                    {
                        error = await UploadHackedAsync(url, name, width, height);
                        break;
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(error))
                {
                    await message.DeleteAsync();
                    await Context.Channel.SendErrorEmbedAsync(error);
                    return;
                }
                
                await message.ModifyAsync(m =>
                {
                    m.Embed = CreateEmbed()
                        .WithTitle("Success")
                        .WithColor(Color.Green)
                        .WithDescription($"Added :{name}:")
                        .Build();
                });
            }
        }
        
        private async Task<string> UploadNormalAsync(string url, string name, int width, int height)
        {
            var path = Path.Combine("emotes", $"{name}-{Guid.NewGuid()}.png");
            await using var magick = new MagickSystem<MagickImage>(url);
            magick.Resize(width, height);
            await magick.WriteAsync(path);
            
            if (!magick.Image.ToByteArray().IsSmallEnough(out var sizes))
            {
                return $"File size too large ({sizes})";
            }
            
            try
            {
                using var img = magick.ToImage();
                await Context.Guild.CreateEmoteAsync(name, img);
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        private async Task<string> UploadAnimatedAsync(string url, string name, int width, int height)
        {
            var path = Path.Combine("emotes", $"{name}-{Guid.NewGuid()}.gif");
            await using var magick = new MagickSystem<MagickImageCollection>(url);
            magick.Resize(width, height);
            await magick.WriteAsync(path);
            
            if (!magick.Collection.ToByteArray().IsSmallEnough(out var size))
            {
                return $"File size too large ({size})";
            }
            
            try
            {
                using var img = magick.ToImage();
                await Context.Guild.CreateEmoteAsync(name, img);
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        private async Task<string> UploadHackedAsync(string url, string name, int width, int height)
        {
            var path = Path.Combine("emotes", $"{name}-{Guid.NewGuid()}.gif");

            using var magick = new MagickSystem<MagickImageCollection>(url);
            using var image = new MagickImage(magick.Collection[0]); 
            magick.Collection.Add(image);
            magick.Resize(width, height);
            
            await magick.WriteAsync(path);
            
            if (!magick.Collection.ToByteArray().IsSmallEnough(out var size))
            {
                return $"File size too large ({size})";
            }
            
            try
            {
                using var img = magick.ToImage();
                await Context.Guild.CreateEmoteAsync(name, img);
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
    }
}