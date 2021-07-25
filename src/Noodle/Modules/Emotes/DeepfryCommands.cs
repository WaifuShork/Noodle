using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using ImageMagick;
using Noodle.Models;
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
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url);
                        for (var k = 0; k <= count; k++)
                        {
                            magick.Sharpen(20, 20, Channels.RGB);
                            magick.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                            for (var i = 0; i < 10; i++)
                            {
                                magick.Contrast(true);
                            }
                            
                            magick.BrightnessContrast(new Percentage(5), new Percentage(0), Channels.RGB);
                            magick.SetQuality(-10);
                            magick.RotationalBlur(2, Channels.RGB);
                            magick.SetFormat(MagickFormat.Jpeg);
                            magick.SetAntialiasing(false);
                        }

                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, "fried.png");
                        break;
                    }
                    case "gif":
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url);
                        for (var k = 0; k <= count; k++)
                        {
                            magick.Sharpen(20, 20, Channels.RGB);
                            magick.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                            for (var i = 0; i < 10; i++)
                            {
                                magick.Contrast(true);
                            }
                            
                            magick.BrightnessContrast(new Percentage(5), new Percentage(0), Channels.RGB);
                            magick.SetQuality(-10);
                            magick.RotationalBlur(2, Channels.RGB);
                            magick.SetAntialiasing(false);
                        }

                        using var stream = magick.ToStream();
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
                        await using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url);
                        magick.Sharpen(20, 20, Channels.RGB);
                        magick.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                        magick.Colorize(new MagickColor(100, 0, 0), new Percentage(10));
                        magick.RotationalBlur(20);
                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, "shook.png");
                        break;
                    }
                    case EmoteType.Gif:
                    {
                        await using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url);
                        magick.Sharpen(20, 20, Channels.RGB);
                        magick.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                        magick.Colorize(new MagickColor(100, 0, 0), new Percentage(10));
                        magick.RotationalBlur(20);
                        using var stream = magick.ToStream();
                        await Context.Channel.SendFileAsync(stream, "shook.gif");
                        break;
                    }
                }
            }
        }
    }
}