using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace EscPos.Commands;

public class ReceiptXmlParser
{
    private const string SchemaNamespace = "http://webefinity.com/escpos/receipt";
    private static readonly XmlSchemaSet SchemaSet = LoadSchema();

    private readonly List<string> _validationErrors = [];
    private readonly List<byte> _buffer = [];
    private Encoding _currentEncoding = Encoding.ASCII;
    private ReceiptTemplateContext? _templateContext;

    public static byte[] Parse(string xml, bool validate = true)
    {
        var parser = new ReceiptXmlParser();
        return parser.ParseXml(xml, validate);
    }

    public static byte[] Parse(string xml, ReceiptTemplateContext context, bool validate = true)
    {
        var parser = new ReceiptXmlParser();
        return parser.ParseXml(xml, context, validate);
    }

    public static byte[] Parse(string xml, object templateData, bool validate = true)
    {
        var context = new ReceiptTemplateContext(templateData);
        return Parse(xml, context, validate);
    }

    public static byte[] ParseFile(string filePath, bool validate = true)
    {
        var xml = File.ReadAllText(filePath);
        return Parse(xml, validate);
    }

    public static byte[] ParseFile(string filePath, ReceiptTemplateContext context, bool validate = true)
    {
        var xml = File.ReadAllText(filePath);
        return Parse(xml, context, validate);
    }

    public static byte[] ParseFile(string filePath, object templateData, bool validate = true)
    {
        var xml = File.ReadAllText(filePath);
        var context = new ReceiptTemplateContext(templateData);
        return Parse(xml, context, validate);
    }

    private static XmlSchemaSet LoadSchema()
    {
        var schemaSet = new XmlSchemaSet();
        var assembly = typeof(ReceiptXmlParser).Assembly;
        var resourceName = "EscPos.Commands.ReceiptSchema.xsd";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is not null)
        {
            using var reader = XmlReader.Create(stream);
            schemaSet.Add(SchemaNamespace, reader);
        }

        return schemaSet;
    }

    private byte[] ParseXml(string xml, bool validate)
    {
        return ParseXml(xml, null, validate);
    }

    private byte[] ParseXml(string xml, ReceiptTemplateContext? context, bool validate)
    {
        _validationErrors.Clear();
        _buffer.Clear();
        _templateContext = context;

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true
        };

        if (validate && SchemaSet.Count > 0)
        {
            settings.Schemas = SchemaSet;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += ValidationCallback;
        }

        using var stringReader = new StringReader(xml);
        using var reader = XmlReader.Create(stringReader, settings);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                ProcessElement(reader);
            }
        }

        if (_validationErrors.Count > 0)
        {
            throw new XmlSchemaValidationException($"XML validation failed: {string.Join("; ", _validationErrors)}");
        }

        return [.. _buffer];
    }

    private void ValidationCallback(object? sender, ValidationEventArgs e)
    {
        _validationErrors.Add($"{e.Severity}: {e.Message}");
    }

    private void ProcessElement(XmlReader reader)
    {
        var elementName = reader.LocalName;

        switch (elementName)
        {
            case "receipt":
                ProcessReceipt(reader);
                break;
            case "text":
                ProcessText(reader);
                break;
            case "line":
                ProcessLine(reader);
                break;
            case "bold":
                ProcessBold(reader);
                break;
            case "underline":
                ProcessUnderline(reader);
                break;
            case "invert":
                ProcessInvert(reader);
                break;
            case "size":
                ProcessSize(reader);
                break;
            case "align":
                ProcessAlign(reader);
                break;
            case "font":
                ProcessFont(reader);
                break;
            case "rotate90":
                ProcessRotate90(reader);
                break;
            case "qrcode":
                ProcessQrCode(reader);
                break;
            case "barcode":
                ProcessBarcode(reader);
                break;
            case "image":
                ProcessImage(reader);
                break;
            case "feed":
                ProcessFeed(reader);
                break;
            case "cut":
                ProcessCut(reader);
                break;
            case "drawer":
                ProcessDrawer(reader);
                break;
            case "beep":
                ProcessBeep(reader);
                break;
            case "initialize":
                _buffer.AddRange(PrintCommands.Initialize());
                break;
            case "for":
                ProcessFor(reader);
                break;
            case "spacing":
                ProcessSpacing(reader);
                break;
            case "margin":
                ProcessMargin(reader);
                break;
            case "codepage":
                ProcessCodePage(reader);
                break;
        }
    }

    private void ProcessReceipt(XmlReader reader)
    {
        var encoding = reader.GetAttribute("encoding");
        if (!string.IsNullOrEmpty(encoding))
        {
            _currentEncoding = Encoding.GetEncoding(encoding);
        }
    }

    private void ProcessText(XmlReader reader)
    {
        var encoding = GetEncoding(reader);
        var content = reader.ReadElementContentAsString();
        if (!string.IsNullOrEmpty(content))
        {
            content = SubstituteVariables(content);
            _buffer.AddRange(PrintCommands.Text(content, encoding));
        }
    }

    private void ProcessLine(XmlReader reader)
    {
        var encoding = GetEncoding(reader);
        var content = reader.ReadElementContentAsString();
        content = SubstituteVariables(content);
        _buffer.AddRange(PrintCommands.PrintLine(content, encoding));
    }

    private void ProcessBold(XmlReader reader)
    {
        _buffer.AddRange(PrintCommands.BoldOn());
        
        if (!reader.IsEmptyElement)
        {
            ProcessChildContent(reader, "bold");
        }
        
        _buffer.AddRange(PrintCommands.BoldOff());
    }

    private void ProcessUnderline(XmlReader reader)
    {
        var mode = reader.GetAttribute("mode");
        var modeValue = string.IsNullOrEmpty(mode) ? (byte)1 : byte.Parse(mode);
        
        _buffer.AddRange(PrintCommands.Underline(modeValue));
        
        if (!reader.IsEmptyElement)
        {
            ProcessChildContent(reader, "underline");
        }
        
        _buffer.AddRange(PrintCommands.UnderlineOff());
    }

    private void ProcessInvert(XmlReader reader)
    {
        _buffer.AddRange(PrintCommands.InvertOn());
        
        if (!reader.IsEmptyElement)
        {
            ProcessChildContent(reader, "invert");
        }
        
        _buffer.AddRange(PrintCommands.InvertOff());
    }

    private void ProcessSize(XmlReader reader)
    {
        var width = reader.GetAttribute("width");
        var height = reader.GetAttribute("height");
        
        var widthValue = string.IsNullOrEmpty(width) ? (byte)1 : byte.Parse(width);
        var heightValue = string.IsNullOrEmpty(height) ? (byte)1 : byte.Parse(height);
        
        _buffer.AddRange(PrintCommands.SetCharacterSize(widthValue, heightValue));
        
        if (!reader.IsEmptyElement)
        {
            ProcessChildContent(reader, "size");
        }
        
        _buffer.AddRange(PrintCommands.SetCharacterSizeNormal());
    }

    private void ProcessAlign(XmlReader reader)
    {
        var value = reader.GetAttribute("value");
        
        _buffer.AddRange(value switch
        {
            "left" => PrintCommands.AlignLeft(),
            "center" => PrintCommands.AlignCenter(),
            "right" => PrintCommands.AlignRight(),
            _ => PrintCommands.AlignLeft()
        });
        
        if (!reader.IsEmptyElement)
        {
            ProcessChildContent(reader, "align");
        }
        
        _buffer.AddRange(PrintCommands.AlignLeft());
    }

    private void ProcessFont(XmlReader reader)
    {
        var name = reader.GetAttribute("name");
        _buffer.AddRange(name switch
        {
            "A" => PrintCommands.SelectFontA(),
            "B" => PrintCommands.SelectFontB(),
            _ => PrintCommands.SelectFontA()
        });
    }

    private void ProcessRotate90(XmlReader reader)
    {
        _buffer.AddRange(PrintCommands.Rotate90On());
        
        if (!reader.IsEmptyElement)
        {
            ProcessChildContent(reader, "rotate90");
        }
        
        _buffer.AddRange(PrintCommands.Rotate90Off());
    }

    private void ProcessQrCode(XmlReader reader)
    {
        var data = SubstituteVariables(reader.GetAttribute("data") ?? "");
        var size = reader.GetAttribute("size");
        var errorLevel = reader.GetAttribute("errorLevel");
        
        var sizeValue = string.IsNullOrEmpty(size) ? (byte)4 : byte.Parse(size);
        var errorLevelValue = errorLevel switch
        {
            "L" => (byte)48,
            "M" => (byte)49,
            "Q" => (byte)50,
            "H" => (byte)51,
            _ => (byte)49
        };
        
        _buffer.AddRange(PrintCommands.QrCodeSelectModel2());
        _buffer.AddRange(PrintCommands.QrCodeSetModuleSize(sizeValue));
        _buffer.AddRange(PrintCommands.QrCodeSetErrorCorrectionLevel(errorLevelValue));
        _buffer.AddRange(PrintCommands.QrCodeStoreData(Encoding.UTF8.GetBytes(data)));
        _buffer.AddRange(PrintCommands.QrCodePrint());
    }

    private void ProcessBarcode(XmlReader reader)
    {
        var type = reader.GetAttribute("type") ?? "code128";
        var data = SubstituteVariables(reader.GetAttribute("data") ?? "");
        var height = reader.GetAttribute("height");
        var width = reader.GetAttribute("width");
        var hri = reader.GetAttribute("hri");
        
        var heightValue = string.IsNullOrEmpty(height) ? (byte)80 : byte.Parse(height);
        var widthValue = string.IsNullOrEmpty(width) ? (byte)3 : byte.Parse(width);
        
        _buffer.AddRange(PrintCommands.SetBarcodeHeight(heightValue));
        _buffer.AddRange(PrintCommands.SetBarcodeWidth(widthValue));
        
        if (!string.IsNullOrEmpty(hri))
        {
            var hriValue = hri switch
            {
                "none" => (byte)0,
                "above" => (byte)1,
                "below" => (byte)2,
                "both" => (byte)3,
                _ => (byte)0
            };
            _buffer.AddRange(PrintCommands.SetHriPosition(hriValue));
        }
        
        var barcodeType = type.ToLowerInvariant() switch
        {
            "upca" => (byte)65,
            "upce" => (byte)66,
            "ean13" => (byte)67,
            "ean8" => (byte)68,
            "code39" => (byte)69,
            "itf" => (byte)70,
            "codabar" => (byte)71,
            "code128" => (byte)73,
            _ => (byte)73
        };
        
        _buffer.AddRange(PrintCommands.PrintBarcode(barcodeType, Encoding.ASCII.GetBytes(data)));
    }

    private void ProcessImage(XmlReader reader)
    {
        var path = SubstituteVariables(reader.GetAttribute("path") ?? "");
        var fn = reader.GetAttribute("fn");
        
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Image path is required");
        
        if (!File.Exists(path))
            throw new FileNotFoundException($"Image file not found: {path}");
        
        var fnValue = string.IsNullOrEmpty(fn) ? (byte)50 : byte.Parse(fn);
        var bmpData = File.ReadAllBytes(path);
        
        _buffer.AddRange(PrintCommands.PrintBmpImage(bmpData, fnValue));
    }

    private void ProcessFeed(XmlReader reader)
    {
        var lines = reader.GetAttribute("lines");
        var dots = reader.GetAttribute("dots");
        
        if (!string.IsNullOrEmpty(lines))
        {
            _buffer.AddRange(PrintCommands.FeedLines(byte.Parse(lines)));
        }
        else if (!string.IsNullOrEmpty(dots))
        {
            _buffer.AddRange(PrintCommands.FeedDots(byte.Parse(dots)));
        }
    }

    private void ProcessCut(XmlReader reader)
    {
        var type = reader.GetAttribute("type");
        var feed = reader.GetAttribute("feed");
        
        if (!string.IsNullOrEmpty(feed))
        {
            var feedValue = byte.Parse(feed);
            _buffer.AddRange(type == "full" 
                ? PrintCommands.CutFull(feedValue) 
                : PrintCommands.CutPartial(feedValue));
        }
        else
        {
            _buffer.AddRange(type == "full" 
                ? PrintCommands.CutFull() 
                : PrintCommands.CutPartial());
        }
    }

    private void ProcessDrawer(XmlReader reader)
    {
        var pin = reader.GetAttribute("pin");
        var onTime = reader.GetAttribute("onTime");
        var offTime = reader.GetAttribute("offTime");
        
        var pinValue = string.IsNullOrEmpty(pin) ? (byte)0 : (byte)(pin == "5" ? 1 : 0);
        var onTimeValue = string.IsNullOrEmpty(onTime) ? (byte)120 : byte.Parse(onTime);
        var offTimeValue = string.IsNullOrEmpty(offTime) ? (byte)240 : byte.Parse(offTime);
        
        _buffer.AddRange(PrintCommands.PulseCashDrawer(pinValue, onTimeValue, offTimeValue));
    }

    private void ProcessBeep(XmlReader reader)
    {
        var times = reader.GetAttribute("times");
        var duration = reader.GetAttribute("duration");
        
        var timesValue = string.IsNullOrEmpty(times) ? (byte)1 : byte.Parse(times);
        var durationValue = string.IsNullOrEmpty(duration) ? (byte)5 : byte.Parse(duration);
        
        _buffer.AddRange(PrintCommands.Beep(timesValue, durationValue));
    }

    private void ProcessSpacing(XmlReader reader)
    {
        var character = reader.GetAttribute("character");
        var line = reader.GetAttribute("line");
        
        if (!string.IsNullOrEmpty(character))
        {
            _buffer.AddRange(PrintCommands.SetRightSideCharacterSpacing(byte.Parse(character)));
        }
        
        if (!string.IsNullOrEmpty(line))
        {
            _buffer.AddRange(PrintCommands.SetLineSpacing(byte.Parse(line)));
        }
    }

    private void ProcessFor(XmlReader reader)
    {
        if (_templateContext is null)
            throw new InvalidOperationException("For loops require a template context. Use Parse(xml, context) or Parse(xml, data).");

        var varName = reader.GetAttribute("var");
        var inPath = reader.GetAttribute("in");

        if (string.IsNullOrEmpty(varName))
            throw new ArgumentException("For loop 'var' attribute is required.");

        if (string.IsNullOrEmpty(inPath))
            throw new ArgumentException("For loop 'in' attribute is required.");

        var collection = _templateContext.GetValue(inPath);

        if (collection is null)
        {
            if (!reader.IsEmptyElement)
            {
                reader.Skip();
            }
            return;
        }

        if (collection is not System.Collections.IEnumerable enumerable)
            throw new InvalidOperationException($"The value at '{inPath}' is not enumerable.");

        if (reader.IsEmptyElement)
            return;

        var depth = reader.Depth;
        var childXml = new System.Text.StringBuilder();
        
        while (reader.Read() && reader.Depth > depth)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                childXml.Append('<').Append(reader.Name);
                
                if (reader.HasAttributes)
                {
                    for (var i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToAttribute(i);
                        childXml.Append(' ')
                            .Append(reader.Name)
                            .Append("=\"")
                            .Append(System.Security.SecurityElement.Escape(reader.Value))
                            .Append('"');
                    }
                    reader.MoveToElement();
                }
                
                if (reader.IsEmptyElement)
                {
                    childXml.Append("/>");
                }
                else
                {
                    childXml.Append('>');
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                childXml.Append("</").Append(reader.Name).Append('>');
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                childXml.Append(System.Security.SecurityElement.Escape(reader.Value));
            }
            else if (reader.NodeType == XmlNodeType.CDATA)
            {
                childXml.Append("<![CDATA[").Append(reader.Value).Append("]]>");
            }
        }

        var childContent = childXml.ToString();

        foreach (var item in enumerable)
        {
            _templateContext.PushScope();
            try
            {
                _templateContext.SetLoopVariable(varName, item);
                
                using var stringReader = new StringReader(childContent);
                using var childReader = XmlReader.Create(stringReader, new XmlReaderSettings 
                { 
                    ConformanceLevel = ConformanceLevel.Fragment,
                    IgnoreWhitespace = true 
                });

                while (childReader.Read())
                {
                    if (childReader.NodeType == XmlNodeType.Element)
                    {
                        ProcessElement(childReader);
                    }
                    else if (childReader.NodeType == XmlNodeType.Text)
                    {
                        var text = SubstituteVariables(childReader.Value);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            _buffer.AddRange(PrintCommands.Text(text, _currentEncoding));
                        }
                    }
                }
            }
            finally
            {
                _templateContext.PopScope();
            }
        }
    }

    private void ProcessMargin(XmlReader reader)
    {
        var left = reader.GetAttribute("left");
        
        if (!string.IsNullOrEmpty(left))
        {
            _buffer.AddRange(PrintCommands.SetLeftMargin(ushort.Parse(left)));
        }
    }

    private void ProcessCodePage(XmlReader reader)
    {
        var value = reader.GetAttribute("value");
        
        if (!string.IsNullOrEmpty(value))
        {
            _buffer.AddRange(PrintCommands.SelectCodePage(byte.Parse(value)));
        }
    }

    private void ProcessChildContent(XmlReader reader, string parentElement)
    {
        var depth = reader.Depth;
        
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == parentElement)
            {
                break;
            }
            
            if (reader.NodeType == XmlNodeType.Text)
            {
                var text = reader.Value;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    text = SubstituteVariables(text);
                    _buffer.AddRange(PrintCommands.Text(text, _currentEncoding));
                }
            }
            else if (reader.NodeType == XmlNodeType.Element)
            {
                ProcessElement(reader);
            }
        }
    }

    private Encoding GetEncoding(XmlReader reader)
    {
        var encoding = reader.GetAttribute("encoding");
        return string.IsNullOrEmpty(encoding) ? _currentEncoding : Encoding.GetEncoding(encoding);
    }

    private string SubstituteVariables(string text)
    {
        if (_templateContext is null || string.IsNullOrEmpty(text))
            return text;

        return _templateContext.Substitute(text);
    }
}
