using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Noodle.Attributes;
using Noodle.Extensions;

namespace Noodle.Modules
{
    [ModuleName("Emote")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public sealed partial class EmoteModule : NoodleModuleBase
    {
        private readonly HttpClient _httpClient;
        
        public EmoteModule(HttpClient client)
        {
            _httpClient = client;
        }
        
        private async Task<T> GetAsMagickAsync<T>(string url) where T : class
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            
            url = Uri.UnescapeDataString(url.SanitizeUrl());
            await using var stream = await _httpClient.GetStreamAsync(url);

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