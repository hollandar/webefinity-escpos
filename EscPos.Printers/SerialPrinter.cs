using EscPos.Commands;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace EscPos.Printers;

public sealed class SerialPrinter : IPrinter
{
    private readonly string _portName;
    private readonly int _baudRate;
    private readonly Parity _parity;
    private readonly int _dataBits;
    private readonly StopBits _stopBits;
    private readonly Handshake _handshake;
    private readonly PrinterSemaphore _lock = new();
    private SerialPort? _serialPort;

    public SerialPrinter(
        string portName,
        int baudRate = 9600,
        Parity parity = Parity.None,
        int dataBits = 8,
        StopBits stopBits = StopBits.One,
        Handshake handshake = Handshake.None)
    {
        _portName = portName;
        _baudRate = baudRate;
        _parity = parity;
        _dataBits = dataBits;
        _stopBits = stopBits;
        _handshake = handshake;
    }

    public async Task ConnectAsync()
    {
        using var _ = await _lock.LockAsync().ConfigureAwait(false);

        if (_serialPort is { IsOpen: true })
            return;

        _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits)
        {
            Handshake = _handshake,
        };

        _serialPort.Open();
    }

    public async Task SendAsync(byte[] data)
    {
        using var _ = await _lock.LockAsync().ConfigureAwait(false);

        if (_serialPort is not { IsOpen: true })
            throw new InvalidOperationException("Printer not connected.");

        _serialPort.Write(data, 0, data.Length);
    }

    public async Task<byte[]> ReadStatusAsync(byte n = 1, CancellationToken cancellationToken = default)
    {
        using var _ = await _lock.LockAsync(cancellationToken).ConfigureAwait(false);

        if (_serialPort is not { IsOpen: true })
            throw new InvalidOperationException("Printer not connected.");

        var request = PrintCommands.RequestStatus(n);
        _serialPort.Write(request, 0, request.Length);

        // Wait briefly for response; SerialPort is sync-based.
        await Task.Delay(50, cancellationToken).ConfigureAwait(false);

        var available = _serialPort.BytesToRead;
        if (available <= 0)
            return Array.Empty<byte>();

        var result = new byte[Math.Min(available, 32)];
        var read = _serialPort.Read(result, 0, result.Length);
        if (read == result.Length)
            return result;

        var trimmed = new byte[read];
        Buffer.BlockCopy(result, 0, trimmed, 0, read);
        return trimmed;
    }

    public void Dispose()
    {
        if (_serialPort is null)
            return;

        try
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }
        finally
        {
            _serialPort.Dispose();
            _serialPort = null;
        }
    }
}
