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
        [Command("fry")]
        public async Task FryAsync(string extension, string url, int count = 0)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                switch (extension)
                {
                    case "png":
                    {
                        var image = await GetAsMagickAsync<MagickImage>(url);

                        for (var k = 0; k <= count; k++)
                        {
                            image.Sharpen(20, 20, Channels.RGB);
                            image.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                            image.Colorize(new MagickColor(100, 0, 0), new Percentage(45));
                            for (var i = 0; i < 10; i++)
                            {
                                image.Contrast(true);
                            }

                            image.BrightnessContrast(new Percentage(5), new Percentage(0), Channels.RGB);

                            image.Quality = -10;
                            
                            image.RotationalBlur(2, Channels.RGB);
                            image.Settings.Format = MagickFormat.Jpeg;
                            image.Settings.AntiAlias = false;
                        }
                        
                        await Context.Channel.SendFileAsync(new MemoryStream(image.ToByteArray()), "fried.png");
                        break;
                    }
                    case "gif":
                    {
                        var collection = await GetAsMagickAsync<MagickImageCollection>(url);
                        for (var k = 0; k <= count; k++)
                        {
                            foreach (var image in collection)
                            {
                                image.Sharpen(20, 20, Channels.RGB);
                                image.AddNoise(NoiseType.MultiplicativeGaussian, 1000, Channels.RGB);
                                image.Colorize(new MagickColor(100, 0, 0), new Percentage(45));
                                for (var i = 0; i < 10; i++)
                                {
                                    image.Contrast(true);
                                }
                            }

                        }
                        
                        await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), "fried.gif");
                        break;
                    }
                }
            }
        }

        [Command("shake")]
        public async Task ShakeAsync(string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
                foreach (var image in collection)
                {
                    image.Sharpen(20, 20, Channels.RGB);
                    image.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                    image.Colorize(new MagickColor(100, 0, 0), new Percentage(10));
                    image.RotationalBlur(-20);
                }
            
                await Context.Channel.SendFileAsync(new MemoryStream(collection.ToByteArray()), "shook.gif");
            }
        }
        
        
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
                    case "":
                        await SendErrorEmbedAsync("You must specify a file extension");
                        return;
                }

                if (!success)
                {
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
            catch
            {
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
            catch
            {
                return false;
            }
        }
    }
}