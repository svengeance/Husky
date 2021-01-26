using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Husky.Internal.Shared
{
    /// <summary>
    ///     An implementation of <see cref="IProgress{T}"/> that guarantees sequential execution.
    ///     Upon disposal, this variant will asynchronously await the execution of all remaining <typeparamref name="TEvent"/>s
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    internal class SequentialBlockingProgressHandler<TEvent> : IProgress<TEvent>, IAsyncDisposable
    {
        private readonly Action<TEvent> _handler;

        private readonly Channel<TEvent> _channel = Channel.CreateUnbounded<TEvent>();

        private readonly CancellationTokenSource _cts = new();

        private Task? _processingTask;

        public SequentialBlockingProgressHandler(Action<TEvent> handler) => _handler = handler;

        public void Report(TEvent progress)
        {
            _processingTask ??= Process(_cts.Token);
            _ = _channel.Writer.TryWrite(progress);
        }

        private async Task Process(CancellationToken ct)
        {
            await foreach (var progressEvent in _channel.Reader.ReadAllAsync(ct))
                _handler.Invoke(progressEvent);
        }

        public async ValueTask DisposeAsync()
        {
            _channel.Writer.Complete();
            
            if (_processingTask != null)
                await _processingTask;
        }
    }
}