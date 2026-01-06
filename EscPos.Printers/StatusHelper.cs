using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EscPos.Printers;

public sealed class StatusHelper
{
    private readonly IPrinter _printer;

    public StatusHelper(IPrinter printer)
    {
        _printer = printer ?? throw new ArgumentNullException(nameof(printer));
    }

    public Task<byte[]> RequestStatusAsync(byte n = 1, CancellationToken cancellationToken = default)
        => _printer.ReadStatusAsync(n, cancellationToken);

    public async Task<IEnumerable<EscPosPaperStatus>> GetPaperStatusAsync(byte n = 1, CancellationToken cancellationToken = default)
    {
        var bytes = await _printer.ReadStatusAsync(n, cancellationToken).ConfigureAwait(false);
        return DecodePaperStatus(bytes);
    }

    public async Task<IEnumerable<EscPosDrawerStatus>> GetDrawerStatusAsync(byte n = 2, CancellationToken cancellationToken = default)
    {
        var bytes = await _printer.ReadStatusAsync(n, cancellationToken).ConfigureAwait(false);
        return DecodeDrawerStatus(bytes);
    }

    public async Task<IEnumerable<EscPosPrinterState>> GetPrinterStateAsync(byte n = 1, CancellationToken cancellationToken = default)
    {
        var bytes = await _printer.ReadStatusAsync(n, cancellationToken).ConfigureAwait(false);
        return DecodePrinterState(bytes);
    }

    public static IEnumerable<EscPosPaperStatus> DecodePaperStatus(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
        {
            yield return EscPosPaperStatus.Unknown;
            yield break;
        }

        // Common mappings seen in ESC/POS status responses:
        // 0x04: paper near-end
        // 0x08: paper end
        var b = bytes[0];

        var any = false;

        if ((b & 0x08) != 0)
        {
            any = true;
            yield return EscPosPaperStatus.Out;
        }

        if ((b & 0x04) != 0)
        {
            any = true;
            yield return EscPosPaperStatus.NearEnd;
        }

        if (!any)
            yield return EscPosPaperStatus.Ok;
    }

    public static IEnumerable<EscPosDrawerStatus> DecodeDrawerStatus(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
        {
            yield return EscPosDrawerStatus.Unknown;
            yield break;
        }

        // Common mapping for kick-out connector pin status: bit 2.
        // 0 = drawer closed
        // 1 = drawer open
        var b = bytes[0];
        var open = (b & 0x04) != 0;
        yield return open ? EscPosDrawerStatus.Open : EscPosDrawerStatus.Closed;
    }

    public static IEnumerable<EscPosPrinterState> DecodePrinterState(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
        {
            yield return EscPosPrinterState.Unknown;
            yield break;
        }

        // Very common flags (vary by command used and printer):
        // 0x08: offline
        // 0x20: cover open
        // 0x40: error
        var b = bytes[0];

        var any = false;

        if ((b & 0x08) != 0)
        {
            any = true;
            yield return EscPosPrinterState.Offline;
        }

        if ((b & 0x20) != 0)
        {
            any = true;
            yield return EscPosPrinterState.CoverOpen;
        }

        if ((b & 0x40) != 0)
        {
            any = true;
            yield return EscPosPrinterState.Error;
        }

        if (!any)
            yield return EscPosPrinterState.Online;
    }
}

public enum EscPosPaperStatus
{
    Unknown = 0,
    Ok = 1,
    NearEnd = 2,
    Out = 3,
}

public enum EscPosDrawerStatus
{
    Unknown = 0,
    Closed = 1,
    Open = 2,
}

public enum EscPosPrinterState
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    CoverOpen = 3,
    Error = 4,
}
