using System;
using System.Linq;
using System.Text;
using EscPos.Commands;

namespace EscPos.UnitTests;

public class CommandTests
{
    private static void AssertBytesEqual(byte[] expected, byte[] actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Text_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PrintCommands.Text(null!));
    }

    [Fact]
    public void Text_DefaultEncoding_IsAscii()
    {
        var actual = PrintCommands.Text("ABC");
        AssertBytesEqual(new byte[] { 0x41, 0x42, 0x43 }, actual);
    }

    [Fact]
    public void Text_CustomEncoding_IsUsed()
    {
        var actual = PrintCommands.Text("\u00A3", Encoding.UTF8); // £ pound sign
        AssertBytesEqual(new byte[] { 0xC2, 0xA3 }, actual);
    }

    [Fact]
    public void PrintLine_Default_IsLineFeedOnly()
    {
        var actual = PrintCommands.PrintLine();
        AssertBytesEqual(new byte[] { 0x0A }, actual);
    }

    [Fact]
    public void PrintLine_AppendsLineFeed()
    {
        var actual = PrintCommands.PrintLine("ABC");
        AssertBytesEqual(new byte[] { 0x41, 0x42, 0x43, 0x0A }, actual);
    }

    [Fact]
    public void PrintLine_CustomEncoding_AppendsLineFeed()
    {
        var actual = PrintCommands.PrintLine("£", Encoding.UTF8);
        AssertBytesEqual(new byte[] { 0xC2, 0xA3, 0x0A }, actual);
    }

    [Fact]
    public void RequestStatus_Default()
    {
        var actual = PrintCommands.RequestStatus();
        AssertBytesEqual(new byte[] { 0x1D, 0x72, 0x01 }, actual);
    }

    [Fact]
    public void RequestStatus_Custom()
    {
        var actual = PrintCommands.RequestStatus(0x02);
        AssertBytesEqual(new byte[] { 0x1D, 0x72, 0x02 }, actual);
    }

    [Fact]
    public void HorizontalTab()
    {
        AssertBytesEqual(new byte[] { 0x09 }, PrintCommands.HorizontalTab());
    }

    [Fact]
    public void LineFeed_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrintCommands.LineFeed(-1));
    }

    [Fact]
    public void LineFeed_Zero_ReturnsEmpty()
    {
        AssertBytesEqual(Array.Empty<byte>(), PrintCommands.LineFeed(0));
    }

    [Fact]
    public void LineFeed_One_ReturnsSingleLf()
    {
        AssertBytesEqual(new byte[] { 0x0A }, PrintCommands.LineFeed(1));
    }

    [Fact]
    public void LineFeed_Many_ReturnsRepeatedLf()
    {
        var actual = PrintCommands.LineFeed(3);
        AssertBytesEqual(new byte[] { 0x0A, 0x0A, 0x0A }, actual);
    }

    [Fact]
    public void CarriageReturn()
    {
        AssertBytesEqual(new byte[] { 0x0D }, PrintCommands.CarriageReturn());
    }

    [Fact]
    public void FormFeed()
    {
        AssertBytesEqual(new byte[] { 0x0C }, PrintCommands.FormFeed());
    }

    [Fact]
    public void PrintAndLineFeed()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x0A }, PrintCommands.PrintAndLineFeed());
    }

    [Fact]
    public void PrintAndReturnToStandardModeInPageMode_IsFormFeed()
    {
        AssertBytesEqual(new byte[] { 0x0C }, PrintCommands.PrintAndReturnToStandardModeInPageMode());
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0xFF)]
    public void SetRightSideCharacterSpacing(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x20, n }, PrintCommands.SetRightSideCharacterSpacing(n));
    }

    [Fact]
    public void SetLineSpacingDefault()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x32 }, PrintCommands.SetLineSpacingDefault());
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0xFF)]
    public void SetLineSpacing(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x33, n }, PrintCommands.SetLineSpacing(n));
    }

    [Theory]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)1)]
    [InlineData((sbyte)-1)]
    [InlineData(sbyte.MinValue)]
    [InlineData(sbyte.MaxValue)]
    public void SetPrintPositionRelative_SByte(sbyte n)
    {
        var actual = PrintCommands.SetPrintPositionRelative(n);
        AssertBytesEqual(new byte[] { 0x1B, 0x5C, unchecked((byte)n), 0x00 }, actual);
    }

    [Theory]
    [InlineData((ushort)0x0000)]
    [InlineData((ushort)0x0001)]
    [InlineData((ushort)0x00FF)]
    [InlineData((ushort)0x0100)]
    [InlineData((ushort)0xFFFF)]
    public void SetPrintPositionRelative_UShort(ushort n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x5C, (byte)(n & 0xFF), (byte)(n >> 8) }, PrintCommands.SetPrintPositionRelative(n));
    }

    [Theory]
    [InlineData((ushort)0x0000)]
    [InlineData((ushort)0x0001)]
    [InlineData((ushort)0x00FF)]
    [InlineData((ushort)0x0100)]
    [InlineData((ushort)0xFFFF)]
    public void SetAbsolutePrintPosition(ushort n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x24, (byte)(n & 0xFF), (byte)(n >> 8) }, PrintCommands.SetAbsolutePrintPosition(n));
    }

    [Theory]
    [InlineData((ushort)0x0000)]
    [InlineData((ushort)0x0001)]
    [InlineData((ushort)0x00FF)]
    [InlineData((ushort)0x0100)]
    [InlineData((ushort)0xFFFF)]
    public void SetLeftMargin(ushort n)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x4C, (byte)(n & 0xFF), (byte)(n >> 8) }, PrintCommands.SetLeftMargin(n));
    }

    [Fact]
    public void Initialize()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x40 }, PrintCommands.Initialize());
    }

    [Fact]
    public void BoldOn()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x45, 0x01 }, PrintCommands.BoldOn());
    }

    [Fact]
    public void BoldOff()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x45, 0x00 }, PrintCommands.BoldOff());
    }

    [Theory]
    [InlineData(true, (byte)0x01)]
    [InlineData(false, (byte)0x00)]
    public void Bold(bool on, byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x45, n }, PrintCommands.Bold(on));
    }

    [Fact]
    public void UnderlineOff()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x2D, 0x00 }, PrintCommands.UnderlineOff());
    }

    [Fact]
    public void Underline1Dot()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x2D, 0x01 }, PrintCommands.Underline1Dot());
    }

    [Fact]
    public void Underline2Dot()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x2D, 0x02 }, PrintCommands.Underline2Dot());
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x02)]
    [InlineData((byte)0xFF)]
    public void Underline(byte mode)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x2D, mode }, PrintCommands.Underline(mode));
    }

    [Fact]
    public void DoubleStrikeOn()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x47, 0x01 }, PrintCommands.DoubleStrikeOn());
    }

    [Fact]
    public void DoubleStrikeOff()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x47, 0x00 }, PrintCommands.DoubleStrikeOff());
    }

    [Fact]
    public void InvertOn()
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x42, 0x01 }, PrintCommands.InvertOn());
    }

    [Fact]
    public void InvertOff()
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x42, 0x00 }, PrintCommands.InvertOff());
    }

    [Theory]
    [InlineData(true, (byte)0x01)]
    [InlineData(false, (byte)0x00)]
    public void Invert(bool on, byte n)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x42, n }, PrintCommands.Invert(on));
    }

    [Theory]
    [InlineData((byte)1, (byte)1, (byte)0x00)]
    [InlineData((byte)2, (byte)1, (byte)0x01)]
    [InlineData((byte)1, (byte)2, (byte)0x10)]
    [InlineData((byte)8, (byte)8, (byte)0x77)]
    public void SetCharacterSize_Valid(byte w, byte h, byte n)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x21, n }, PrintCommands.SetCharacterSize(w, h));
    }

    [Theory]
    [InlineData((byte)0, (byte)1)]
    [InlineData((byte)9, (byte)1)]
    public void SetCharacterSize_InvalidWidth_Throws(byte w, byte h)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrintCommands.SetCharacterSize(w, h));
    }

    [Theory]
    [InlineData((byte)1, (byte)0)]
    [InlineData((byte)1, (byte)9)]
    public void SetCharacterSize_InvalidHeight_Throws(byte w, byte h)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrintCommands.SetCharacterSize(w, h));
    }

    [Fact]
    public void SetCharacterSizeNormal()
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x21, 0x00 }, PrintCommands.SetCharacterSizeNormal());
    }

    [Fact]
    public void SelectFontA()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x4D, 0x00 }, PrintCommands.SelectFontA());
    }

    [Fact]
    public void SelectFontB()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x4D, 0x01 }, PrintCommands.SelectFontB());
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void SelectFont(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x4D, n }, PrintCommands.SelectFont(n));
    }

    [Fact]
    public void Rotate90On()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x56, 0x01 }, PrintCommands.Rotate90On());
    }

    [Fact]
    public void Rotate90Off()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x56, 0x00 }, PrintCommands.Rotate90Off());
    }

    [Fact]
    public void AlignLeft()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x61, 0x00 }, PrintCommands.AlignLeft());
    }

    [Fact]
    public void AlignCenter()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x61, 0x01 }, PrintCommands.AlignCenter());
    }

    [Fact]
    public void AlignRight()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x61, 0x02 }, PrintCommands.AlignRight());
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0x02)]
    [InlineData((byte)0xFF)]
    public void Align(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x61, n }, PrintCommands.Align(n));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void FeedLines(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x64, n }, PrintCommands.FeedLines(n));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void FeedDots(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x4A, n }, PrintCommands.FeedDots(n));
    }

    [Fact]
    public void CutFull_Default()
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x56, 0x00 }, PrintCommands.CutFull());
    }

    [Fact]
    public void CutPartial_Default()
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x56, 0x01 }, PrintCommands.CutPartial());
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void CutFull_WithFeed(byte feed)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x56, 0x41, feed }, PrintCommands.CutFull(feed));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void CutPartial_WithFeed(byte feed)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x56, 0x42, feed }, PrintCommands.CutPartial(feed));
    }

    [Fact]
    public void PulseCashDrawer_DefaultTimes()
    {
        var actual = PrintCommands.PulseCashDrawer(drawer: 0x00);
        AssertBytesEqual(new byte[] { 0x1B, 0x70, 0x00, 120, 240 }, actual);
    }

    [Theory]
    [InlineData((byte)0x00, (byte)0x00, (byte)0x00)]
    [InlineData((byte)0x01, (byte)0x01, (byte)0x01)]
    [InlineData((byte)0xFF, (byte)0xFF, (byte)0xFF)]
    public void PulseCashDrawer_Custom(byte drawer, byte onTime, byte offTime)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x70, drawer, onTime, offTime }, PrintCommands.PulseCashDrawer(drawer, onTime, offTime));
    }

    [Theory]
    [InlineData((byte)0x00, (byte)0x00)]
    [InlineData((byte)0x01, (byte)0xFF)]
    public void PulseCashDrawer2_UsesDrawer0(byte onTime, byte offTime)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x70, 0x00, onTime, offTime }, PrintCommands.PulseCashDrawer2(onTime, offTime));
    }

    [Theory]
    [InlineData((byte)0x00, (byte)0x00)]
    [InlineData((byte)0x01, (byte)0xFF)]
    public void PulseCashDrawer5_UsesDrawer1(byte onTime, byte offTime)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x70, 0x01, onTime, offTime }, PrintCommands.PulseCashDrawer5(onTime, offTime));
    }

    [Theory]
    [InlineData((byte)0x00, (byte)0x00)]
    [InlineData((byte)0x01, (byte)0x02)]
    [InlineData((byte)0xFF, (byte)0xFF)]
    public void Beep(byte times, byte duration)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x42, times, duration }, PrintCommands.Beep(times, duration));
    }

    [Fact]
    public void SelectCodePage_Enum()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x74, (byte)CodePage.CP437_USA_StandardEurope }, PrintCommands.SelectCodePage(CodePage.CP437_USA_StandardEurope));
        AssertBytesEqual(new byte[] { 0x1B, 0x74, (byte)CodePage.UTF8 }, PrintCommands.SelectCodePage(CodePage.UTF8));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x10)]
    [InlineData((byte)0xFF)]
    public void SelectCodePage_Byte(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x74, n }, PrintCommands.SelectCodePage(n));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void SelectInternationalCharacterSet(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x52, n }, PrintCommands.SelectInternationalCharacterSet(n));
    }

    [Fact]
    public void PrintRasterImage_ZeroWidth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrintCommands.PrintRasterImage(Array.Empty<byte>(), 0, 1));
    }

    [Fact]
    public void PrintRasterImage_ZeroHeight_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrintCommands.PrintRasterImage(Array.Empty<byte>(), 1, 0));
    }

    [Fact]
    public void PrintRasterImage_LengthMismatch_Throws()
    {
        Assert.Throws<ArgumentException>(() => PrintCommands.PrintRasterImage(new byte[] { 0x00 }, 1, 2));
    }

    [Fact]
    public void PrintRasterImage_Valid_BuildsHeaderAndData()
    {
        var raster = new byte[] { 0xAA, 0x55 };
        var actual = PrintCommands.PrintRasterImage(raster, widthBytes: 1, heightPixels: 2, mode: 3);
        var expected = new byte[]
        {
            0x1D, 0x76, 0x30, 0x03,
            0x01, 0x00,
            0x02, 0x00,
            0xAA, 0x55,
        };
        AssertBytesEqual(expected, actual);
    }

    [Fact]
    public void PrintBmpImage_TooSmall_Throws()
    {
        Assert.Throws<ArgumentException>(() => PrintCommands.PrintBmpImage(new byte[10]));
    }

    [Fact]
    public void PrintBmpImage_NotBmp_Throws()
    {
        var bytes = new byte[54];
        bytes[0] = (byte)'N';
        bytes[1] = (byte)'O';
        Assert.Throws<ArgumentException>(() => PrintCommands.PrintBmpImage(bytes));
    }

    [Fact]
    public void PrintBmpImage_1x1_BlackPixel_ReturnsGsLStoreAndPrint()
    {
        // 1x1 24bpp BMP, bottom-up, 4-byte row padding.
        // Black pixel -> monochrome data 0x80.
        var bmp = new byte[58];
        bmp[0] = (byte)'B';
        bmp[1] = (byte)'M';

        void WriteInt32LE(int offset, int value)
        {
            bmp[offset + 0] = (byte)(value & 0xFF);
            bmp[offset + 1] = (byte)((value >> 8) & 0xFF);
            bmp[offset + 2] = (byte)((value >> 16) & 0xFF);
            bmp[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        void WriteInt16LE(int offset, short value)
        {
            bmp[offset + 0] = (byte)(value & 0xFF);
            bmp[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        WriteInt32LE(2, bmp.Length);      // file size
        WriteInt32LE(10, 54);            // pixel array offset
        WriteInt32LE(14, 40);            // DIB header size
        WriteInt32LE(18, 1);             // width (dots)
        WriteInt32LE(22, 1);             // height (dots)
        WriteInt16LE(26, 1);             // planes
        WriteInt16LE(28, 24);            // bpp
        WriteInt32LE(30, 0);             // compression = BI_RGB
        WriteInt32LE(34, 4);             // image size (rowStride=4)

        // Pixel BGR + padding: black pixel (0,0,0) + 1 padding byte
        bmp[54] = 0x00; // B
        bmp[55] = 0x00; // G
        bmp[56] = 0x00; // R
        bmp[57] = 0x00; // padding

        var actual = PrintCommands.PrintBmpImage(bmp); // default print fn=50

        var expected = new byte[]
        {
            // Store: GS ( L pL pH 30 70 a bx by c xL xH yL yH d...
            0x1D, 0x28, 0x4C, 0x0B, 0x00,
            0x30, 0x70,
            0x30, 0x01, 0x01, 0x31,
            0x01, 0x00,
            0x01, 0x00,
            0x80,
            // Print: GS ( L 02 00 30 50
            0x1D, 0x28, 0x4C, 0x02, 0x00,
            0x30, 0x32,
        };

        AssertBytesEqual(expected, actual);
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void SetBarcodeHeight(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x68, n }, PrintCommands.SetBarcodeHeight(n));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void SetBarcodeWidth(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x77, n }, PrintCommands.SetBarcodeWidth(n));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void SetHriPosition(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x48, n }, PrintCommands.SetHriPosition(n));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void SetHriFont(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x66, n }, PrintCommands.SetHriFont(n));
    }

    [Fact]
    public void PrintBarcode_MLessOrEqual6_AppendsNul()
    {
        var data = new byte[] { 0x31, 0x32, 0x33 };
        var actual = PrintCommands.PrintBarcode(6, data);
        AssertBytesEqual(new byte[] { 0x1D, 0x6B, 0x06, 0x31, 0x32, 0x33, 0x00 }, actual);
    }

    [Fact]
    public void PrintBarcode_MGreaterThan6_PrefixesLength()
    {
        var data = new byte[] { 0x41, 0x42 };
        var actual = PrintCommands.PrintBarcode(73, data);
        AssertBytesEqual(new byte[] { 0x1D, 0x6B, 73, 0x02, 0x41, 0x42 }, actual);
    }

    [Fact]
    public void PrintBarcode_Length256_Throws()
    {
        var data = Enumerable.Repeat((byte)0x41, 256).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => PrintCommands.PrintBarcode(73, data));
    }

    [Fact]
    public void QrCodeSelectModel2()
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00 }, PrintCommands.QrCodeSelectModel2());
    }

    [Theory]
    [InlineData((byte)1)]
    [InlineData((byte)16)]
    public void QrCodeSetModuleSize_Valid(byte size)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, size }, PrintCommands.QrCodeSetModuleSize(size));
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)17)]
    public void QrCodeSetModuleSize_Invalid_Throws(byte size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrintCommands.QrCodeSetModuleSize(size));
    }

    [Theory]
    [InlineData((byte)48)]
    [InlineData((byte)51)]
    public void QrCodeSetErrorCorrectionLevel_Valid(byte level)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, level }, PrintCommands.QrCodeSetErrorCorrectionLevel(level));
    }

    [Theory]
    [InlineData((byte)47)]
    [InlineData((byte)52)]
    public void QrCodeSetErrorCorrectionLevel_Invalid_Throws(byte level)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrintCommands.QrCodeSetErrorCorrectionLevel(level));
    }

    [Fact]
    public void QrCodeStoreData_Empty()
    {
        var actual = PrintCommands.QrCodeStoreData(ReadOnlySpan<byte>.Empty);
        AssertBytesEqual(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x50, 0x30 }, actual);
    }

    [Fact]
    public void QrCodeStoreData_SmallPayload()
    {
        var data = new byte[] { 0x41, 0x42, 0x43 };
        var actual = PrintCommands.QrCodeStoreData(data);
        // pL/pH = data.Length + 3 = 6 => 0x06 0x00
        AssertBytesEqual(new byte[] { 0x1D, 0x28, 0x6B, 0x06, 0x00, 0x31, 0x50, 0x30, 0x41, 0x42, 0x43 }, actual);
    }

    [Fact]
    public void QrCodePrint()
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 }, PrintCommands.QrCodePrint());
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void RealTimeStatusTransmission(byte n)
    {
        AssertBytesEqual(new byte[] { 0x10, 0x04, n }, PrintCommands.RealTimeStatusTransmission(n));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x01)]
    [InlineData((byte)0xFF)]
    public void RealTimeRequestToPrinter(byte n)
    {
        AssertBytesEqual(new byte[] { 0x10, 0x05, n }, PrintCommands.RealTimeRequestToPrinter(n));
    }

    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x20)]
    [InlineData((byte)0xFF)]
    public void SetPrintMode(byte n)
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x21, n }, PrintCommands.SetPrintMode(n));
    }

    [Fact]
    public void ResetPrintMode()
    {
        AssertBytesEqual(new byte[] { 0x1B, 0x21, 0x00 }, PrintCommands.ResetPrintMode());
    }

    [Theory]
    [InlineData(true, (byte)0x01)]
    [InlineData(false, (byte)0x00)]
    public void EnableSmoothing(bool on, byte n)
    {
        AssertBytesEqual(new byte[] { 0x1D, 0x62, n }, PrintCommands.EnableSmoothing(on));
    }
}
