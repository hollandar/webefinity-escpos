// See https://aka.ms/new-console-template for more information
using EscPos.Console;
using EscPos.Commands;
using System.Text;
using EscPos.Printers;

await PrintReceipt();

static async Task PrintReceipt()
{
    using IPrinter printer = new IPPrinter("192.168.1.128", 9100);

    await printer.ConnectAsync();

    // Embedded 64x64 24bpp BMP (uncompressed BI_RGB). Content: simple black circle on white.
    // Note: This is generated at runtime and stored as a byte[] to simulate embedded BMP payload.
    byte[] bmpImage = CreateCircleBmp24(64, 64);

    // 128x128 1bpp raster (widthBytes=16, height=128). Simple circle outline.
    const int imageWidthPixels = 128;
    const int imageHeightPixels = 128;
    const ushort imageWidthBytes = imageWidthPixels / 8;
    const ushort imageHeight = imageHeightPixels;

    byte[] rasterImage = CreateCircleRaster1Bpp(imageWidthPixels, imageHeightPixels);

    using var buffer = new PrintBuffer();

    buffer
        .Write(PrintCommands.Initialize())
        .Write(PrintCommands.AlignCenter())
        .Write(PrintCommands.BoldOn())
        .Write(PrintCommands.PrintLine("EXAMPLE SHOP"))
        .Write(PrintCommands.BoldOff())
        .Write(PrintCommands.AlignLeft())
        .Write(PrintCommands.PrintLine("Item A        $10.00"))
        .Write(PrintCommands.PrintLine("Item B        $ 5.00"))
        .Write(PrintCommands.LineFeed())
        .Write(PrintCommands.AlignCenter())
        .Write(PrintCommands.PrintLine("Thank you!"))

        // BMP image (NV graphics define+print)
        .Write(PrintCommands.LineFeed())
        .Write(PrintCommands.AlignCenter())
        .Write(PrintCommands.PrintBmpImage(bmpImage))
        .Write(PrintCommands.LineFeed(2))

        // Raster image
        .Write(PrintCommands.AlignCenter())
        .Write(PrintCommands.PrintRasterImage(rasterImage, imageWidthBytes, imageHeight))
        .Write(PrintCommands.LineFeed(2))

        // QR code
        .Write(PrintCommands.AlignCenter())
        .Write(PrintCommands.QrCodeSelectModel2())
        .Write(PrintCommands.QrCodeSetModuleSize(6))
        .Write(PrintCommands.QrCodeSetErrorCorrectionLevel(49)) // M
        .Write(PrintCommands.QrCodeStoreData(Encoding.ASCII.GetBytes("https://microsoft.com")))
        .Write(PrintCommands.QrCodePrint())

        .Write(PrintCommands.FeedLines(3))
        .Write(PrintCommands.CutFull());

    await printer.SendAsync(buffer.ToArray());

    var status = new StatusHelper(printer);

    var paperStatuses = await status.GetPaperStatusAsync();
    Console.WriteLine($"Paper status: {string.Join(", ", paperStatuses)}");

    var drawerStatuses = await status.GetDrawerStatusAsync();
    Console.WriteLine($"Drawer status: {string.Join(", ", drawerStatuses)}");

    var printerStates = await status.GetPrinterStateAsync();
    Console.WriteLine($"Printer state: {string.Join(", ", printerStates)}");
}

static byte[] CreateCircleRaster1Bpp(int widthPixels, int heightPixels)
{
    if (widthPixels <= 0) throw new ArgumentOutOfRangeException(nameof(widthPixels));
    if ((widthPixels % 8) != 0) throw new ArgumentOutOfRangeException(nameof(widthPixels), "Width must be a multiple of 8 for 1bpp byte-packed raster.");
    if (heightPixels <= 0) throw new ArgumentOutOfRangeException(nameof(heightPixels));

    var widthBytes = widthPixels / 8;
    var data = new byte[widthBytes * heightPixels];

    var cx = (widthPixels - 1) / 2.0;
    var cy = (heightPixels - 1) / 2.0;
    var radius = Math.Min(widthPixels, heightPixels) * 0.35;
    var thickness = 1.5;

    for (var y = 0; y < heightPixels; y++)
    {
        var dy = y - cy;
        for (var x = 0; x < widthPixels; x++)
        {
            var dx = x - cx;
            var dist = Math.Sqrt((dx * dx) + (dy * dy));

            var on = Math.Abs(dist - radius) <= thickness;
            if (!on) continue;

            var byteIndex = (y * widthBytes) + (x / 8);
            var bit = 7 - (x % 8); // MSB first
            data[byteIndex] |= (byte)(1 << bit);
        }
    }

    return data;
}

static byte[] CreateCircleBmp24(int width, int height)
{
    if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
    if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

    // 24bpp rows padded to 4-byte boundary
    var rowStride = ((width * 3) + 3) & ~3;
    var pixelDataSize = rowStride * height;

    const int fileHeaderSize = 14;
    const int dibHeaderSize = 40;
    var pixelOffset = fileHeaderSize + dibHeaderSize;
    var fileSize = pixelOffset + pixelDataSize;

    var bmp = new byte[fileSize];

    void WriteInt16LE(int offset, int value)
    {
        bmp[offset + 0] = (byte)(value & 0xFF);
        bmp[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    void WriteInt32LE(int offset, int value)
    {
        bmp[offset + 0] = (byte)(value & 0xFF);
        bmp[offset + 1] = (byte)((value >> 8) & 0xFF);
        bmp[offset + 2] = (byte)((value >> 16) & 0xFF);
        bmp[offset + 3] = (byte)((value >> 24) & 0xFF);
    }

    // BITMAPFILEHEADER
    bmp[0] = (byte)'B';
    bmp[1] = (byte)'M';
    WriteInt32LE(2, fileSize);
    WriteInt16LE(6, 0);
    WriteInt16LE(8, 0);
    WriteInt32LE(10, pixelOffset);

    // BITMAPINFOHEADER
    WriteInt32LE(14, dibHeaderSize);
    WriteInt32LE(18, width);
    WriteInt32LE(22, height); // bottom-up
    WriteInt16LE(26, 1); // planes
    WriteInt16LE(28, 24); // bpp
    WriteInt32LE(30, 0); // BI_RGB
    WriteInt32LE(34, pixelDataSize);
    WriteInt32LE(38, 0);
    WriteInt32LE(42, 0);
    WriteInt32LE(46, 0);
    WriteInt32LE(50, 0);

    // Fill white background
    for (var i = pixelOffset; i < bmp.Length; i++)
        bmp[i] = 0xFF;

    // Draw a black circle outline
    var cx = (width - 1) / 2.0;
    var cy = (height - 1) / 2.0;
    var radius = Math.Min(width, height) * 0.35;
    var thickness = 1.5;

    for (var y = 0; y < height; y++)
    {
        for (var x = 0; x < width; x++)
        {
            var dx = x - cx;
            var dy = y - cy;
            var dist = Math.Sqrt((dx * dx) + (dy * dy));
            if (Math.Abs(dist - radius) > thickness)
                continue;

            // bottom-up rows
            var row = (height - 1 - y);
            var px = pixelOffset + (row * rowStride) + (x * 3);
            bmp[px + 0] = 0x7f; // B
            bmp[px + 1] = 0x7f; // G
            bmp[px + 2] = 0x7f; // R
        }
    }

    return bmp;
}
