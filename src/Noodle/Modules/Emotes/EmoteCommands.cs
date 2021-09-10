using System;
using Discord;
using System.IO;
using ImageMagick;
using System.Linq;
using System.Text;
using Discord.Commands;
using Noodle.Extensions;
using Noodle.TypeReaders;
using Noodle.Common.Models;
using System.Threading.Tasks;
using Humanizer;

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
                    var template = $"Normal: {guild.Emotes.Count(e => !e.Animated)}\n" +
                                        $"Animated: {guild.Emotes.Count(e => e.Animated)}";
                    builder.AddField(guild.Name, template);
                }
            }
            
            await Context.Channel.SendAsync(builder);
        }

        private readonly string _emoteDatabase = Path.Combine("assets", "emotes.json");
        
        [Command("save")]
        public async Task SaveEmoteAsync(EmoteType type, string input, string category = "null")
        {
            var animated = type == EmoteType.Gif;
            
            var url = input;
            if (Emote.TryParse(input, out var emote))
            {
                url = emote.Url;
            }
            
            using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, emote.Name);
            var base64 = Convert.ToBase64String(magick.ToByteArray());
            
            var emoteModel = new EmoteModel
            {
                Name = emote.Name,
                Url = url,
                Base64 = base64,
                Category = category,
                IsAnimated = animated
            };
            
            await DatabaseUtilities.AddAsync(emoteModel, _emoteDatabase);
            await Context.Channel.SendSuccessAsync($"Added **{emote.Name}** to the database");
        }

        [Command("addall")]
        public async Task AddAllAsync()
        {
            foreach (var emote in Context.Guild.Emotes)
            {
                var animated = emote.Animated;
                var url = emote.Url;

                using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, emote.Name);
                var base64 = Convert.ToBase64String(magick.ToByteArray());
            
                var emoteModel = new EmoteModel
                {
                    Name = emote.Name,
                    Url = url,
                    Base64 = base64,
                    Category = "null",
                    IsAnimated = animated
                };
            
                await DatabaseUtilities.AddAsync(emoteModel, _emoteDatabase);
            }
        }

        [Command("getall")]
        public async Task GetAllAsync()
        {
            var emotes = await DatabaseUtilities.GetAllAsync(_emoteDatabase);

            var sb = new StringBuilder();
            foreach (var emote in emotes)
            {
                sb.AppendLine($"**{emote.Name}**");
            }

            var description = sb.ToString();
            if (description.Length > EmbedBuilder.MaxDescriptionLength)
            {
                description = description.TrimTo(EmbedBuilder.MaxDescriptionLength, true);
            }
            
            await Context.Channel.SendAsync(CreateEmbed("Emotes").WithDescription(description));
        }

        [Command("load")]
        public async Task LoadEmoteAsync(string input, string category = "null")
        {
            var name = input;
            if (Emote.TryParse(input, out var emote))
            {
                name = emote.Name;
            }

            try
            {
                var emoteModel = await DatabaseUtilities.GetAsync(name, _emoteDatabase, category);
                await Context.Channel.SendAsync(CreateEmbed(name)
                    .WithColor(Color.Blue)
                    .WithImageUrl(emoteModel.Url));
            }
            catch (NullReferenceException exception)
            {
                await Context.Channel.SendErrorAsync(exception.Message);
            }
        }

        [Command("remove")]
        public async Task RemoveEmoteAsync(string emoteName, string category = "null")
        {
            var name = emoteName;
            if (Emote.TryParse(emoteName, out var emote))
            {
                name = emote.Name;
            }
            
            await DatabaseUtilities.RemoveAsync(name, category, _emoteDatabase);
            await Context.Channel.SendSuccessAsync($"Removed **{name}** from the database");
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
            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (type)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "test.png");
                    magick.DrawText(text, fillColor, strokeColor, fontSize, xCoord, yCoord, ignoreAspect, alignment, style, weight, stretch);
                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, "test.png");
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "test.png");
                    magick.DrawText(text, fillColor, strokeColor, fontSize, xCoord, yCoord, ignoreAspect, alignment, style, weight, stretch);
                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, "test.gif");
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{type.Humanize()}` is not valid for `{nameof(DrawAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }

        [Command("sizeof")]
        public async Task SizeOfFileAsync(EmoteType type, string url)
        {
            if (Emote.TryParse(url, out var emote))
            {
                await SendFileSize(type, emote.Url);
                return;
            }
                
            await SendFileSize(type, url);
        }
        
        private async Task SendFileSize(EmoteType type, string url)
        {
            switch (type)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "test.png");
                    var bytes = magick.ToByteArray();
                    var size = bytes.LongLength.FormatSize();
                    await Context.Channel.SendAsync(new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithDescription($"Size: {size}"));
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "test.png");
                    var bytes = magick.ToByteArray();
                    var size = bytes.LongLength.FormatSize();
                    await Context.Channel.SendAsync(new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithDescription($"Size: {size}"));
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{type.Humanize()}` is not valid for `{nameof(SizeOfFileAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }
    }
}