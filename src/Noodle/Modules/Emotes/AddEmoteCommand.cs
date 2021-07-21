using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Noodle.Extensions;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule    
    {
        [Command("addmote")]
        [Summary("Add an emote to the server via url")]
        [Remarks("addmote <extension> <url> <name> <width = 100> <height = 100>")]
        [RequireContext(ContextType.Guild)]
        public async Task AddEmoteAsync([Summary("Image/Gif url to upload as an emote")] string url, 
                                        [Summary("The extension of the emote to upload")] string extension,
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
                
                switch (extension)
                {
                    case "png":
                    {
                        if (normal.Count < normalCap)
                        {
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
                    m.Embed = new EmbedBuilder()
                        .WithTitle("Success")
                        .WithColor(Color.Green)
                        .WithDescription($"Added :{name}:")
                        .Build();
                });
            }
        }
        
        private async Task<bool> UploadNormalAsync(string url, string name, int width, int height)
        {
            var image = await GetAsMagickAsync<MagickImage>(url);
            
            var size = new MagickGeometry
            {
                Width = width,
                Height = height,
                IgnoreAspectRatio = true
            };
                    
            image.Resize(size);

            try
            {
                await Context.Guild.CreateEmoteAsync(name, new Image(new MemoryStream(image.ToByteArray())));
                return true;
            }
            catch (Exception exception)
            {
                await SendErrorEmbedAsync(exception.Message);
                return false;
            }
        }

        private async Task<bool> UploadAnimatedAsync(string url, string name, int width, int height)
        {
            using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
            collection.Resize(width, height, true);
            
            try
            {
                await Context.Guild.CreateEmoteAsync(name, new Image(new MemoryStream(collection.ToByteArray())));
                return true;
            }
            catch (Exception exception)
            {
                await SendErrorEmbedAsync(exception.Message);
                return false;
            }
        }

        private async Task<bool> UploadHackedAsync(string url, string name, int width, int height)
        {
            await using var stream = await Constants.HttpClient.GetStreamAsync(url.SanitizeUrl());
            

            using var collection = new MagickImageCollection(stream);
            using var image = new MagickImage(collection[0]);
            collection.Add(image);
            collection.Resize(width, height, true);

            try
            {
                using var memoryStream = new MemoryStream(collection.ToByteArray());
                using var discordImage = new Image(memoryStream);
                
                await Context.Guild.CreateEmoteAsync(name, discordImage);
                return true;
            }
            catch (Exception exception)
            {
                await SendErrorEmbedAsync(exception.Message);
                return false;
            }
        }
    }
}