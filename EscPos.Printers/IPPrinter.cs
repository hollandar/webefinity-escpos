using EscPos.Commands;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EscPos.Printers;

public class IPPrinter : IPrinter
{
    private readonly string _ip;
    private readonly int _port;
    private TcpClient _client = default!;
    private NetworkStream _stream = default!;
    private readonly PrinterSemaphore _lock = new();

    public IPPrinter(string ip, int port = 9100)
    {
        _ip = ip;
        _port = port;
    }

    public async Task ConnectAsync()
    {
        using var _ = await _lock.LockAsync().ConfigureAwait(false);

        _client = new TcpClient();
        await _client.ConnectAsync(_ip, _port).ConfigureAwait(false);
        _stream = _client.GetStream();
    }

    public async Task SendAsync(byte[] data)
    {
        using var _ = await _lock.LockAsync().ConfigureAwait(false);

        if (_stream == null)
            throw new InvalidOperationException("Printer not connected.");

        await _stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
        await _stream.FlushAsync().ConfigureAwait(false);
    }

    public async Task<byte[]> ReadStatusAsync(byte n = 1, CancellationToken cancellationToken = default)
    {
        using var _ = await _lock.LockAsync(cancellationToken).ConfigureAwait(false);

        if (_stream == null)
            throw new InvalidOperationException("Printer not connected.");

        var request = PrintCommands.RequestStatus(n);
        await _stream.WriteAsync(request, 0, request.Length, cancellationToken).ConfigureAwait(false);
        await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);

        // Many printers respond with 1 byte, but some return more depending on n.
        // Read whatever is available shortly after request, up to a small cap.
        var buffer = new byte[32];

        // Give the device a brief moment; NetworkStream has no ReadTimeout for async.
        // We'll attempt at least one read with cancellation.
        var read = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
        if (read <= 0)
            return Array.Empty<byte>();

        var result = new byte[read];
        Buffer.BlockCopy(buffer, 0, result, 0, read);
        return result;
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Close();
    }
}
