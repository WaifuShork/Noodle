using System;
using Discord;
using ImageMagick;
using Discord.Commands;
using Noodle.TypeReaders;
using Noodle.Common.Models;
using System.Threading.Tasks;
using Humanizer;
using Noodle.Extensions;

namespace Noodle.Modules
{
    public sealed partial class EmoteModule
    {
        [Command("fry")]
        [Summary("Deep fries an image or gif")]
        [Remarks("fry <extension> <url> <count [default = 0]>")]
        public async Task FryAsync([Summary("The extension of the file to fry")] EmoteType extension, 
                                   [Summary("The url of the image or gif")] string url, 
                                   [Summary("The amount of times to fry the image")] int count = 0)
        {
            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "fried");
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

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "fried");
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

                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(FryAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }
        }

        [Command("shake")]
        [Summary("Shakes an image or gif")]
        [Remarks("shake <extension> <url> <count [default = 0]>")]
        public async Task ShakeAsync([Summary("The extension of the file to shake")] EmoteType extension, 
                                     [Summary("The url of the image or gif")] string url)
        {
            if (Emote.TryParse(url, out var emote))
            {
                url = emote.Url;
            }
                
            switch (extension)
            {
                case EmoteType.Png:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImage>(_httpClient, url, "shook");
                    magick.Sharpen(20, 20, Channels.RGB);
                    magick.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                    magick.Colorize(new MagickColor(100, 0, 0), new Percentage(10));
                    magick.RotationalBlur(20);
                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Gif:
                {
                    using var magick = await MagickSystem.CreateAsync<MagickImageCollection>(_httpClient, url, "shook");
                    magick.Sharpen(20, 20, Channels.RGB);
                    magick.AddNoise(NoiseType.MultiplicativeGaussian, Channels.RGB);
                    magick.Colorize(new MagickColor(100, 0, 0), new Percentage(10));
                    magick.RotationalBlur(20);
                    await using var stream = magick.ToStream();
                    await Context.Channel.SendFileAsync(stream, magick.FilePath);
                    break;
                }
                case EmoteType.Hack:
                {
                    await Context.Channel.SendUnsupportedAsync($"`{extension.Humanize()}` is not valid for `{nameof(ShakeAsync)}`");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
                }
            }
        }
    }
}