using System;
using System.Threading;
using System.Threading.Tasks;

namespace EscPos.Printers;

public interface IPrinter : IDisposable
{
    Task ConnectAsync();
    Task SendAsync(byte[] data);

    Task<byte[]> ReadStatusAsync(byte n = 1, CancellationToken cancellationToken = default);
}
