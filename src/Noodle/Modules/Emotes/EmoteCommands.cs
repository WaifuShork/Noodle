using System;
using System.IO;
using System.Linq;

using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Microsoft.VisualBasic.CompilerServices;
using Noodle.Extensions;
using Noodle.Models;
using Noodle.TypeReaders;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule   
    {
        [Command("emoteupdate"), Alias("eu", "emotestatus", "emotecount", "es")]
        [Summary("Shows the current emote count in the emote servers")]
        [Remarks("es")]
        public async Task DisplayEmoteCountAsync(bool showAll = false)
        {
            var guilds = Context.User.MutualGuilds;

            var counter = guilds.Where(guild => guild.Name.Contains("ShorkMotes")).Sum(guild => guild.Emotes.Count);

            var emotes = await Context.Guild.GetEmotesAsync();
            var animatedEmotes = emotes.Where(e => e.Animated).ToList();
            var normalEmotes = emotes.Where(e => !e.Animated).ToList();

            var builder = CreateEmbed()
                .WithTitle("Emote Status")
                .AddField("Normal", normalEmotes.Count)
                .AddField("Animated", animatedEmotes.Count)
                .AddField("Guild Total", emotes.Count)
                .AddField("True Total", counter)
                .WithColor(Color.Blue);

            if (showAll)
            {
                var emoteGuilds = guilds.Where(guild => guild.Name.Contains("ShorkMotes")).OrderBy(n => n.Name);
            
                foreach (var guild in emoteGuilds)
                {
                    var template = $"Normal: {guild.Emotes.Where(e => !e.Animated).Count()}\n" +
                                   $"Animated: {guild.Emotes.Where(e => e.Animated).Count()}";
                    builder.AddField(guild.Name, template);
                }
            }
            
            await Context.Channel.SendAsync(builder);
        }

        [Command("save")]
        public async Task SaveEmoteAsync([OverrideTypeReader(typeof(EmoteTypeReader))] Emote emote)
        {
            var path = Path.Combine("assets", "emotes.json");
            await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, emote.Url, emote.Name);
            var base64 = Convert.ToBase64String(magick.ToByteArray());
            
            var emoteModel = new EmoteModel
            {
                Name = emote.Name,
                Url = emote.Url,
                Base64 = base64,
                Category = "null"
            };
            
            await DatabaseUtilities.AddAsync(emoteModel, path);
        }
        
        [Command("load")]
        public async Task LoadEmoteAsync([OverrideTypeReader(typeof(EmoteTypeReader))] Emote emote)
        {
            var path = Path.Combine("assets", "emotes.json");

            var model = await DatabaseUtilities.GetAsync(emote.Name, path);

            var bytes = Convert.FromBase64String(model.Base64);
            using var stream = new MemoryStream(bytes);
            await Context.Channel.SendFileAsync(stream, $"{emote.Name}.gif");
        }

        [Command("draw")]
        public async Task DrawAsync([Summary("The extension of the logo")] EmoteType type, 
                                    [Summary("The url of the gif or image")] string url, 
                                    [Summary("The text to draw to the gif or image")] string text, 
                                    [Summary("The fill color of the text")] string fillColor = "black", 
                                    [Summary("The stroke color of the text ")] string strokeColor = "black",
                                    [Summary("The font size of the text")] int fontSize = 70, 
                                    [Summary("The X coordinate to draw the text to")] int xCoord = 128, 
                                    [Summary("The Y coordinate to draw the text to")] int yCoord = 128, 
                                    [Summary("Ignore aspect ratio when resizing")] bool ignoreAspect = true, 
                                    [Summary("The text alignment")] TextAlignment alignment = TextAlignment.Center,
                                    [Summary("The font style")] FontStyleType style = FontStyleType.Undefined,
                                    [Summary("The font weight")] FontWeight weight = FontWeight.Undefined,
                                    [Summary("The font stretching")] FontStretch stretch = FontStretch.Undefined)
        {  
            using (var _ = Context.Channel.EnterTypingState())
            {
                switch (type)
                {
                    case EmoteType.Png:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "test.png");
                        magick.DrawText(text, fillColor, strokeColor, fontSize, xCoord, yCoord, ignoreAspect, alignment, style, weight, stretch);
                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, "test.png");
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "test.png");
                        magick.DrawText(text, fillColor, strokeColor, fontSize, xCoord, yCoord, ignoreAspect, alignment, style, weight, stretch);
                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, "test.gif");
                        break;
                    }
                    default:
                        await Context.Channel.SendErrorEmbedAsync("Unsupported file type");
                        break;
                }
            }
        }

        [Command("sizeof")]
        public async Task SizeOfFileAsync(EmoteType type, string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                if (Emote.TryParse(url, out var emote))
                {
                    await SendFileSize(type, emote.Url);
                    return;
                }
                
                await SendFileSize(type, url);
            }
        }
        
        private async Task SendFileSize(EmoteType type, string url)
        {
            switch (type)
            {
                case EmoteType.Png:
                {
                    await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "test.png");
                    var bytes = magick.ToByteArray();
                    var size = bytes.LongLength.FormatSize();
                    await Context.Channel.SendAsync(new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithDescription($"Size: {size}"));
                    break;
                }
                case EmoteType.Gif:
                {
                    await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "test.png");
                    var bytes = magick.ToByteArray();
                    var size = bytes.LongLength.FormatSize();
                    await Context.Channel.SendAsync(new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithDescription($"Size: {size}"));
                    break;
                }
                default:
                    await Context.Channel.SendErrorEmbedAsync("Unsupported file type");
                    break;
            }
        }
    }
}