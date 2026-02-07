using System.Text;
using EscPos.Commands;
using EscPos.Console;
using EscPos.Printers;

namespace EscPos.UnitTests;

/// <summary>
/// Demonstrates the TextPrinter showing barcode and QR code data
/// </summary>
public sealed class TextPrinterBarcodeDemo
{
    [Fact]
    public async Task DemonstrateBarcodesAndQRCodesWithData()
    {
        var printer = new TextPrinter(lineWidth: 48);
        var buffer = new PrintBuffer();
        
        // Header
        buffer.Write(PrintCommands.Initialize());
        buffer.Write(PrintCommands.AlignCenter());
        buffer.WriteLine("BARCODE DEMO");
        buffer.WriteLine("================================");
        buffer.Write(PrintCommands.AlignLeft());
        buffer.WriteLine();
        
        // Regular barcode with data
        buffer.WriteLine("Product Barcode:");
        buffer.Write(PrintCommands.SetBarcodeHeight(50));
        buffer.Write(PrintCommands.PrintBarcode(73, Encoding.ASCII.GetBytes("123456789012"))); // CODE128
        buffer.WriteLine();
        
        // QR Code with URL
        buffer.WriteLine("Order QR Code:");
        buffer.Write(PrintCommands.QrCodeSelectModel2());
        buffer.Write(PrintCommands.QrCodeSetModuleSize(4));
        buffer.Write(PrintCommands.QrCodeSetErrorCorrectionLevel(49)); // M level
        buffer.Write(PrintCommands.QrCodeStoreData(Encoding.UTF8.GetBytes("https://example.com/order/12345")));
        buffer.Write(PrintCommands.QrCodePrint());
        buffer.WriteLine();
        
        // Another barcode
        buffer.WriteLine("Item Code:");
        buffer.Write(PrintCommands.PrintBarcode(69, Encoding.ASCII.GetBytes("ITEM-ABC-123"))); // CODE39
        buffer.WriteLine();
        
        buffer.Write(PrintCommands.AlignCenter());
        buffer.WriteLine("================================");
        buffer.Write(PrintCommands.CutPartial());
        
        await printer.SendAsync(buffer.ToArray());
        
        var output = printer.GetOutput();
        
        System.Console.WriteLine("=== TextPrinter Output ===");
        System.Console.WriteLine(output);
        System.Console.WriteLine("=========================");
        
        // Verify all barcode data is present
        Assert.Contains("[CODE128]", output);
        Assert.Contains("123456789012", output);
        Assert.Contains("[QRCode]", output);
        Assert.Contains("https://example.com/order/12345", output);
        Assert.Contains("[CODE39]", output);
        Assert.Contains("ITEM-ABC-123", output);
    }
}
