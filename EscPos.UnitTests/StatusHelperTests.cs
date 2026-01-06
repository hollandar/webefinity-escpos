using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscPos.Printers;

namespace EscPos.UnitTests;

public sealed class StatusHelperTests
{
    public static IEnumerable<object?[]> PaperStatusCases()
    {
        yield return new object?[] { null, new[] { EscPosPaperStatus.Unknown } };
        yield return new object?[] { Array.Empty<byte>(), new[] { EscPosPaperStatus.Unknown } };
        yield return new object?[] { new byte[] { 0x00 }, new[] { EscPosPaperStatus.Ok } };
        yield return new object?[] { new byte[] { 0x04 }, new[] { EscPosPaperStatus.NearEnd } };
        yield return new object?[] { new byte[] { 0x08 }, new[] { EscPosPaperStatus.Out } };
        yield return new object?[] { new byte[] { 0x0C }, new[] { EscPosPaperStatus.Out, EscPosPaperStatus.NearEnd } };
    }

    [Theory]
    [MemberData(nameof(PaperStatusCases))]
    public void DecodePaperStatus_CoversAllOutcomes(byte[]? bytes, EscPosPaperStatus[] expected)
    {
        Assert.Equal(expected, StatusHelper.DecodePaperStatus(bytes).ToArray());
    }

    public static IEnumerable<object?[]> DrawerStatusCases()
    {
        yield return new object?[] { null, new[] { EscPosDrawerStatus.Unknown } };
        yield return new object?[] { Array.Empty<byte>(), new[] { EscPosDrawerStatus.Unknown } };
        yield return new object?[] { new byte[] { 0x00 }, new[] { EscPosDrawerStatus.Closed } };
        yield return new object?[] { new byte[] { 0x04 }, new[] { EscPosDrawerStatus.Open } };
    }

    [Theory]
    [MemberData(nameof(DrawerStatusCases))]
    public void DecodeDrawerStatus_CoversAllOutcomes(byte[]? bytes, EscPosDrawerStatus[] expected)
    {
        Assert.Equal(expected, StatusHelper.DecodeDrawerStatus(bytes).ToArray());
    }

    public static IEnumerable<object?[]> PrinterStateCases()
    {
        yield return new object?[] { null, new[] { EscPosPrinterState.Unknown } };
        yield return new object?[] { Array.Empty<byte>(), new[] { EscPosPrinterState.Unknown } };
        yield return new object?[] { new byte[] { 0x00 }, new[] { EscPosPrinterState.Online } };
        yield return new object?[] { new byte[] { 0x08 }, new[] { EscPosPrinterState.Offline } };
        yield return new object?[] { new byte[] { 0x20 }, new[] { EscPosPrinterState.CoverOpen } };
        yield return new object?[] { new byte[] { 0x40 }, new[] { EscPosPrinterState.Error } };
        yield return new object?[] { new byte[] { 0x68 }, new[] { EscPosPrinterState.Offline, EscPosPrinterState.CoverOpen, EscPosPrinterState.Error } };
    }

    [Theory]
    [MemberData(nameof(PrinterStateCases))]
    public void DecodePrinterState_CoversAllOutcomes(byte[]? bytes, EscPosPrinterState[] expected)
    {
        Assert.Equal(expected, StatusHelper.DecodePrinterState(bytes).ToArray());
    }

    [Fact]
    public async Task StatusHelper_UsesPrinterResponses_ForAsyncMethods()
    {
        var printer = new TestPrinter(new Dictionary<byte, byte[]>
        {
            [1] = new byte[] { 0x00 }, // paper/printer state = OK/Online
            [2] = new byte[] { 0x04 }, // drawer = open
        });

        var helper = new StatusHelper(printer);

        var raw = await helper.RequestStatusAsync(1);
        Assert.Equal(new byte[] { 0x00 }, raw);

        var paper = (await helper.GetPaperStatusAsync(1)).ToArray();
        Assert.Equal(new[] { EscPosPaperStatus.Ok }, paper);

        var drawer = (await helper.GetDrawerStatusAsync(2)).ToArray();
        Assert.Equal(new[] { EscPosDrawerStatus.Open }, drawer);

        var state = (await helper.GetPrinterStateAsync(1)).ToArray();
        Assert.Equal(new[] { EscPosPrinterState.Online }, state);
    }
}
