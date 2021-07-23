using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using ImageMagick;
using Noodle.TypeReaders;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule
    {
        [Command("fry")]
        [Summary("Deep fries an image or gif")]
        [Remarks("fry <extension> <url> <count [default = 0]>")]
        public async Task FryAsync([Summary("The extension of the file to fry")] string extension, 
                                   [Summary("The url of the image or gif")] string url, 
                                   [Summary("The amount of times to fry the image")] int count = 0)
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
                        
                        using var stream = new MemoryStream(image.ToByteArray());
                        await Context.Channel.SendFileAsync(stream, "fried.png");
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
                        
                        using var stream = new MemoryStream(collection.ToByteArray());
                        await Context.Channel.SendFileAsync(stream, "fried.gif");
                        break;
                    }
                }
            }
        }

        [Command("shake")]
        [Summary("Shakes an image or gif")]
        [Remarks("shake <extension> <url> <count [default = 0]>")]
        public async Task ShakeAsync([Summary("The extension of the file to shake")] EmoteType extension, 
                                     [Summary("The url of the image or gif")] string url)
        {
            using (var _ = Context.Channel.EnterTypingState())
            {
                switch (extension)
                {
                    case EmoteType.Png:
                    {
                        var image = await GetAsMagickAsync<MagickImage>(url);
                        image.Sharpen(20, 20, Channels.RGB);
                        image.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                        image.Colorize(new MagickColor(100, 0, 0), new Percentage(10));
                        image.RotationalBlur(-20);
                        
                        using var stream = new MemoryStream(image.ToByteArray());
                        await Context.Channel.SendFileAsync(stream, "shook.png");
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        using var collection = await GetAsMagickAsync<MagickImageCollection>(url);
                        foreach (var image in collection)
                        {
                            image.Sharpen(20, 20, Channels.RGB);
                            image.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                            image.Colorize(new MagickColor(100, 0, 0), new Percentage(10));
                            image.RotationalBlur(-20);
                        }
            
                        using var stream = new MemoryStream(collection.ToByteArray());
                        await Context.Channel.SendFileAsync(stream, "shook.gif");
                        break;
                    }
                }
            }
        }
    }
}