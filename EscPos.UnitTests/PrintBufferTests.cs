using System;
using System.Collections.Generic;
using System.Text;
using EscPos.Commands;
using EscPos.Console;

namespace EscPos.UnitTests;

public sealed class PrintBufferTests
{
    [Fact]
    public void WriteReceipt_UsesAllWriteVariants_AndMatchesExpectedBytes()
    {
        using var buffer = new PrintBuffer(initialCapacity: 0);

        // 1) Write(ReadOnlySpan<byte>)
        buffer.Write(new byte[] { 0x1B, 0x40 }); // Initialize

        // 2) Write(byte[])
        buffer.Write(PrintCommands.AlignCenter());

        // 3) Write(string, Encoding?) default encoding boundary (ASCII)
        buffer.Write("STORE");

        // 4) WriteLine(string, Encoding?, int) boundary lineFeedCount=0
        buffer.WriteLine("", lineFeedCount: 0);

        // 5) WriteLine with explicit UTF-8 encoding and lineFeedCount=1
        buffer.WriteLine("\u00A3", Encoding.UTF8, lineFeedCount: 1); // £ pound sign

        // 6) WriteLine with lineFeedCount>1
        buffer.WriteLine("THANKS", lineFeedCount: 2);

        // 7) WriteLine default params (text="")
        buffer.WriteLine();

        // 8) Another span write for a trailing command
        buffer.Write(PrintCommands.CutFull());

        var actual = buffer.ToArray();

        var expected = new List<byte>();
        expected.AddRange(new byte[] { 0x1B, 0x40 }); // Initialize
        expected.AddRange(new byte[] { 0x1B, 0x61, 0x01 }); // AlignCenter
        expected.AddRange(new byte[] { 0x53, 0x54, 0x4F, 0x52, 0x45 }); // "STORE" ASCII
        // WriteLine("", lineFeedCount:0) => nothing
        expected.AddRange(new byte[] { 0xC2, 0xA3, 0x0A }); // "\u00A3" (£) UTF-8 + LF
        expected.AddRange(new byte[] { 0x54, 0x48, 0x41, 0x4E, 0x4B, 0x53, 0x0A, 0x0A }); // "THANKS" + 2x LF
        expected.AddRange(new byte[] { 0x0A }); // WriteLine() => default text="" then LF
        expected.AddRange(new byte[] { 0x1D, 0x56, 0x00 }); // CutFull

        Assert.Equal(expected.ToArray(), actual);
        Assert.Equal(expected.Count, buffer.Length);
    }

    [Fact]
    public void Constructor_NegativeCapacity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PrintBuffer(-1));
    }

    [Fact]
    public void Write_ByteArray_Null_Throws()
    {
        using var buffer = new PrintBuffer();
        Assert.Throws<ArgumentNullException>(() => buffer.Write((byte[])null!));
    }

    [Fact]
    public void Clear_ResetsLength_AndToArrayIsEmpty()
    {
        using var buffer = new PrintBuffer();
        buffer.Write("ABC");
        Assert.True(buffer.Length > 0);

        buffer.Clear();

        Assert.Equal(0, buffer.Length);
        Assert.Empty(buffer.ToArray());
    }

    [Fact]
    public void WriteLine_NegativeLineFeedCount_Throws()
    {
        using var buffer = new PrintBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.WriteLine("X", lineFeedCount: -1));
    }
}
