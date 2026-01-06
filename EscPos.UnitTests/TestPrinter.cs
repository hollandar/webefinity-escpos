using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EscPos.Printers;

namespace EscPos.UnitTests;

internal sealed class TestPrinter : IPrinter
{
    private readonly Dictionary<byte, byte[]> _responsesByN;

    public List<byte[]> Sent { get; } = new();

    public TestPrinter(Dictionary<byte, byte[]>? responsesByN = null)
    {
        _responsesByN = responsesByN ?? new Dictionary<byte, byte[]>();
    }

    public Task ConnectAsync() => Task.CompletedTask;

    public Task SendAsync(byte[] data)
    {
        Sent.Add(data);
        return Task.CompletedTask;
    }

    public Task<byte[]> ReadStatusAsync(byte n = 1, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_responsesByN.TryGetValue(n, out var res))
            return Task.FromResult(res);

        return Task.FromResult(Array.Empty<byte>());
    }

    public void Dispose()
    {
    }
}
