# EscPos - ESC/POS Printer Library for .NET

A modern, lightweight .NET library for controlling ESC/POS thermal printers. Supports network (IP), serial, and file-based printer connections with comprehensive command support for receipts, barcodes, QR codes, and image printing.

## Features

- **Multiple Connection Types**: Network (TCP/IP), Serial Port, and File-based printers
- **Rich Command Set**: Text formatting, alignment, fonts, character sizing, and more
- **XML Template System**: Declarative receipt generation with variables and loops
- **Image Support**: Print BMP images (1-bit, 4-bit, 8-bit, 16-bit, 24-bit, 32-bit) with RLE compression support
- **Barcodes & QR Codes**: Generate and print various barcode formats and QR codes
- **Printer Status**: Query paper status, drawer status, and printer state
- **Async/Await**: Modern async API for non-blocking operations
- **Type-Safe**: Strongly-typed commands with validation
- **No External Dependencies**: Pure .NET implementation without image processing libraries

## Installation

```bash
# Add reference to your project
dotnet add reference path/to/EscPos.Printers.csproj
dotnet add reference path/to/EscPos.Commands.csproj
```

## Quick Start

### Basic Receipt Printing (Network Printer)

```csharp
using EscPos.Commands;
using EscPos.Printers;
using System.Text;

// Connect to network printer (IP address and port)
using IPrinter printer = new IPPrinter("192.168.1.100", 9100);
await printer.ConnectAsync();

// Build print buffer
using var buffer = new PrintBuffer();
buffer
    .Write(PrintCommands.Initialize())
    .Write(PrintCommands.AlignCenter())
    .Write(PrintCommands.BoldOn())
    .Write(PrintCommands.PrintLine("MY SHOP"))
    .Write(PrintCommands.BoldOff())
    .Write(PrintCommands.AlignLeft())
    .Write(PrintCommands.PrintLine("Item A        $10.00"))
    .Write(PrintCommands.PrintLine("Item B        $ 5.00"))
    .Write(PrintCommands.PrintLine("----------------------------"))
    .Write(PrintCommands.PrintLine("Total         $15.00"))
    .Write(PrintCommands.LineFeed(2))
    .Write(PrintCommands.AlignCenter())
    .Write(PrintCommands.PrintLine("Thank you!"))
    .Write(PrintCommands.FeedLines(3))
    .Write(PrintCommands.CutFull());

// Send to printer
await printer.SendAsync(buffer.ToArray());
```

### Serial Printer

```csharp
using IPrinter printer = new SerialPrinter("COM3", 9600);
await printer.ConnectAsync();
// ... use same PrintBuffer API
```

### File Printer (for testing)

```csharp
using IPrinter printer = new FilePrinter("output.prn");
await printer.ConnectAsync();
// ... use same PrintBuffer API
```

## Advanced Features

### XML Template System (Recommended)

For complex receipts with dynamic content, use the XML Template System:

```csharp
var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <bold>
      <size width=""2"" height=""2"">
        <line>${Store.Name}</line>
      </size>
    </bold>
    <line>${Store.Address}</line>
  </align>
  
  <line>================================</line>
  <line>Order #${Order.Number}</line>
  
  <for var=""item"" in=""Order.Items"">
    <text>${item.Quantity}x ${item.Name}</text>
    <align value=""right"">
      <line>$${item.Price}</line>
    </align>
  </for>
  
  <line>--------------------------------</line>
  <align value=""right"">
    <bold><line>Total: $${Order.Total}</line></bold>
  </align>
  
  <qrcode data=""${Order.Url}"" size=""4"" errorLevel=""M""/>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

var orderData = new
{
    Store = new { Name = "MY SHOP", Address = "123 Main St" },
    Order = new
    {
        Number = "1234",
        Items = new[]
        {
            new { Quantity = 1, Name = "Burger", Price = "12.99" },
            new { Quantity = 2, Name = "Fries", Price = "6.98" }
        },
        Total = "19.97",
        Url = "https://shop.com/order/1234"
    }
};

byte[] commands = ReceiptXmlParser.Parse(xml, orderData);
await printer.SendAsync(commands);
```

**Features:**
- `${variable}` syntax for template substitution
- `<if>` conditional rendering
- `<for>` loops for iterating collections
- Nested properties support (`${Order.Items}`)
- Schema validation
- All ESC/POS commands available as XML elements

**?? See [Documentation](#documentation) section below for complete guides.**

### QR Code Printing

```csharp
buffer
    .Write(PrintCommands.AlignCenter())
    .Write(PrintCommands.QrCodeSelectModel2())
    .Write(PrintCommands.QrCodeSetModuleSize(6))
    .Write(PrintCommands.QrCodeSetErrorCorrectionLevel(49)) // M level
    .Write(PrintCommands.QrCodeStoreData(Encoding.ASCII.GetBytes("https://example.com")))
    .Write(PrintCommands.QrCodePrint())
    .Write(PrintCommands.LineFeed(2));
```

### Image Printing (BMP)

```csharp
byte[] bmpData = File.ReadAllBytes("logo.bmp");
buffer
    .Write(PrintCommands.AlignCenter())
    .Write(PrintCommands.PrintBmpImage(bmpData))
    .Write(PrintCommands.LineFeed(2));
```

Supported BMP formats:
- **Bit Depths**: 1-bit, 4-bit, 8-bit, 16-bit, 24-bit, 32-bit
- **Compression**: Uncompressed (BI_RGB), RLE8, RLE4
- Automatic conversion to 1-bit monochrome for thermal printing

### Barcode Printing

```csharp
buffer
    .Write(PrintCommands.SetBarcodeHeight(80))
    .Write(PrintCommands.SetBarcodeWidth(3))
    .Write(PrintCommands.SetHriPosition(2)) // Print human-readable below
    .Write(PrintCommands.PrintBarcode(73, Encoding.ASCII.GetBytes("123456789012")));
```

### Printer Status Check

```csharp
var status = new StatusHelper(printer);

var paperStatuses = await status.GetPaperStatusAsync();
Console.WriteLine($"Paper status: {string.Join(", ", paperStatuses)}");

var drawerStatuses = await status.GetDrawerStatusAsync();
Console.WriteLine($"Drawer status: {string.Join(", ", drawerStatuses)}");

var printerStates = await status.GetPrinterStateAsync();
Console.WriteLine($"Printer state: {string.Join(", ", printerStates)}");
```

### Text Formatting

```csharp
buffer
    .Write(PrintCommands.BoldOn())
    .Write(PrintCommands.PrintLine("Bold Text"))
    .Write(PrintCommands.BoldOff())
    .Write(PrintCommands.Underline1Dot())
    .Write(PrintCommands.PrintLine("Underlined Text"))
    .Write(PrintCommands.UnderlineOff())
    .Write(PrintCommands.SetCharacterSize(2, 2)) // 2x width, 2x height
    .Write(PrintCommands.PrintLine("Large Text"))
    .Write(PrintCommands.SetCharacterSizeNormal())
    .Write(PrintCommands.InvertOn())
    .Write(PrintCommands.PrintLine("Inverted Text"))
    .Write(PrintCommands.InvertOff());
```

### Testing and Debugging with TextPrinter

The `TextPrinter` class allows you to preview receipt output as human-readable text without connecting to an actual printer. This is useful for testing, debugging, and development.

```csharp
using EscPos.Printers;

// Create a TextPrinter with desired line width (default is 48 characters)
using var textPrinter = new TextPrinter(lineWidth: 48);

// Parse your receipt XML or build commands
var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  <align value=""center"">
    <bold><line>MY RESTAURANT</line></bold>
    <line>123 Main Street</line>
  </align>
  <line>================================</line>
  <line>1x Burger.................$12.99</line>
  <line>2x Fries..................$6.98</line>
  <line>--------------------------------</line>
  <align value=""right"">
    <bold><line>Total:      $19.97</line></bold>
  </align>
  <qrcode data=""https://shop.com/order/1234"" size=""4""/>
  <feed lines=""2""/>
  <cut type=""partial""/>
</receipt>";

var escposData = ReceiptXmlParser.Parse(xml);

// Send to TextPrinter
await textPrinter.SendAsync(escposData);

// Get the text representation
string textOutput = textPrinter.GetOutput();
Console.WriteLine(textOutput);
```

**Output:**
```
        MY RESTAURANT
        123 Main Street
================================
1x Burger.................$12.99
2x Fries..................$6.98
--------------------------------
             Total:      $19.97
[QRCode]
https://shop.com/order/1234


---
```

**Features:**
- **Text-only commands**: Bold, underline, alignment, character sizing are interpreted
- **Images**: Shown as `[IMAGE]`
- **Barcodes**: Shown as `[QRCode]`, `[CODE128]`, etc., with barcode data on the next line
- **Paper cuts**: Full cut = `------`, Partial cut = `---`
- **Alignment**: Left, center, and right alignment with configurable line width
- **No printer required**: Perfect for unit tests and CI/CD pipelines

This makes it easy to validate receipt formatting and content without physical hardware.

## Project Structure

- **EscPos.Commands**: Core ESC/POS command generation and XML template system
  - `PrintCommands`: Low-level ESC/POS command generation
  - `ReceiptXmlParser`: XML-based declarative receipt generation
  - `ReceiptTemplateContext`: Template variable substitution engine
- **EscPos.Printers**: Printer connection implementations (Network, Serial, File)
- **EscPos.Console**: Example console application
- **EscPos.UnitTests**: Comprehensive unit tests

## Documentation

- **[XML Template System](ReceiptXmlParser.md)** - Complete guide to declarative receipt generation
  - [Template Variables](ReceiptTemplateVariables.md) - `${variable}` syntax and usage
  - [Conditional Rendering](ReceiptConditionals.md) - `<if>` conditions for dynamic content
  - [For Loops](ReceiptLoops.md) - Iterating over collections in templates
- **API Reference** - See XML documentation in source code

## Requirements

- **.NET 10.0** or later
- For serial printers: Windows, Linux, or macOS with serial port support

## License

MIT License

Copyright (c) 2025

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Supported Commands

### Initialization & Control
- `Initialize()`, `RequestStatus()`

### Text & Formatting
- `Text()`, `PrintLine()`, `LineFeed()`, `FeedLines()`, `FeedDots()`
- `Bold()`, `Underline()`, `DoubleStrike()`, `Invert()`
- `SetCharacterSize()`, `SelectFont()`
- `AlignLeft()`, `AlignCenter()`, `AlignRight()`

### Positioning
- `HorizontalTab()`, `CarriageReturn()`, `FormFeed()`
- `SetAbsolutePrintPosition()`, `SetPrintPositionRelative()`
- `SetLeftMargin()`, `SetLineSpacing()`, `SetRightSideCharacterSpacing()`

### Images & Graphics
- `PrintBmpImage()` - Supports 1/4/8/16/24/32-bit BMPs with RLE compression
- `PrintRasterImage()` - Print raw 1-bit raster data

### Barcodes & QR Codes
- `PrintBarcode()`, `SetBarcodeHeight()`, `SetBarcodeWidth()`, `SetHriPosition()`
- `QrCodeSelectModel2()`, `QrCodeSetModuleSize()`, `QrCodeSetErrorCorrectionLevel()`
- `QrCodeStoreData()`, `QrCodePrint()`

### Paper & Cutting
- `CutFull()`, `CutPartial()`

### Cash Drawer
- `PulseCashDrawer2()`, `PulseCashDrawer5()`

### Internationalization
- `SelectCodePage()`, `SelectInternationalCharacterSet()`

## Tested Printers

This library follows the ESC/POS standard and should work with most thermal receipt printers including:
- Epson TM series
- Star Micronics
- Citizen
- Bixolon
- And other ESC/POS compatible printers

## Examples

See the `EscPos.Console` project for a complete working example demonstrating:
- Text formatting and alignment
- BMP image printing
- Raster image generation and printing
- QR code generation
- Status checking
