using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Noodle.Extensions;
using Serilog;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule    
    {
        [Command("addmote")]
        [Summary("Add an emote to the server via url")]
        [Remarks("addmote <extension> <url> <name> <width = 100> <height = 100>")]
        public async Task AddEmoteAsync([Summary("The extension of the emote to upload")] string extension,
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
                
                var success = false;
                
                switch (extension.ToLowerInvariant())
                {
                    case "png":
                    {
                        if (normal.Count < normalCap)
                        {
                            await ReplyAsync("Uploading normal");
                            success = await UploadNormalAsync(url, name, width, height);
                        }
                        else
                        {
                            await SendErrorEmbedAsync($"Server at emote limit\n\nEmote Cap: {normalCap}\nEmote Count: {normal.Count}");
                        }
                        break;
                    }
                    case "gif":
                    {
                        if (animated.Count < animatedCap)
                        {
                            await ReplyAsync("Uploading animated");
                            success = await UploadAnimatedAsync(url, name, width, height);
                        }
                        else
                        {
                            await SendErrorEmbedAsync($"Server at emote limit\n\nEmote Cap: {animatedCap}\nEmote Count: {animated.Count}");
                        }
                        break;
                    }
                    case "hack":
                    {
                        if (animated.Count() < animatedCap)
                        {
                            await ReplyAsync("Uploading animated");
                            success = await UploadHackedAsync(url, name, width, height);
                        }
                        else
                        {
                            await SendErrorEmbedAsync($"Server at emote limit\n\nEmote Cap: {animatedCap}\nEmote Count: {animated.Count}");
                        }
                        break;
                    }
                }
                
                if (success == false)
                {
                    await SendErrorEmbedAsync("Unable to upload emote");
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
        
        private async Task<bool> UploadNormalAsync(string url, string name, int width, int height)
        {
            var path = Path.Combine("emotes", $"{name}-{Guid.NewGuid()}.png");
            var image = await GetAsMagickAsync<MagickImage>(url);
            
            var size = new MagickGeometry
            {
                Width = width,
                Height = height,
                IgnoreAspectRatio = true
            };
                    
            image.Resize(size);
            await image.WriteAsync(path);
            
            try
            {
                using var discordImage = new Image(path);
                await Context.Guild.CreateEmoteAsync(name, discordImage);
                return true;
            }
            catch (Exception exception)
            {
                Log.Information(exception, "Unable to upload emote");
                await SendErrorEmbedAsync(exception.Message);
                return false;
            }
        }

        private async Task<bool> UploadAnimatedAsync(string url, string name, int width, int height)
        {
            var path = Path.Combine("emotes", $"{name}-{Guid.NewGuid()}.gif");
            using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
            collection.Resize(width, height, true);

            await collection.WriteAsync(path);
            try
            {
                using var discordGif = new Image(path); 
                await Context.Guild.CreateEmoteAsync(name, discordGif);
                return true;
            }
            catch (Exception exception)
            {
                Log.Information(exception, "Unable to upload emote");
                await SendErrorEmbedAsync(exception.Message);
                return false;
            }
        }

        private async Task<bool> UploadHackedAsync(string url, string name, int width, int height)
        {
            var path = Path.Combine("emotes", $"{name}-{Guid.NewGuid()}.gif");
            await using var stream = await _httpClient.GetStreamAsync(url.SanitizeUrl());

            using var collection = new MagickImageCollection(stream);
            using var image = new MagickImage(collection[0]);
            collection.Add(image);
            collection.Resize(width, height, true);

            await collection.WriteAsync(path);
            
            try
            {
                using var discordImage = new Image(path);
                await Context.Guild.CreateEmoteAsync(name, discordImage);
                return true;
            }
            catch (Exception exception)
            {
                Log.Information(exception, "Unable to upload emote");
                await SendErrorEmbedAsync(exception.Message);
                return false;
            }
        }
    }
}