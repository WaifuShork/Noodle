using Discord.WebSocket;
using Noodle.Hosting.Util;
using Microsoft.Extensions.Options;

namespace Noodle.Hosting.Injectables
{
    internal class InjectableDiscordSocketClient : DiscordSocketClient
    {
        public InjectableDiscordSocketClient(IOptions<DiscordHostConfiguration> config) : base(config.Value.SocketConfig)
        {
            this.RegisterSocketClientReady();
        }
    }
}
