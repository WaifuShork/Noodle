using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Noodle;
using Noodle.Extensions;

namespace Noodle.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public sealed partial class EmoteModule : NoodleModuleBase
    {
        private static async Task<T> GetAsMagickAsync<T>(string url) where T : class
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            
            url = Uri.UnescapeDataString(url.SanitizeUrl());
            await using var stream = await Constants.HttpClient.GetStreamAsync(url);

            if (typeof(T) == typeof(MagickImageCollection))
            {
                return new MagickImageCollection(stream) as T;
            }
            if (typeof(T) == typeof(MagickImage))
            {
                return new MagickImage(stream) as T;
            }
            
            throw new ArgumentException($"Unexpected type: {typeof(T)}");
        }
    }
}