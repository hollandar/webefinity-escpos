using System;
using System.Threading;
using System.Threading.Tasks;

namespace EscPos.Printers;

internal sealed class PrinterSemaphore
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async ValueTask<Releaser> LockAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Releaser(_semaphore);
    }

    internal readonly struct Releaser : IDisposable
    {
        private readonly SemaphoreSlim? _toRelease;

        public Releaser(SemaphoreSlim toRelease) => _toRelease = toRelease;

        public void Dispose() => _toRelease?.Release();
    }
}
