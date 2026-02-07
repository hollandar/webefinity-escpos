using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EscPos.Printers;

public sealed class TextPrinter : IPrinter
{
    private readonly StringBuilder _output = new();
    private readonly StringBuilder _currentLine = new();
    private bool _bold;
    private bool _underline;
    private bool _invert;
    private int _alignment; // 0=left, 1=center, 2=right
    private int _charWidth = 1;
    private int _charHeight = 1;
    private readonly int _lineWidth;

    public TextPrinter(int lineWidth = 48)
    {
        if (lineWidth <= 0) throw new ArgumentOutOfRangeException(nameof(lineWidth));
        _lineWidth = lineWidth;
    }

    public string GetOutput() => _output.ToString();

    public Task ConnectAsync() => Task.CompletedTask;

    public Task SendAsync(byte[] data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        
        var i = 0;
        while (i < data.Length)
        {
            var b = data[i];

            // Check for ESC sequences
            if (b == 0x1B && i + 1 < data.Length)
            {
                var cmd = data[i + 1];
                
                // ESC @ - Initialize
                if (cmd == 0x40)
                {
                    ResetState();
                    i += 2;
                    continue;
                }
                
                // ESC E n - Bold
                if (cmd == 0x45 && i + 2 < data.Length)
                {
                    _bold = data[i + 2] != 0;
                    i += 3;
                    continue;
                }
                
                // ESC - n - Underline
                if (cmd == 0x2D && i + 2 < data.Length)
                {
                    _underline = data[i + 2] != 0;
                    i += 3;
                    continue;
                }
                
                // ESC a n - Alignment
                if (cmd == 0x61 && i + 2 < data.Length)
                {
                    _alignment = data[i + 2];
                    i += 3;
                    continue;
                }
                
                // ESC d n - Feed lines
                if (cmd == 0x64 && i + 2 < data.Length)
                {
                    FlushCurrentLine();
                    var feedCount = data[i + 2];
                    for (var j = 0; j < feedCount; j++)
                    {
                        _output.AppendLine();
                    }
                    i += 3;
                    continue;
                }
                
                // ESC M n - Select font (ignore for text output)
                if (cmd == 0x4D && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC V n - Rotate (ignore for text output)
                if (cmd == 0x56 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC ! n - Print mode (ignore for text output)
                if (cmd == 0x21 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC R n - International character set (ignore)
                if (cmd == 0x52 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC t n - Select code page (ignore)
                if (cmd == 0x74 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC SP n - Character spacing
                if (cmd == 0x20 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC 2 - Default line spacing
                if (cmd == 0x32)
                {
                    i += 2;
                    continue;
                }
                
                // ESC 3 n - Set line spacing
                if (cmd == 0x33 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC G n - Double strike
                if (cmd == 0x47 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC J n - Feed dots
                if (cmd == 0x4A && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // ESC \ nL nH - Relative position
                if (cmd == 0x5C && i + 3 < data.Length)
                {
                    i += 4;
                    continue;
                }
                
                // ESC $ nL nH - Absolute position
                if (cmd == 0x24 && i + 3 < data.Length)
                {
                    i += 4;
                    continue;
                }
                
                // ESC p m t1 t2 - Cash drawer
                if (cmd == 0x70 && i + 4 < data.Length)
                {
                    i += 5;
                    continue;
                }
                
                // ESC B n t - Beep
                if (cmd == 0x42 && i + 3 < data.Length)
                {
                    i += 4;
                    continue;
                }
            }
            
            // Check for GS sequences
            if (b == 0x1D && i + 1 < data.Length)
            {
                var cmd = data[i + 1];
                
                // GS ! n - Character size
                if (cmd == 0x21 && i + 2 < data.Length)
                {
                    var size = data[i + 2];
                    _charWidth = (size & 0x0F) + 1;
                    _charHeight = ((size >> 4) & 0x0F) + 1;
                    i += 3;
                    continue;
                }
                
                // GS B n - Invert
                if (cmd == 0x42 && i + 2 < data.Length)
                {
                    _invert = data[i + 2] != 0;
                    i += 3;
                    continue;
                }
                
                // GS V m [n] - Cut
                if (cmd == 0x56 && i + 2 < data.Length)
                {
                    FlushCurrentLine();
                    var mode = data[i + 2];
                    if (mode == 0x00) // Full cut
                    {
                        _output.AppendLine("------");
                    }
                    else if (mode == 0x01) // Partial cut
                    {
                        _output.AppendLine("---");
                    }
                    else if (mode == 0x41 || mode == 0x42) // With feed
                    {
                        if (i + 3 < data.Length)
                        {
                            if (mode == 0x41)
                                _output.AppendLine("------");
                            else
                                _output.AppendLine("---");
                            i += 4;
                            continue;
                        }
                    }
                    i += 3;
                    continue;
                }
                
                // GS k m [n] d... - Barcode
                if (cmd == 0x6B && i + 2 < data.Length)
                {
                    FlushCurrentLine();
                    var barcodeType = data[i + 2];
                    var barcodeData = new List<byte>();
                    
                    if (barcodeType <= 6) // NUL-terminated
                    {
                        var j = i + 3;
                        while (j < data.Length && data[j] != 0x00)
                        {
                            barcodeData.Add(data[j]);
                            j++;
                        }
                        i = j + 1;
                    }
                    else // Length-prefixed
                    {
                        if (i + 3 < data.Length)
                        {
                            var len = data[i + 3];
                            for (var j = 0; j < len && i + 4 + j < data.Length; j++)
                            {
                                barcodeData.Add(data[i + 4 + j]);
                            }
                            i += 4 + len;
                        }
                        else
                        {
                            i += 3;
                        }
                    }
                    
                    var barcodeTypeName = GetBarcodeTypeName(barcodeType);
                    _output.AppendLine($"[{barcodeTypeName}]");
                    if (barcodeData.Count > 0)
                    {
                        _output.AppendLine(Encoding.ASCII.GetString(barcodeData.ToArray()));
                    }
                    continue;
                }
                
                // GS ( k - QR Code and other 2D barcodes
                if (cmd == 0x28 && i + 2 < data.Length && data[i + 2] == 0x6B && i + 5 < data.Length)
                {
                    var pL = data[i + 3];
                    var pH = data[i + 4];
                    var len = pL | (pH << 8);
                    var fn = i + 6 < data.Length ? data[i + 6] : (byte)0;
                    
                    // QR Code store data (fn = 0x50, cn = 0x31)
                    if (i + 7 < data.Length && data[i + 5] == 0x31 && fn == 0x50)
                    {
                        var dataLen = len - 3;
                        var qrData = new List<byte>();
                        for (var j = 0; j < dataLen && i + 8 + j < data.Length; j++)
                        {
                            qrData.Add(data[i + 8 + j]);
                        }
                        i += 8 + dataLen;
                        // Store for later print command
                        continue;
                    }
                    
                    // QR Code print (fn = 0x51, cn = 0x31)
                    if (i + 6 < data.Length && data[i + 5] == 0x31 && fn == 0x51)
                    {
                        FlushCurrentLine();
                        _output.AppendLine("[QRCode]");
                        i += 8;
                        continue;
                    }
                    
                    // Skip other GS ( k commands
                    i += 5 + len;
                    continue;
                }
                
                // GS ( L - Graphics/Image commands
                if (cmd == 0x28 && i + 2 < data.Length && data[i + 2] == 0x4C && i + 5 < data.Length)
                {
                    var pL = data[i + 3];
                    var pH = data[i + 4];
                    var len = pL | (pH << 8);
                    
                    FlushCurrentLine();
                    _output.AppendLine("[IMAGE]");
                    i += 5 + len;
                    continue;
                }
                
                // GS v 0 - Raster image
                if (cmd == 0x76 && i + 2 < data.Length && data[i + 2] == 0x30 && i + 8 < data.Length)
                {
                    var widthBytes = data[i + 4] | (data[i + 5] << 8);
                    var height = data[i + 6] | (data[i + 7] << 8);
                    var imageDataLen = widthBytes * height;
                    
                    FlushCurrentLine();
                    _output.AppendLine("[IMAGE]");
                    i += 8 + imageDataLen;
                    continue;
                }
                
                // GS h n - Barcode height (ignore)
                if (cmd == 0x68 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // GS w n - Barcode width (ignore)
                if (cmd == 0x77 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // GS H n - HRI position (ignore)
                if (cmd == 0x48 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // GS f n - HRI font (ignore)
                if (cmd == 0x66 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // GS L nL nH - Left margin
                if (cmd == 0x4C && i + 3 < data.Length)
                {
                    i += 4;
                    continue;
                }
                
                // GS r n - Request status
                if (cmd == 0x72 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
                
                // GS b n - Smoothing
                if (cmd == 0x62 && i + 2 < data.Length)
                {
                    i += 3;
                    continue;
                }
            }
            
            // Handle printable characters and control codes
            if (b == 0x0A) // Line feed
            {
                FlushCurrentLine();
            }
            else if (b == 0x0D) // Carriage return (ignore in text output)
            {
                i++;
                continue;
            }
            else if (b == 0x09) // Horizontal tab
            {
                _currentLine.Append("    "); // 4 spaces
            }
            else if (b == 0x0C) // Form feed
            {
                FlushCurrentLine();
                _output.AppendLine();
            }
            else if (b >= 0x20 && b < 0x7F) // Printable ASCII
            {
                var ch = (char)b;
                AppendStyledChar(ch);
            }
            else if (b >= 0x80) // Extended ASCII / UTF-8 handling
            {
                // Try to decode UTF-8 sequences
                var decoded = TryDecodeUtf8(data, ref i);
                if (decoded != null)
                {
                    foreach (var ch in decoded)
                    {
                        AppendStyledChar(ch);
                    }
                    continue;
                }
                else
                {
                    // Fallback to extended ASCII
                    AppendStyledChar((char)b);
                }
            }
            
            i++;
        }
        
        return Task.CompletedTask;
    }

    public Task<byte[]> ReadStatusAsync(byte n = 1, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }

    public void Dispose()
    {
        FlushCurrentLine();
    }

    private void ResetState()
    {
        _bold = false;
        _underline = false;
        _invert = false;
        _alignment = 0;
        _charWidth = 1;
        _charHeight = 1;
    }

    private void AppendStyledChar(char ch)
    {
        if (_bold || _underline || _invert || _charWidth > 1 || _charHeight > 1)
        {
            // Repeat character for size effect (simple text representation)
            for (var w = 0; w < _charWidth; w++)
            {
                _currentLine.Append(ch);
            }
        }
        else
        {
            _currentLine.Append(ch);
        }
    }

    private void FlushCurrentLine()
    {
        if (_currentLine.Length == 0 && _output.Length > 0 && !_output.ToString().EndsWith("\n"))
        {
            _output.AppendLine();
            return;
        }
        
        if (_currentLine.Length == 0)
        {
            _output.AppendLine();
            return;
        }
        
        var line = _currentLine.ToString();
        
        // Apply alignment
        if (_alignment == 1) // Center
        {
            var padding = Math.Max(0, (_lineWidth - line.Length) / 2);
            line = new string(' ', padding) + line;
        }
        else if (_alignment == 2) // Right
        {
            var padding = Math.Max(0, _lineWidth - line.Length);
            line = new string(' ', padding) + line;
        }
        
        _output.AppendLine(line);
        _currentLine.Clear();
    }

    private static string? TryDecodeUtf8(byte[] data, ref int index)
    {
        var b = data[index];
        
        if ((b & 0x80) == 0) return null; // Single byte ASCII
        
        var bytesToRead = 0;
        if ((b & 0xE0) == 0xC0) bytesToRead = 2;
        else if ((b & 0xF0) == 0xE0) bytesToRead = 3;
        else if ((b & 0xF8) == 0xF0) bytesToRead = 4;
        else return null;
        
        if (index + bytesToRead > data.Length) return null;
        
        try
        {
            var utf8Bytes = new byte[bytesToRead];
            for (var i = 0; i < bytesToRead; i++)
            {
                utf8Bytes[i] = data[index + i];
            }
            
            var result = Encoding.UTF8.GetString(utf8Bytes);
            index += bytesToRead - 1; // -1 because the outer loop will increment
            return result;
        }
        catch
        {
            return null;
        }
    }

    private static string GetBarcodeTypeName(byte barcodeType)
    {
        return barcodeType switch
        {
            0 or 65 => "UPC-A",
            1 or 66 => "UPC-E",
            2 or 67 => "EAN13",
            3 or 68 => "EAN8",
            4 or 69 => "CODE39",
            5 or 70 => "ITF",
            6 or 71 => "CODABAR",
            72 => "CODE93",
            73 => "CODE128",
            _ => $"Barcode{barcodeType}"
        };
    }
}
