using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EscPos.Printers;

// Cross-platform printer implementation that writes raw ESC/POS bytes to a file/device path.
// Examples:
// - Linux: "/dev/usb/lp0" or "/dev/ttyUSB0" (permissions required)
// - Windows: "LPT1" (if available) or a UNC path to a share that accepts raw bytes
public sealed class FilePrinter : IPrinter
{
    private readonly string _path;
    private readonly Func<FileStream> _streamFactory;
    private readonly PrinterSemaphore _lock = new();
    private FileStream? _stream;

    public FilePrinter(string path)
    {
        _path = path;
        _streamFactory = () => new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
    }

    public FilePrinter(string path, Func<FileStream> streamFactory)
    {
        _path = path;
        _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
    }

    public async Task ConnectAsync()
    {
        using var _ = await _lock.LockAsync().ConfigureAwait(false);
        _stream ??= _streamFactory();
    }

    public async Task SendAsync(byte[] data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));

        using var _ = await _lock.LockAsync().ConfigureAwait(false);

        if (_stream is null)
            throw new InvalidOperationException("Printer not connected.");

        await _stream.WriteAsync(data, 0, data.Length, CancellationToken.None).ConfigureAwait(false);
        await _stream.FlushAsync(CancellationToken.None).ConfigureAwait(false);
    }

    public Task<byte[]> ReadStatusAsync(byte n = 1, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Status reads are not supported for file/device based printers.");
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _stream = null;
    }
}
