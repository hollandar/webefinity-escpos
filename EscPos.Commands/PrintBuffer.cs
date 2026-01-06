using EscPos.Commands;
using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace EscPos.Console;

public sealed class PrintBuffer : IDisposable
{
    private readonly MemoryStream _stream;

    public PrintBuffer(int initialCapacity = 1024)
    {
        if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        _stream = new MemoryStream(initialCapacity);
    }

    public int Length => checked((int)_stream.Length);

    public PrintBuffer Write(ReadOnlySpan<byte> bytes)
    {
        _stream.Write(bytes);
        return this;
    }

    public PrintBuffer Write(byte[] bytes)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        _stream.Write(bytes, 0, bytes.Length);
        return this;
    }

    public PrintBuffer Write(string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;
        var byteCount = encoding.GetByteCount(text);
        var rented = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            var written = encoding.GetBytes(text, 0, text.Length, rented, 0);
            _stream.Write(rented, 0, written);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }

        return this;
    }

    public PrintBuffer WriteLine(string text = "", Encoding? encoding = null, int lineFeedCount = 1)
    {
        Write(text, encoding);
        Write(PrintCommands.LineFeed(lineFeedCount));
        return this;
    }

    public byte[] ToArray() => _stream.ToArray();

    public void Clear() => _stream.SetLength(0);

    public void Dispose() => _stream.Dispose();
}
