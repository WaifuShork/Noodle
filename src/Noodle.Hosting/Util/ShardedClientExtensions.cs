using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Noodle.Hosting.Util
{
    /// <summary>
    /// Utilities for the Discord.NET sharded client
    /// </summary>
    public static class ShardedClientExtensions
    {
        private static TaskCompletionSource<object>? _shardedTcs;
        
        /// <summary>
        /// Asynchronously waits for all shards to report ready.
        /// </summary>
        /// <param name="client">The Discord.NET sharded client.</param>
        /// <param name="cancellationToken">A cancellation token that will cause an early exit if cancelled.</param>
        /// <returns></returns>
        public static Task WaitForReadyAsync(this DiscordShardedClient client, CancellationToken cancellationToken)
        {
            if (_shardedTcs is null)
            {
                throw new InvalidOperationException("The sharded client has not been registered correctly. Did you use ConfigureDiscordShardedHost on your HostBuilder?");
            }

            if (_shardedTcs.Task.IsCompleted)
            {
                return _shardedTcs.Task;
            }

            var registration = cancellationToken.Register(state =>
                {
                    ((TaskCompletionSource<object>) state).TrySetResult(null!);
                },
                _shardedTcs);

            return _shardedTcs.Task.ContinueWith(_ => registration.DisposeAsync(), cancellationToken);
        }


        internal static void RegisterShardedClientReady(this DiscordShardedClient client)
        {
            _shardedTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var shardReadyCount = 0;

            client.ShardReady += ShardReadyAsync;

            Task ShardReadyAsync(DiscordSocketClient _)
            {
                shardReadyCount++;
                if (shardReadyCount == client.Shards.Count)
                {
                    if (_shardedTcs != null)
                    {
                        _shardedTcs.TrySetResult(null!);
                    }
                    
                    client.ShardReady -= ShardReadyAsync;
                }
                return Task.CompletedTask;
            }
        }
    }
}
