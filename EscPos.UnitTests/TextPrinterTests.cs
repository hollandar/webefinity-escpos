using System.Text;
using EscPos.Commands;
using EscPos.Console;
using EscPos.Printers;

namespace EscPos.UnitTests;

public sealed class TextPrinterTests
{
    [Fact]
    public async Task TextPrinter_SimpleText_OutputsCorrectly()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        buffer.Write(PrintCommands.Initialize());
        buffer.Write(PrintCommands.AlignCenter());
        buffer.WriteLine("RECEIPT");
        buffer.Write(PrintCommands.AlignLeft());
        buffer.WriteLine("Item 1: $10.00");
        buffer.WriteLine("Item 2: $20.00");
        buffer.Write(PrintCommands.CutFull());
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        Assert.Contains("RECEIPT", output);
        Assert.Contains("Item 1: $10.00", output);
        Assert.Contains("Item 2: $20.00", output);
        Assert.Contains("-< full cut >-", output);
    }
    
    [Fact]
    public async Task TextPrinter_PartialCut_OutputsDashes()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        buffer.Write(PrintCommands.CutPartial());
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        Assert.Contains("-< cut >-", output);
        Assert.DoesNotContain("-< full cut >-", output);
    }
    
    [Fact]
    public async Task TextPrinter_BoldAndUnderline_IgnoredInTextOutput()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        buffer.Write(PrintCommands.BoldOn());
        buffer.Write("Bold Text");
        buffer.Write(PrintCommands.BoldOff());
        buffer.Write(PrintCommands.LineFeed());
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        Assert.Contains("Bold Text", output);
    }
    
    [Fact]
    public async Task TextPrinter_QRCode_OutputsQRCodeMarker()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        buffer.Write(PrintCommands.QrCodeSelectModel2());
        buffer.Write(PrintCommands.QrCodeSetModuleSize(5));
        buffer.Write(PrintCommands.QrCodeSetErrorCorrectionLevel(48));
        buffer.Write(PrintCommands.QrCodeStoreData(Encoding.UTF8.GetBytes("https://example.com")));
        buffer.Write(PrintCommands.QrCodePrint());
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        Assert.Contains("[QRCode]", output);
        Assert.Contains("https://example.com", output);
    }
    
    [Fact]
    public async Task TextPrinter_RasterImage_OutputsImageMarker()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        // Create a simple 8x8 black image (1 bit per pixel)
        var imageData = new byte[8]; // 8 rows, 1 byte per row
        for (var i = 0; i < 8; i++)
        {
            imageData[i] = 0xFF; // All black
        }
        
        buffer.Write(PrintCommands.PrintRasterImage(imageData, 1, 8));
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        Assert.Contains("[IMAGE]", output);
    }
    
    [Fact]
    public async Task TextPrinter_Barcode_OutputsBarcodeTypeAndData()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        // Print a CODE39 barcode with data "12345"
        buffer.Write(PrintCommands.SetBarcodeHeight(50));
        buffer.Write(PrintCommands.PrintBarcode(73, Encoding.ASCII.GetBytes("12345"))); // CODE128
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        Assert.Contains("[CODE128]", output);
        Assert.Contains("12345", output);
    }
    
    [Fact]
    public async Task TextPrinter_CharacterSize_RepeatsCharacters()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        buffer.Write(PrintCommands.SetCharacterSize(2, 2));
        buffer.Write("X");
        buffer.Write(PrintCommands.SetCharacterSizeNormal());
        buffer.Write(PrintCommands.LineFeed());
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        // With 2x width, "X" should appear as "XX"
        Assert.Contains("XX", output);
    }
    
    [Fact]
    public async Task TextPrinter_CenterAlignment_AddsPadding()
    {
        var printer = new TextPrinter(lineWidth: 40);
        var buffer = new PrintBuffer();
        
        buffer.Write(PrintCommands.AlignCenter());
        buffer.WriteLine("CENTER");
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        var lines = output.Split('\n');
        var centerLine = lines.FirstOrDefault(l => l.Contains("CENTER"));
        
        Assert.NotNull(centerLine);
        // The line should have leading spaces
        Assert.True(centerLine.StartsWith(" "), "Centered text should have leading spaces");
    }
    
    [Fact]
    public async Task TextPrinter_RightAlignment_AddsPadding()
    {
        var printer = new TextPrinter(lineWidth: 40);
        var buffer = new PrintBuffer();
        
        buffer.Write(PrintCommands.AlignRight());
        buffer.WriteLine("RIGHT");
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        var lines = output.Split('\n');
        var rightLine = lines.FirstOrDefault(l => l.Contains("RIGHT"));
        
        Assert.NotNull(rightLine);
        // The line should have leading spaces
        Assert.True(rightLine.StartsWith(" "), "Right-aligned text should have leading spaces");
    }
    
    [Fact]
    public async Task TextPrinter_FeedLines_OutputsBlankLines()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        buffer.WriteLine("Line 1");
        buffer.Write(PrintCommands.FeedLines(3));
        buffer.WriteLine("Line 2");
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        var lines = output.Split('\n');
        
        // Should have Line 1, 3 blank lines, then Line 2
        Assert.Contains("Line 1", output);
        Assert.Contains("Line 2", output);
        
        // Count lines between Line 1 and Line 2
        var line1Index = Array.FindIndex(lines, l => l.Contains("Line 1"));
        var line2Index = Array.FindIndex(lines, l => l.Contains("Line 2"));
        
        Assert.True(line2Index - line1Index > 3, "Should have blank lines between Line 1 and Line 2");
    }
    
    [Fact]
    public async Task TextPrinter_BmpImage_OutputsImageMarker()
    {
        var printer = new TextPrinter();
        var buffer = new PrintBuffer();
        
        // Create a minimal valid 1bpp BMP (2x2 pixels, black and white pattern)
        var bmpData = CreateMinimal1bppBmp(2, 2);
        
        buffer.Write(PrintCommands.PrintBmpImage(bmpData));
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        Assert.Contains("[IMAGE]", output);
    }
    
    private static byte[] CreateMinimal1bppBmp(int width, int height)
    {
        // BMP file header (14 bytes)
        var fileHeaderSize = 14;
        var dibHeaderSize = 40;
        var paletteSize = 8; // 2 colors * 4 bytes
        var rowStride = ((width + 31) / 32) * 4; // Aligned to 4 bytes
        var pixelDataSize = rowStride * height;
        var fileSize = fileHeaderSize + dibHeaderSize + paletteSize + pixelDataSize;
        var pixelDataOffset = fileHeaderSize + dibHeaderSize + paletteSize;
        
        var bmp = new byte[fileSize];
        
        // File header
        bmp[0] = (byte)'B';
        bmp[1] = (byte)'M';
        WriteInt32LE(bmp, 2, fileSize);
        WriteInt32LE(bmp, 10, pixelDataOffset);
        
        // DIB header (BITMAPINFOHEADER)
        WriteInt32LE(bmp, 14, dibHeaderSize);
        WriteInt32LE(bmp, 18, width);
        WriteInt32LE(bmp, 22, height);
        WriteInt16LE(bmp, 26, 1); // Planes
        WriteInt16LE(bmp, 28, 1); // Bits per pixel
        WriteInt32LE(bmp, 30, 0); // Compression (BI_RGB)
        
        // Palette (black and white)
        // Color 0: Black (B=0, G=0, R=0, Reserved=0)
        bmp[54] = 0; bmp[55] = 0; bmp[56] = 0; bmp[57] = 0;
        // Color 1: White (B=255, G=255, R=255, Reserved=0)
        bmp[58] = 255; bmp[59] = 255; bmp[60] = 255; bmp[61] = 0;
        
        // Pixel data (all zeros = all black)
        // Already zero-initialized
        
        return bmp;
    }
    
    private static void WriteInt32LE(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
    
    private static void WriteInt16LE(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
    }
    
    [Fact]
    public async Task TextPrinter_WithReceiptXml_ProducesReadableOutput()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <bold>
      <line>MY RESTAURANT</line>
    </bold>
    <line>123 Main Street</line>
    <line>City, State 12345</line>
  </align>
  
  <line>================================</line>
  
  <align value=""left"">
    <bold><text>Order #1234</text></bold>
    <line> - Table 5</line>
  </align>
  
  <line>================================</line>
  
  <line>1x Burger.................$12.99</line>
  <line>2x Fries..................$6.98</line>
  
  <line>--------------------------------</line>
  
  <align value=""right"">
    <line>Subtotal:      $22.47</line>
    <line>Tax (10%):      $2.25</line>
    <bold>
      <line>Total:      $24.72</line>
    </bold>
  </align>
  
  <line>================================</line>
  
  <align value=""center"">
    <line/>
    <line>Scan for receipt</line>
  </align>
  
  <qrcode data=""https://restaurant.com/order/1234"" size=""4"" errorLevel=""M""/>
  
  <feed lines=""2""/>
  <cut type=""partial""/>
</receipt>";
        
        var escposData = ReceiptXmlParser.Parse(xml, validate: true);
        
        var printer = new TextPrinter(lineWidth: 48);
        await printer.SendAsync(escposData);
        
        var output = printer.GetOutput();
        
        // Verify key content is present
        Assert.Contains("MY RESTAURANT", output);
        Assert.Contains("123 Main Street", output);
        Assert.Contains("Order #1234", output);
        Assert.Contains("1x Burger", output);
        Assert.Contains("$12.99", output);
        Assert.Contains("Subtotal:", output);
        Assert.Contains("Total:", output);
        Assert.Contains("[QRCode]", output);
        Assert.Contains("https://restaurant.com/order/1234", output); // QR code data
        Assert.Contains("Scan for receipt", output);
        Assert.Contains("---", output); // Partial cut
    }
}
