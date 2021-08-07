using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageMagick;
using Noodle.Attributes;
using Noodle.Extensions;

namespace Noodle.Modules
{
    [ModuleName("Emotes/Imaging")]
    [Summary("An extensive set of commands for emote/image/gif manipulation.\n" +
             "Notes: A few of these commands have long running execution cycles and shouldn't be abused.")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public sealed partial class EmoteModule : NoodleModuleBase
    {
        private readonly HttpClient _httpClient;
        
        public EmoteModule(HttpClient client)
        {
            _httpClient = client;
        }
    }
}