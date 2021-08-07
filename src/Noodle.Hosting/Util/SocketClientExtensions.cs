using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Noodle.Hosting.Util
{
    /// <summary>
    /// Utilities for the Discord.NET socket client
    /// </summary>
    public static class SocketClientExtensions
    {
        private static TaskCompletionSource<object>? _socketTcs;

        /// <summary>
        /// Asynchronously waits for the socket client's ready event to fire.
        /// </summary>
        /// <param name="client">The Discord.NET socket client</param>
        /// <param name="cancellationToken">The cancellation</param>
        /// <returns></returns>
        public static Task WaitForReadyAsync(this DiscordSocketClient client, CancellationToken cancellationToken)
        {
            if (_socketTcs is null)
            {
                throw new InvalidOperationException("The socket client has not been registered correctly. Did you use ConfigureDiscordHost on your HostBuilder?");
            }

            if (_socketTcs.Task.IsCompleted)
            {
                return _socketTcs.Task;
            }

            var registration = cancellationToken.Register(state =>
                {
                    ((TaskCompletionSource<object>) state).TrySetResult(null!);
                },
                _socketTcs);

            return _socketTcs.Task.ContinueWith(_=> registration.DisposeAsync(), cancellationToken);
        }

        internal static void RegisterSocketClientReady(this DiscordSocketClient client)
        {
            _socketTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.Ready += ClientReady;

            Task ClientReady()
            {
                if (_socketTcs != null)
                {
                    _socketTcs.TrySetResult(null!);
                }

                client.Ready -= ClientReady;
                return Task.CompletedTask;
            }
        }
    }
}
