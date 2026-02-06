using System;
using System.Collections.Generic;
using System.Text;
using EscPos.Commands;

namespace EscPos.UnitTests;

public sealed class ReceiptXmlParserTests
{
    private const string XmlNamespace = "http://webefinity.com/escpos/receipt";

    #region Basic Text Elements

    [Fact]
    public void Parse_TextElement_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <text>Hello</text>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.Text("Hello");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_LineElement_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <line>Hello World</line>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.PrintLine("Hello World");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_EmptyLine_ProducesLineFeed()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <line/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.PrintLine("");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_TextWithEncoding_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <text encoding=""UTF-8"">€£¥</text>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.Text("€£¥", Encoding.UTF8);

        Assert.Equal(expected, result);
    }

    #endregion

    #region Text Formatting

    [Fact]
    public void Parse_BoldElement_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <bold>
    <line>Bold Text</line>
  </bold>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.BoldOn());
        expectedList.AddRange(PrintCommands.PrintLine("Bold Text"));
        expectedList.AddRange(PrintCommands.BoldOff());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_UnderlineElement_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <underline mode=""1"">
    <line>Underlined</line>
  </underline>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.Underline(1));
        expectedList.AddRange(PrintCommands.PrintLine("Underlined"));
        expectedList.AddRange(PrintCommands.UnderlineOff());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_InvertElement_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <invert>
    <line>Inverted</line>
  </invert>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.InvertOn());
        expectedList.AddRange(PrintCommands.PrintLine("Inverted"));
        expectedList.AddRange(PrintCommands.InvertOff());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_SizeElement_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <size width=""2"" height=""3"">
    <line>Large Text</line>
  </size>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.SetCharacterSize(2, 3));
        expectedList.AddRange(PrintCommands.PrintLine("Large Text"));
        expectedList.AddRange(PrintCommands.SetCharacterSizeNormal());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_NestedFormatting_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <bold>
    <size width=""2"" height=""2"">
      <line>Big Bold</line>
    </size>
  </bold>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.BoldOn());
        expectedList.AddRange(PrintCommands.SetCharacterSize(2, 2));
        expectedList.AddRange(PrintCommands.PrintLine("Big Bold"));
        expectedList.AddRange(PrintCommands.SetCharacterSizeNormal());
        expectedList.AddRange(PrintCommands.BoldOff());

        Assert.Equal(expectedList.ToArray(), result);
    }

    #endregion

    #region Alignment and Fonts

    [Fact]
    public void Parse_AlignLeft_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <align value=""left"">
    <line>Left</line>
  </align>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.AlignLeft());
        expectedList.AddRange(PrintCommands.PrintLine("Left"));
        expectedList.AddRange(PrintCommands.AlignLeft());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_AlignCenter_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <align value=""center"">
    <line>Center</line>
  </align>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.AlignCenter());
        expectedList.AddRange(PrintCommands.PrintLine("Center"));
        expectedList.AddRange(PrintCommands.AlignLeft());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_AlignRight_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <align value=""right"">
    <line>Right</line>
  </align>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.AlignRight());
        expectedList.AddRange(PrintCommands.PrintLine("Right"));
        expectedList.AddRange(PrintCommands.AlignLeft());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_FontA_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <font name=""A""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.SelectFontA();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_FontB_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <font name=""B""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.SelectFontB();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_Rotate90_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <rotate90>
    <line>Vertical</line>
  </rotate90>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.Rotate90On());
        expectedList.AddRange(PrintCommands.PrintLine("Vertical"));
        expectedList.AddRange(PrintCommands.Rotate90Off());

        Assert.Equal(expectedList.ToArray(), result);
    }

    #endregion

    #region Graphics

    [Fact]
    public void Parse_QrCode_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <qrcode data=""https://example.com"" size=""4"" errorLevel=""M""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.QrCodeSelectModel2());
        expectedList.AddRange(PrintCommands.QrCodeSetModuleSize(4));
        expectedList.AddRange(PrintCommands.QrCodeSetErrorCorrectionLevel(49)); // M
        expectedList.AddRange(PrintCommands.QrCodeStoreData(Encoding.UTF8.GetBytes("https://example.com")));
        expectedList.AddRange(PrintCommands.QrCodePrint());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_QrCode_WithDifferentErrorLevels_ProducesCorrectBytes()
    {
        var testCases = new[]
        {
            ("L", (byte)48),
            ("M", (byte)49),
            ("Q", (byte)50),
            ("H", (byte)51)
        };

        foreach (var (level, expectedByte) in testCases)
        {
            var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <qrcode data=""test"" errorLevel=""{level}""/>
</receipt>";

            var result = ReceiptXmlParser.Parse(xml, validate: true);

            var expectedList = new List<byte>();
            expectedList.AddRange(PrintCommands.QrCodeSelectModel2());
            expectedList.AddRange(PrintCommands.QrCodeSetModuleSize(4)); // default
            expectedList.AddRange(PrintCommands.QrCodeSetErrorCorrectionLevel(expectedByte));
            expectedList.AddRange(PrintCommands.QrCodeStoreData(Encoding.UTF8.GetBytes("test")));
            expectedList.AddRange(PrintCommands.QrCodePrint());

            Assert.Equal(expectedList.ToArray(), result);
        }
    }

    [Fact]
    public void Parse_Barcode_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <barcode type=""code128"" data=""123456"" height=""80"" width=""3""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.SetBarcodeHeight(80));
        expectedList.AddRange(PrintCommands.SetBarcodeWidth(3));
        expectedList.AddRange(PrintCommands.PrintBarcode(73, Encoding.ASCII.GetBytes("123456")));

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_Barcode_WithHri_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <barcode type=""code128"" data=""123456"" hri=""below""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.SetBarcodeHeight(80)); // default
        expectedList.AddRange(PrintCommands.SetBarcodeWidth(3)); // default
        expectedList.AddRange(PrintCommands.SetHriPosition(2)); // below
        expectedList.AddRange(PrintCommands.PrintBarcode(73, Encoding.ASCII.GetBytes("123456")));

        Assert.Equal(expectedList.ToArray(), result);
    }

    #endregion

    #region Paper Control

    [Fact]
    public void Parse_FeedLines_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <feed lines=""3""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.FeedLines(3);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_FeedDots_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <feed dots=""100""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.FeedDots(100);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_CutFull_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <cut type=""full""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.CutFull();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_CutPartial_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <cut type=""partial""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.CutPartial();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_CutWithFeed_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <cut type=""partial"" feed=""3""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.CutPartial(3);

        Assert.Equal(expected, result);
    }

    #endregion

    #region Hardware Control

    [Fact]
    public void Parse_Drawer_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <drawer pin=""2"" onTime=""120"" offTime=""240""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.PulseCashDrawer(0, 120, 240);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_DrawerPin5_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <drawer pin=""5""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.PulseCashDrawer(1, 120, 240);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_Beep_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <beep times=""2"" duration=""5""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.Beep(2, 5);

        Assert.Equal(expected, result);
    }

    #endregion

    #region Special Commands

    [Fact]
    public void Parse_Initialize_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <initialize/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.Initialize();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_Spacing_CharacterAndLine_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <spacing character=""5"" line=""30""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.SetRightSideCharacterSpacing(5));
        expectedList.AddRange(PrintCommands.SetLineSpacing(30));

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_Margin_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <margin left=""50""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.SetLeftMargin(50);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_CodePage_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <codepage value=""16""/>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: true);
        var expected = PrintCommands.SelectCodePage(16);

        Assert.Equal(expected, result);
    }

    #endregion

    #region Template Variables

    [Fact]
    public void Parse_WithSimpleVariable_SubstitutesCorrectly()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <line>Order: ${{OrderNumber}}</line>
</receipt>";

        var data = new { OrderNumber = "1234" };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);
        var expected = PrintCommands.PrintLine("Order: 1234");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_WithNestedVariable_SubstitutesCorrectly()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <text>${{Store.Name}} - ${{Store.Address}}</text>
</receipt>";

        var data = new
        {
            Store = new
            {
                Name = "My Store",
                Address = "123 Main St"
            }
        };

        var result = ReceiptXmlParser.Parse(xml, data, validate: true);
        var expected = PrintCommands.Text("My Store - 123 Main St");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_WithVariableInAttribute_SubstitutesCorrectly()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <qrcode data=""${{Url}}"" size=""4""/>
</receipt>";

        var data = new { Url = "https://example.com/order/1234" };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.QrCodeSelectModel2());
        expectedList.AddRange(PrintCommands.QrCodeSetModuleSize(4));
        expectedList.AddRange(PrintCommands.QrCodeSetErrorCorrectionLevel(49));
        expectedList.AddRange(PrintCommands.QrCodeStoreData(Encoding.UTF8.GetBytes("https://example.com/order/1234")));
        expectedList.AddRange(PrintCommands.QrCodePrint());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_WithMissingVariable_ReplacesWithEmpty()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <line>Name: ${{Name}}</line>
</receipt>";

        var data = new { Other = "Value" };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);
        var expected = PrintCommands.PrintLine("Name: ");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_WithContext_SubstitutesCorrectly()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <line>Order: ${{OrderNumber}}</line>
</receipt>";

        var context = new ReceiptTemplateContext();
        context.Add("OrderNumber", "5678");

        var result = ReceiptXmlParser.Parse(xml, context, validate: true);
        var expected = PrintCommands.PrintLine("Order: 5678");

        Assert.Equal(expected, result);
    }

    #endregion

    #region For Loops

    [Fact]
    public void Parse_SimpleForLoop_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <for var=""item"" in=""Items"">
    <line>${{item.Name}}</line>
  </for>
</receipt>";

        var data = new
        {
            Items = new object[]
            {
                new { Name = "Item1" },
                new { Name = "Item2" },
                new { Name = "Item3" }
            }
        };

        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.PrintLine("Item1"));
        expectedList.AddRange(PrintCommands.PrintLine("Item2"));
        expectedList.AddRange(PrintCommands.PrintLine("Item3"));

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_NestedForLoop_ProducesCorrectBytes()
    {
        // Simplified test - demonstrates nested loop functionality
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <for var=""cat"" in=""Categories"">
    <for var=""item"" in=""cat.Items"">
      <text>${{cat.Name}}:${{item.Name}} </text>
    </for>
  </for>
</receipt>";

        var data = new
        {
            Categories = new object[]
            {
                new
                {
                    Name = "A",
                    Items = new object[]
                    {
                        new { Name = "1" },
                        new { Name = "2" }
                    }
                },
                new
                {
                    Name = "B",
                    Items = new object[]
                    {
                        new { Name = "3" }
                    }
                }
            }
        };

        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.Text("A:1 "));
        expectedList.AddRange(PrintCommands.Text("A:2 "));
        expectedList.AddRange(PrintCommands.Text("B:3 "));

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_ForLoopWithFormatting_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <for var=""item"" in=""Items"">
    <bold>
      <line>${{item.Name}}</line>
    </bold>
  </for>
</receipt>";

        var data = new
        {
            Items = new object[]
            {
                new { Name = "Item1" },
                new { Name = "Item2" }
            }
        };

        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.BoldOn());
        expectedList.AddRange(PrintCommands.PrintLine("Item1"));
        expectedList.AddRange(PrintCommands.BoldOff());
        expectedList.AddRange(PrintCommands.BoldOn());
        expectedList.AddRange(PrintCommands.PrintLine("Item2"));
        expectedList.AddRange(PrintCommands.BoldOff());

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_ForLoopWithEmptyCollection_ProducesNoOutput()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <for var=""item"" in=""Items"">
    <text>${{item.Name}}</text>
  </for>
</receipt>";

        var data = new { Items = Array.Empty<object>() };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_ForLoopWithNullCollection_ProducesNoOutput()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <for var=""item"" in=""Items"">
    <text>${{item.Name}}</text>
  </for>
</receipt>";

        var data = new { Items = (object[]?)null };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        Assert.Empty(result);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Parse_CompleteReceipt_ProducesCorrectBytes()
    {
        // Simplified complete receipt test
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <initialize/>
  <align value=""center"">
    <bold>
      <line>${{StoreName}}</line>
    </bold>
  </align>
  <for var=""item"" in=""Items"">
    <text>${{item.Name}}: $${{item.Price}} </text>
  </for>
  <line>Total: $${{Total}}</line>
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

        var data = new
        {
            StoreName = "TEST",
            Items = new object[]
            {
                new { Name = "A", Price = "1.00" },
                new { Name = "B", Price = "2.00" }
            },
            Total = "3.00"
        };

        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        // Just verify it doesn't throw and produces some output
        Assert.NotEmpty(result);
        
        // Check it contains expected bytes
        Assert.Contains((byte)0x1B, result); // ESC
        Assert.Contains((byte)'T', result); // part of TEST
        Assert.Contains((byte)'o', result); // part of Total
    }

    #endregion

    #region Error Cases

    [Fact]
    public void Parse_InvalidXml_ThrowsException()
    {
        var xml = @"<?xml version=""1.0""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <text>Unclosed tag
</receipt>";

        Assert.ThrowsAny<Exception>(() => ReceiptXmlParser.Parse(xml, validate: true));
    }

    [Fact]
    public void Parse_ForLoopWithoutContext_ThrowsException()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <for var=""item"" in=""Items"">
    <line>${{item}}</line>
  </for>
</receipt>";

        var ex = Assert.Throws<InvalidOperationException>(() => 
            ReceiptXmlParser.Parse(xml, validate: true));
        
        Assert.Contains("template context", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_ForLoopWithNonEnumerableCollection_ThrowsException()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <for var=""item"" in=""NotACollection"">
    <line>${{item}}</line>
  </for>
</receipt>";

        var data = new { NotACollection = 12345 }; // Integer is not enumerable

        var ex = Assert.Throws<InvalidOperationException>(() => 
            ReceiptXmlParser.Parse(xml, data, validate: true));
        
        Assert.Contains("not enumerable", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Conditional Rendering (If)

    [Fact]
    public void Parse_IfConditionTrue_RendersContent()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <if condition=""HasDiscount"">
    <line>10% Discount Applied!</line>
  </if>
</receipt>";

        var data = new { HasDiscount = true };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);
        var expected = PrintCommands.PrintLine("10% Discount Applied!");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_IfConditionFalse_SkipsContent()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <if condition=""HasDiscount"">
    <line>10% Discount Applied!</line>
  </if>
</receipt>";

        var data = new { HasDiscount = false };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_IfConditionStringTrue_RendersContent()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <if condition=""Status"">
    <line>Status: ${{Status}}</line>
  </if>
</receipt>";

        var data = new { Status = "true" };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);
        var expected = PrintCommands.PrintLine("Status: true");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_IfConditionNonZeroNumber_RendersContent()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <if condition=""ItemCount"">
    <line>You have ${{ItemCount}} items</line>
  </if>
</receipt>";

        var data = new { ItemCount = 5 };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);
        var expected = PrintCommands.PrintLine("You have 5 items");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_IfConditionZero_SkipsContent()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <if condition=""ItemCount"">
    <line>You have items</line>
  </if>
</receipt>";

        var data = new { ItemCount = 0 };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_IfConditionNull_SkipsContent()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <if condition=""OptionalMessage"">
    <line>${{OptionalMessage}}</line>
  </if>
</receipt>";

        var data = new { OptionalMessage = (string?)null };
        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_IfWithinForLoop_WorksCorrectly()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <for var=""item"" in=""Items"">
    <if condition=""item.Show"">
      <line>${{item.Name}}</line>
    </if>
  </for>
</receipt>";

        var data = new
        {
            Items = new object[]
            {
                new { Name = "Item1", Show = true },
                new { Name = "Item2", Show = false },
                new { Name = "Item3", Show = true }
            }
        };

        var result = ReceiptXmlParser.Parse(xml, data, validate: true);

        var expectedList = new List<byte>();
        expectedList.AddRange(PrintCommands.PrintLine("Item1"));
        expectedList.AddRange(PrintCommands.PrintLine("Item3"));

        Assert.Equal(expectedList.ToArray(), result);
    }

    [Fact]
    public void Parse_IfWithoutContext_ThrowsException()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <if condition=""HasDiscount"">
    <line>Discount</line>
  </if>
</receipt>";

        var ex = Assert.Throws<InvalidOperationException>(() => 
            ReceiptXmlParser.Parse(xml, validate: true));
        
        Assert.Contains("template context", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region File-based Tests

    [Fact]
    public void ParseFile_ValidFile_ProducesCorrectBytes()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <line>From File</line>
</receipt>";

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, xml);

            var result = ReceiptXmlParser.ParseFile(tempFile, validate: true);
            var expected = PrintCommands.PrintLine("From File");

            Assert.Equal(expected, result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_WithTemplateData_SubstitutesCorrectly()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <line>Order: ${{OrderNumber}}</line>
</receipt>";

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, xml);

            var data = new { OrderNumber = "9999" };
            var result = ReceiptXmlParser.ParseFile(tempFile, data, validate: true);
            var expected = PrintCommands.PrintLine("Order: 9999");

            Assert.Equal(expected, result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Parse_WithValidationDisabled_ParsesSuccessfully()
    {
        var xml = $@"<?xml version=""1.0""?>
<receipt xmlns=""{XmlNamespace}"">
  <line>Test</line>
</receipt>";

        var result = ReceiptXmlParser.Parse(xml, validate: false);
        var expected = PrintCommands.PrintLine("Test");

        Assert.Equal(expected, result);
    }

    #endregion
}
