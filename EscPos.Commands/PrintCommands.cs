using System;
using System.Collections.Generic;
using System.Text;

namespace EscPos.Commands;

public static class PrintCommands
{
    // Helpers
    private static byte[] Bytes(params byte[] bytes) => bytes;

    public static byte[] Text(string text, Encoding? encoding = null)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        encoding ??= Encoding.ASCII;
        return encoding.GetBytes(text);
    }

    public static byte[] PrintLine(string text = "", Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;
        var textBytes = Text(text, encoding);
        var lf = LineFeed();

        var result = new byte[textBytes.Length + lf.Length];
        Buffer.BlockCopy(textBytes, 0, result, 0, textBytes.Length);
        Buffer.BlockCopy(lf, 0, result, textBytes.Length, lf.Length);
        return result;
    }

    public static byte[] RequestStatus(byte n = 1) => Bytes(0x1D, 0x72, n); // GS r n

    // Print position / spacing
    public static byte[] HorizontalTab() => Bytes(0x09); // HT

    public static byte[] LineFeed(int count = 1)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0) return Array.Empty<byte>();
        if (count == 1) return Bytes(0x0A);

        var bytes = new byte[count];
        bytes.AsSpan().Fill(0x0A);
        return bytes;
    }

    public static byte[] CarriageReturn() => Bytes(0x0D); // CR
    public static byte[] FormFeed() => Bytes(0x0C); // FF

    public static byte[] PrintAndLineFeed() => Bytes(0x1B, 0x0A); // ESC LF
    public static byte[] PrintAndReturnToStandardModeInPageMode() => Bytes(0x0C); // same as FF

    public static byte[] SetRightSideCharacterSpacing(byte n) => Bytes(0x1B, 0x20, n); // ESC SP n

    public static byte[] SetLineSpacingDefault() => Bytes(0x1B, 0x32); // ESC 2
    public static byte[] SetLineSpacing(byte n) => Bytes(0x1B, 0x33, n); // ESC 3 n

    public static byte[] SetPrintPositionRelative(sbyte n) => unchecked(Bytes(0x1B, 0x5C, (byte)n, 0x00)); // ESC \ nL nH (signed convenience for small moves)
    public static byte[] SetPrintPositionRelative(ushort n) => Bytes(0x1B, 0x5C, (byte)(n & 0xFF), (byte)(n >> 8));
    public static byte[] SetAbsolutePrintPosition(ushort n) => Bytes(0x1B, 0x24, (byte)(n & 0xFF), (byte)(n >> 8)); // ESC $ nL nH

    public static byte[] SetLeftMargin(ushort n) => Bytes(0x1D, 0x4C, (byte)(n & 0xFF), (byte)(n >> 8)); // GS L nL nH

    // Initialize
    public static byte[] Initialize() => Bytes(0x1B, 0x40); // ESC @

    // Text style / emphasis
    public static byte[] BoldOn() => Bytes(0x1B, 0x45, 0x01);
    public static byte[] BoldOff() => Bytes(0x1B, 0x45, 0x00);
    public static byte[] Bold(bool on) => Bytes(0x1B, 0x45, (byte)(on ? 1 : 0));

    public static byte[] UnderlineOff() => Bytes(0x1B, 0x2D, 0x00); // ESC - 0
    public static byte[] Underline1Dot() => Bytes(0x1B, 0x2D, 0x01); // ESC - 1
    public static byte[] Underline2Dot() => Bytes(0x1B, 0x2D, 0x02); // ESC - 2
    public static byte[] Underline(byte mode) => Bytes(0x1B, 0x2D, mode);

    public static byte[] DoubleStrikeOn() => Bytes(0x1B, 0x47, 0x01); // ESC G 1
    public static byte[] DoubleStrikeOff() => Bytes(0x1B, 0x47, 0x00); // ESC G 0

    public static byte[] InvertOn() => Bytes(0x1D, 0x42, 0x01); // GS B 1
    public static byte[] InvertOff() => Bytes(0x1D, 0x42, 0x00); // GS B 0
    public static byte[] Invert(bool on) => Bytes(0x1D, 0x42, (byte)(on ? 1 : 0));

    // Character size: GS ! n (width in low nibble, height in high nibble)
    // width/height multipliers are 1..8 (value written is multiplier-1)
    public static byte[] SetCharacterSize(byte widthMultiplier, byte heightMultiplier)
    {
        if (widthMultiplier is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(widthMultiplier));
        if (heightMultiplier is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(heightMultiplier));

        byte n = (byte)(((heightMultiplier - 1) << 4) | (widthMultiplier - 1));
        return Bytes(0x1D, 0x21, n);
    }

    public static byte[] SetCharacterSizeNormal() => Bytes(0x1D, 0x21, 0x00);

    // Font: ESC M n (0 = Font A, 1 = Font B)
    public static byte[] SelectFontA() => Bytes(0x1B, 0x4D, 0x00);
    public static byte[] SelectFontB() => Bytes(0x1B, 0x4D, 0x01);
    public static byte[] SelectFont(byte n) => Bytes(0x1B, 0x4D, n);

    // Rotation
    public static byte[] Rotate90On() => Bytes(0x1B, 0x56, 0x01); // ESC V 1
    public static byte[] Rotate90Off() => Bytes(0x1B, 0x56, 0x00); // ESC V 0

    // Alignment
    public static byte[] AlignLeft() => Bytes(0x1B, 0x61, 0x00);
    public static byte[] AlignCenter() => Bytes(0x1B, 0x61, 0x01);
    public static byte[] AlignRight() => Bytes(0x1B, 0x61, 0x02);
    public static byte[] Align(byte n) => Bytes(0x1B, 0x61, n);

    // Paper feeding
    public static byte[] FeedLines(byte n) => Bytes(0x1B, 0x64, n); // ESC d n
    public static byte[] FeedDots(byte n) => Bytes(0x1B, 0x4A, n); // ESC J n

    // Cut
    public static byte[] CutFull() => Bytes(0x1D, 0x56, 0x00);
    public static byte[] CutPartial() => Bytes(0x1D, 0x56, 0x01);
    public static byte[] CutFull(byte feed) => Bytes(0x1D, 0x56, 0x41, feed); // GS V 65 n
    public static byte[] CutPartial(byte feed) => Bytes(0x1D, 0x56, 0x42, feed); // GS V 66 n

    // Cash drawer (m = 0 or 1 for drawer pin 2 or 5 in many setups)
    public static byte[] PulseCashDrawer(byte drawer, byte onTime = 120, byte offTime = 240) => Bytes(0x1B, 0x70, drawer, onTime, offTime); // ESC p m t1 t2
    public static byte[] PulseCashDrawer2(byte onTime = 120, byte offTime = 240) => PulseCashDrawer(0, onTime, offTime);
    public static byte[] PulseCashDrawer5(byte onTime = 120, byte offTime = 240) => PulseCashDrawer(1, onTime, offTime);

    // Beeper (supported on some configurations)
    public static byte[] Beep(byte times, byte duration) => Bytes(0x1B, 0x42, times, duration); // ESC B n t

    // Code pages / internationalization (actual supported pages depend on firmware)
    public static byte[] SelectCodePage(CodePage codePage) => Bytes(0x1B, 0x74, (byte)codePage); // ESC t n
    public static byte[] SelectCodePage(byte n) => Bytes(0x1B, 0x74, n); // ESC t n
    public static byte[] SelectInternationalCharacterSet(byte n) => Bytes(0x1B, 0x52, n); // ESC R n

    // Raster bit image (GS v 0)
    // rasterData must be 1bpp, packed MSB->LSB, row-major.
    // widthBytes is number of bytes per row (typically (widthPixels + 7)/8)
    // heightPixels is number of rows.
    // mode: 0 = normal, 1 = double width, 2 = double height, 3 = quadruple
    public static byte[] PrintRasterImage(ReadOnlySpan<byte> rasterData, ushort widthBytes, ushort heightPixels, byte mode = 0)
    {
        if (widthBytes == 0) throw new ArgumentOutOfRangeException(nameof(widthBytes));
        if (heightPixels == 0) throw new ArgumentOutOfRangeException(nameof(heightPixels));

        var expected = widthBytes * heightPixels;
        if (rasterData.Length != expected)
            throw new ArgumentException($"rasterData length must be widthBytes * heightPixels ({expected}), but was {rasterData.Length}.", nameof(rasterData));

        // GS v 0 m xL xH yL yH d...
        var result = new byte[8 + rasterData.Length];
        result[0] = 0x1D;
        result[1] = 0x76;
        result[2] = 0x30;
        result[3] = mode;
        result[4] = (byte)(widthBytes & 0xFF);
        result[5] = (byte)(widthBytes >> 8);
        result[6] = (byte)(heightPixels & 0xFF);
        result[7] = (byte)(heightPixels >> 8);

        rasterData.CopyTo(result.AsSpan(8));
        return result;
    }

    // Print a BMP image by converting it to 1bpp raster and printing using GS ( L.
    // Supports: 1-bit, 4-bit, 8-bit, 16-bit, 24-bit, and 32-bit BMPs
    // Supports: Uncompressed (BI_RGB) and RLE compression (BI_RLE4, BI_RLE8)
    public static byte[] PrintBmpImage(ReadOnlySpan<byte> bmpData, byte fn = 50, byte c = 0x31)
    {
        if (bmpData.Length < 54) throw new ArgumentException("BMP data too small.", nameof(bmpData));
        if (bmpData[0] != (byte)'B' || bmpData[1] != (byte)'M') throw new ArgumentException("Not a BMP (missing BM header).", nameof(bmpData));

        var s = bmpData;

        static int ReadInt32LE(ReadOnlySpan<byte> data, int offset) =>
            data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);

        static int ReadInt16LE(ReadOnlySpan<byte> data, int offset) =>
            data[offset] | (data[offset + 1] << 8);

        var pixelOffset = ReadInt32LE(s, 10);
        var dibSize = ReadInt32LE(s, 14);
        if (dibSize < 40) throw new ArgumentException("Unsupported BMP DIB header.", nameof(bmpData));

        var width = ReadInt32LE(s, 18);
        var height = ReadInt32LE(s, 22);
        var planes = ReadInt16LE(s, 26);
        var bpp = ReadInt16LE(s, 28);
        var compression = ReadInt32LE(s, 30);

        if (planes != 1) throw new ArgumentException("Unsupported BMP planes.", nameof(bmpData));
        if (bpp is not (1 or 4 or 8 or 16 or 24 or 32))
            throw new ArgumentException($"Unsupported BMP bit depth: {bpp}bpp. Supported: 1, 4, 8, 16, 24, 32.", nameof(bmpData));
        
        if (compression is not (0 or 1 or 2))
            throw new ArgumentException($"Unsupported BMP compression: {compression}. Supported: 0 (BI_RGB), 1 (BI_RLE8), 2 (BI_RLE4).", nameof(bmpData));

        if (compression == 1 && bpp != 8)
            throw new ArgumentException("BI_RLE8 compression requires 8bpp BMP.", nameof(bmpData));
        if (compression == 2 && bpp != 4)
            throw new ArgumentException("BI_RLE4 compression requires 4bpp BMP.", nameof(bmpData));

        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(bmpData), "BMP width must be positive.");

        var absHeight = Math.Abs(height);
        if (absHeight <= 0) throw new ArgumentOutOfRangeException(nameof(bmpData), "BMP height must be non-zero.");

        if (width > 2047) throw new ArgumentOutOfRangeException(nameof(bmpData), "BMP width exceeds TM-T20 range (1..2047 dots).");
        if (absHeight > 1662) throw new ArgumentOutOfRangeException(nameof(bmpData), "BMP height exceeds TM-T20 range for by=1 (1..1662 dots).");

        var topDown = height < 0;

        // Read palette if needed (for 1, 4, 8 bpp)
        Span<(byte r, byte g, byte b)> palette = stackalloc (byte, byte, byte)[256];
        if (bpp <= 8)
        {
            var paletteEntries = 1 << bpp;
            var paletteOffset = 14 + dibSize;
            
            if (paletteOffset + paletteEntries * 4 > s.Length)
                throw new ArgumentException("BMP palette data is truncated.", nameof(bmpData));

            for (var i = 0; i < paletteEntries; i++)
            {
                var offset = paletteOffset + i * 4;
                palette[i] = (s[offset + 2], s[offset + 1], s[offset + 0]); // BGR -> RGB
            }
        }

        // Helper to convert RGB to grayscale and determine if pixel should be black
        static bool IsBlackPixel(byte r, byte g, byte b)
        {
            var luminance = (r * 30 + g * 59 + b * 11) / 100;
            return luminance < 128;
        }

        var widthBytes = (width + 7) / 8;
        var data = new byte[widthBytes * absHeight];

        // Decode pixel data based on format
        if (compression == 0) // BI_RGB (uncompressed)
        {
            var rowStride = bpp switch
            {
                1 => ((width + 31) / 32) * 4,
                4 => ((width + 7) / 8 + 3) & ~3,
                8 => (width + 3) & ~3,
                16 => ((width * 2) + 3) & ~3,
                24 => ((width * 3) + 3) & ~3,
                32 => width * 4,
                _ => throw new InvalidOperationException()
            };

            var pixelBytesRequired = rowStride * absHeight;
            if (pixelOffset < 0 || pixelOffset > s.Length)
                throw new ArgumentException("BMP pixel data offset is invalid.", nameof(bmpData));
            if (pixelOffset + pixelBytesRequired > s.Length)
                throw new ArgumentException("BMP pixel data is truncated.", nameof(bmpData));

            for (var y = 0; y < absHeight; y++)
            {
                var srcY = topDown ? y : (absHeight - 1 - y);
                var rowStart = pixelOffset + (srcY * rowStride);

                for (var xPixel = 0; xPixel < width; xPixel++)
                {
                    byte r, g, b;

                    switch (bpp)
                    {
                        case 1:
                            {
                                var byteIndex = xPixel / 8;
                                var bitIndex = 7 - (xPixel % 8);
                                var paletteIdx = (s[rowStart + byteIndex] >> bitIndex) & 1;
                                (r, g, b) = palette[paletteIdx];
                                break;
                            }
                        case 4:
                            {
                                var byteIndex = xPixel / 2;
                                var nibbleShift = (1 - (xPixel % 2)) * 4;
                                var paletteIdx = (s[rowStart + byteIndex] >> nibbleShift) & 0x0F;
                                (r, g, b) = palette[paletteIdx];
                                break;
                            }
                        case 8:
                            {
                                var paletteIdx = s[rowStart + xPixel];
                                (r, g, b) = palette[paletteIdx];
                                break;
                            }
                        case 16:
                            {
                                var px = rowStart + (xPixel * 2);
                                var pixel16 = (ushort)(s[px] | (s[px + 1] << 8));
                                // Assume RGB555 (most common), could also be RGB565
                                r = (byte)(((pixel16 >> 10) & 0x1F) * 255 / 31);
                                g = (byte)(((pixel16 >> 5) & 0x1F) * 255 / 31);
                                b = (byte)((pixel16 & 0x1F) * 255 / 31);
                                break;
                            }
                        case 24:
                            {
                                var px = rowStart + (xPixel * 3);
                                b = s[px + 0];
                                g = s[px + 1];
                                r = s[px + 2];
                                break;
                            }
                        case 32:
                            {
                                var px = rowStart + (xPixel * 4);
                                b = s[px + 0];
                                g = s[px + 1];
                                r = s[px + 2];
                                // s[px + 3] is alpha, ignored
                                break;
                            }
                        default:
                            throw new InvalidOperationException();
                    }

                    if (!IsBlackPixel(r, g, b)) continue;

                    var dstIndex = (y * widthBytes) + (xPixel / 8);
                    var bit = 7 - (xPixel % 8);
                    data[dstIndex] |= (byte)(1 << bit);
                }
            }
        }
        else if (compression == 1) // BI_RLE8
        {
            if (pixelOffset < 0 || pixelOffset > s.Length)
                throw new ArgumentException("BMP pixel data offset is invalid.", nameof(bmpData));

            var decodedPixels = new byte[width * absHeight];
            var srcIdx = pixelOffset;
            var dstX = 0;
            var dstY = topDown ? 0 : absHeight - 1;
            var yIncrement = topDown ? 1 : -1;

            while (srcIdx < s.Length - 1)
            {
                var count = s[srcIdx++];
                var value = s[srcIdx++];

                if (count == 0) // Escape
                {
                    if (value == 0) // End of line
                    {
                        dstX = 0;
                        dstY += yIncrement;
                        if (dstY < 0 || dstY >= absHeight) break;
                    }
                    else if (value == 1) // End of bitmap
                    {
                        break;
                    }
                    else if (value == 2) // Delta
                    {
                        if (srcIdx >= s.Length - 1) break;
                        var dx = s[srcIdx++];
                        var dy = s[srcIdx++];
                        dstX += dx;
                        dstY += dy * yIncrement;
                        if (dstY < 0 || dstY >= absHeight) break;
                    }
                    else // Absolute mode
                    {
                        var absCount = value;
                        for (var i = 0; i < absCount && srcIdx < s.Length; i++)
                        {
                            var paletteIdx = s[srcIdx++];
                            if (dstX < width && dstY >= 0 && dstY < absHeight)
                            {
                                decodedPixels[dstY * width + dstX] = paletteIdx;
                                dstX++;
                            }
                        }
                        // Align to word boundary
                        if ((absCount & 1) != 0) srcIdx++;
                    }
                }
                else // Encoded mode
                {
                    for (var i = 0; i < count; i++)
                    {
                        if (dstX < width && dstY >= 0 && dstY < absHeight)
                        {
                            decodedPixels[dstY * width + dstX] = value;
                            dstX++;
                        }
                    }
                }
            }

            // Convert decoded pixels to 1bpp
            for (var y = 0; y < absHeight; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var paletteIdx = decodedPixels[y * width + x];
                    var (r, g, b_val) = palette[paletteIdx];

                    if (!IsBlackPixel(r, g, b_val)) continue;

                    var dstIndex = (y * widthBytes) + (x / 8);
                    var bit = 7 - (x % 8);
                    data[dstIndex] |= (byte)(1 << bit);
                }
            }
        }
        else if (compression == 2) // BI_RLE4
        {
            if (pixelOffset < 0 || pixelOffset > s.Length)
                throw new ArgumentException("BMP pixel data offset is invalid.", nameof(bmpData));

            var decodedPixels = new byte[width * absHeight];
            var srcIdx = pixelOffset;
            var dstX = 0;
            var dstY = topDown ? 0 : absHeight - 1;
            var yIncrement = topDown ? 1 : -1;

            while (srcIdx < s.Length - 1)
            {
                var count = s[srcIdx++];
                var value = s[srcIdx++];

                if (count == 0) // Escape
                {
                    if (value == 0) // End of line
                    {
                        dstX = 0;
                        dstY += yIncrement;
                        if (dstY < 0 || dstY >= absHeight) break;
                    }
                    else if (value == 1) // End of bitmap
                    {
                        break;
                    }
                    else if (value == 2) // Delta
                    {
                        if (srcIdx >= s.Length - 1) break;
                        var dx = s[srcIdx++];
                        var dy = s[srcIdx++];
                        dstX += dx;
                        dstY += dy * yIncrement;
                        if (dstY < 0 || dstY >= absHeight) break;
                    }
                    else // Absolute mode
                    {
                        var absCount = value;
                        for (var i = 0; i < absCount && srcIdx < s.Length; i++)
                        {
                            byte nibble;
                            if ((i & 1) == 0)
                            {
                                nibble = (byte)((s[srcIdx] >> 4) & 0x0F);
                            }
                            else
                            {
                                nibble = (byte)(s[srcIdx++] & 0x0F);
                            }

                            if (dstX < width && dstY >= 0 && dstY < absHeight)
                            {
                                decodedPixels[dstY * width + dstX] = nibble;
                                dstX++;
                            }
                        }
                        // Align to word boundary
                        if (((absCount + 1) / 2 & 1) != 0) srcIdx++;
                    }
                }
                else // Encoded mode
                {
                    var nibble1 = (byte)((value >> 4) & 0x0F);
                    var nibble2 = (byte)(value & 0x0F);

                    for (var i = 0; i < count; i++)
                    {
                        if (dstX < width && dstY >= 0 && dstY < absHeight)
                        {
                            decodedPixels[dstY * width + dstX] = (i & 1) == 0 ? nibble1 : nibble2;
                            dstX++;
                        }
                    }
                }
            }

            // Convert decoded pixels to 1bpp
            for (var y = 0; y < absHeight; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var paletteIdx = decodedPixels[y * width + x];
                    var (r, g, b_val) = palette[paletteIdx];

                    if (!IsBlackPixel(r, g, b_val)) continue;

                    var dstIndex = (y * widthBytes) + (x / 8);
                    var bit = 7 - (x % 8);
                    data[dstIndex] |= (byte)(1 << bit);
                }
            }
        }

        const byte m = 0x30;
        const byte fnStore = 0x70;
        const byte a = 0x30;
        const byte bx = 0x01;
        const byte by = 0x01;

        var xDots = (ushort)width;
        var yDots = (ushort)absHeight;

        var storePayloadLen = 2 /*m,fn*/ + 1 /*a*/ + 1 /*bx*/ + 1 /*by*/ + 1 /*c*/ + 2 /*x*/ + 2 /*y*/ + data.Length;
        if (storePayloadLen > 0xFFFF) throw new ArgumentOutOfRangeException(nameof(bmpData), "Graphics payload too large.");

        var store = new byte[3 + 2 + storePayloadLen];
        store[0] = 0x1D;
        store[1] = 0x28;
        store[2] = 0x4C;
        store[3] = (byte)(storePayloadLen & 0xFF);
        store[4] = (byte)(storePayloadLen >> 8);
        store[5] = m;
        store[6] = fnStore;
        store[7] = a;
        store[8] = bx;
        store[9] = by;
        store[10] = c;
        store[11] = (byte)(xDots & 0xFF);
        store[12] = (byte)(xDots >> 8);
        store[13] = (byte)(yDots & 0xFF);
        store[14] = (byte)(yDots >> 8);
        data.CopyTo(store.AsSpan(15));

        // Print buffered graphics
        if (fn is not 2 and not 50)
            throw new ArgumentOutOfRangeException(nameof(fn), "For GS ( L print, fn must be 2 or 50.");

        var print = new byte[]
        {
            0x1D, 0x28, 0x4C,
            0x02, 0x00,
            0x30, fn,
        };

        var result = new byte[store.Length + print.Length];
        Buffer.BlockCopy(store, 0, result, 0, store.Length);
        Buffer.BlockCopy(print, 0, result, store.Length, print.Length);
        return result;
    }

    // Barcode (basic)
    public static byte[] SetBarcodeHeight(byte n) => Bytes(0x1D, 0x68, n); // GS h n
    public static byte[] SetBarcodeWidth(byte n) => Bytes(0x1D, 0x77, n); // GS w n
    public static byte[] SetHriPosition(byte n) => Bytes(0x1D, 0x48, n); // GS H n
    public static byte[] SetHriFont(byte n) => Bytes(0x1D, 0x66, n); // GS f n

    // Print barcode: GS k m d1..dk NUL (m 0-6) or GS k m n d1..dn (m 65-73)
    // Provide Code 128 etc. via m=73 format, with length prefix.
    public static byte[] PrintBarcode(byte m, ReadOnlySpan<byte> data)
    {
        if (m <= 6)
        {
            var result = new byte[3 + data.Length + 1];
            result[0] = 0x1D; result[1] = 0x6B; result[2] = m;
            data.CopyTo(result.AsSpan(3));
            result[^1] = 0x00;
            return result;
        }

        if (data.Length > 255) throw new ArgumentOutOfRangeException(nameof(data), "Barcode data too long (max 255 for length-prefixed forms)." );
        var res2 = new byte[4 + data.Length];
        res2[0] = 0x1D; res2[1] = 0x6B; res2[2] = m; res2[3] = (byte)data.Length;
        data.CopyTo(res2.AsSpan(4));
        return res2;
    }

    // QR Code (model 2) - typical Epson sequence using GS ( k
    public static byte[] QrCodeSelectModel2() => Bytes(0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00);
    public static byte[] QrCodeSetModuleSize(byte size)
    {
        if (size is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(size));
        return Bytes(0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, size);
    }

    public static byte[] QrCodeSetErrorCorrectionLevel(byte level)
    {
        // 48(L),49(M),50(Q),51(H)
        if (level is < 48 or > 51) throw new ArgumentOutOfRangeException(nameof(level));
        return Bytes(0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, level);
    }

    public static byte[] QrCodeStoreData(ReadOnlySpan<byte> data)
    {
        // pL pH = data.Length + 3
        var len = data.Length + 3;
        if (len > 0xFFFF) throw new ArgumentOutOfRangeException(nameof(data));

        var result = new byte[8 + data.Length];
        result[0] = 0x1D;
        result[1] = 0x28;
        result[2] = 0x6B;
        result[3] = (byte)(len & 0xFF);
        result[4] = (byte)(len >> 8);
        result[5] = 0x31;
        result[6] = 0x50;
        result[7] = 0x30;
        data.CopyTo(result.AsSpan(8));
        return result;
    }

    public static byte[] QrCodePrint() => Bytes(0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30);

    // Status (real-time) - may require bidirectional support; useful as raw commands
    public static byte[] RealTimeStatusTransmission(byte n) => Bytes(0x10, 0x04, n); // DLE EOT n
    public static byte[] RealTimeRequestToPrinter(byte n) => Bytes(0x10, 0x05, n); // DLE ENQ n

    // Misc
    public static byte[] SetPrintMode(byte n) => Bytes(0x1B, 0x21, n); // ESC ! n
    public static byte[] ResetPrintMode() => Bytes(0x1B, 0x21, 0x00);

    public static byte[] EnableSmoothing(bool on) => Bytes(0x1D, 0x62, (byte)(on ? 1 : 0)); // GS b n (model dependent)
}
