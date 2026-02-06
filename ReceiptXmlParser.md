# Receipt XML Parser for ESC/POS

A declarative XML-based approach to generating ESC/POS receipt commands.

## Overview

The `ReceiptXmlParser` allows you to define receipt layouts in XML format and automatically generates the corresponding ESC/POS byte commands. The XML is validated against a schema to ensure correctness before processing.

## Features

- **XML Schema Validation**: Validates receipt XML against XSD schema
- **Type Safety**: Compile-time validation of attributes and element structure
- **Template Variables**: Built-in `${variable}` substitution support
- **Conditional Rendering**: Show/hide content with `<if>` elements
- **For Loops**: Iterate over collections with `<for>` elements
- **Nested Formatting**: Supports nested text styling (bold within size, etc.)
- **Full Command Support**: All ESC/POS commands from PrintCommands
- **Encoding Support**: Per-element or document-wide encoding configuration
- **Template-Friendly**: Easy to use with template engines for dynamic content

## Usage

### Basic Example

```csharp
using EscPos.Commands;

var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <bold>
      <size width=""2"" height=""2"">
        <line>RESTAURANT NAME</line>
      </size>
    </bold>
  </align>
  
  <line>Order #1234</line>
  
  <qrcode data=""https://example.com/order/1234"" size=""4"" errorLevel=""M""/>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

byte[] escposCommands = ReceiptXmlParser.Parse(xml, validate: true);
// Send escposCommands to your printer
```

### Parse from File

```csharp
byte[] commands = ReceiptXmlParser.ParseFile("receipt.xml", validate: true);
```

### With Template Variables

Use `${variable}` syntax for dynamic content:

```csharp
var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <bold>
      <size width=""2"" height=""2"">
        <line>${StoreName}</line>
      </size>
    </bold>
  </align>
  
  <line>Order #${OrderNumber}</line>
  <line>Total: $${Total}</line>
  
  <qrcode data=""${ReceiptUrl}"" size=""4"" errorLevel=""M""/>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

var data = new
{
    StoreName = "MY RESTAURANT",
    OrderNumber = "1234",
    Total = "24.99",
    ReceiptUrl = "https://restaurant.com/receipt/1234"
};

byte[] commands = ReceiptXmlParser.Parse(xml, data, validate: true);
```

**See [ReceiptTemplateVariables.md](ReceiptTemplateVariables.md) for complete template documentation.**

### With Loops

Iterate over collections using `<for>` elements:

```csharp
var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <line>Order Items:</line>
  
  <for var=""item"" in=""Items"">
    <text>${item.Quantity}x ${item.Name}</text>
    <align value=""right"">
      <line>$${item.Price}</line>
    </align>
  </for>
  
  <cut type=""partial""/>
</receipt>";

var data = new
{
    Items = new object[]
    {
        new { Quantity = 1, Name = "Burger", Price = "12.99" },
        new { Quantity = 2, Name = "Fries", Price = "6.98" }
    }
};

byte[] commands = ReceiptXmlParser.Parse(xml, data);
```

**See [ReceiptLoops.md](ReceiptLoops.md) for complete loop documentation and examples.**

### With Conditionals

Show or hide content based on conditions:

```csharp
var xml = @"<?xml version=""1.0""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <line>Order Total: $${Total}</line>
  
  <if condition=""HasDiscount"">
    <line>** 10% DISCOUNT APPLIED **</line>
    <line>You saved: $${DiscountAmount}</line>
  </if>
  
  <if condition=""Customer.IsMember"">
    <line>Loyalty Points Earned: ${Customer.PointsEarned}</line>
  </if>
</receipt>";

var data = new
{
    Total = "24.99",
    HasDiscount = true,
    DiscountAmount = "2.50",
    Customer = new
    {
        IsMember = true,
        PointsEarned = 25
    }
};

byte[] commands = ReceiptXmlParser.Parse(xml, data);
```

**See [ReceiptConditionals.md](ReceiptConditionals.md) for complete conditional rendering guide.**

## XML Elements Reference

### Document Root

```xml
<receipt xmlns="http://webefinity.com/escpos/receipt" encoding="UTF-8">
  <!-- content -->
</receipt>
```

**Attributes:**
- `encoding` (optional): Default text encoding (e.g., "UTF-8", "Windows-1252")

### Text Elements

#### `<text>` - Plain text without line feed
```xml
<text>Hello World</text>
<text encoding="UTF-8">Special characters: ���</text>
```

#### `<line>` - Text with line feed
```xml
<line>First line</line>
<line>Second line</line>
<line/> <!-- Empty line -->
```

### Text Formatting

#### `<bold>` - Bold text
```xml
<bold>
  <line>This is bold</line>
</bold>
```

#### `<underline>` - Underlined text
```xml
<underline mode="1">
  <line>Single underline</line>
</underline>
<underline mode="2">
  <line>Double underline</line>
</underline>
```

**Attributes:**
- `mode`: 0 (off), 1 (1-dot), 2 (2-dot) - default: 1

#### `<invert>` - Inverted colors (white on black)
```xml
<invert>
  <line>White on black text</line>
</invert>
```

#### `<size>` - Character size multiplier
```xml
<size width="2" height="2">
  <line>2x sized text</line>
</size>
```

**Attributes:**
- `width`: 1-8 (default: 1)
- `height`: 1-8 (default: 1)

#### `<align>` - Text alignment
```xml
<align value="center">
  <line>Centered text</line>
</align>
```

**Attributes:**
- `value`: "left", "center", "right" (required)

#### `<font>` - Select font
```xml
<font name="A"/>  <!-- Font A (default) -->
<font name="B"/>  <!-- Font B (smaller/condensed) -->
```

#### `<rotate90>` - Rotate text 90 degrees
```xml
<rotate90>
  <line>Vertical text</line>
</rotate90>
```

### Graphics

#### `<qrcode>` - QR Code
```xml
<qrcode data="https://example.com" size="4" errorLevel="M"/>
```

**Attributes:**
- `data`: QR code content (required)
- `size`: Module size 1-16 (default: 4)
- `errorLevel`: "L", "M", "Q", "H" (default: "M")

#### `<barcode>` - Barcode
```xml
<barcode type="code128" data="123456" height="80" width="3" hri="below"/>
```

**Attributes:**
- `type`: "upca", "upce", "ean13", "ean8", "code39", "itf", "codabar", "code128" (required)
- `data`: Barcode data (required)
- `height`: Height in dots (default: 80)
- `width`: Width multiplier (default: 3)
- `hri`: Human Readable Interpretation - "none", "above", "below", "both" (optional)

#### `<image>` - BMP Image
```xml
<image path="logo.bmp" fn="50"/>
```

**Attributes:**
- `path`: Path to BMP file (required)
- `fn`: Print function 2 or 50 (default: 50)

### Paper Control

#### `<feed>` - Feed paper
```xml
<feed lines="3"/>   <!-- Feed 3 lines -->
<feed dots="100"/>  <!-- Feed 100 dots -->
```

**Attributes:**
- `lines`: Number of lines to feed
- `dots`: Number of dots to feed
(Only one should be specified)

#### `<cut>` - Cut paper
```xml
<cut type="partial" feed="3"/>
```

**Attributes:**
- `type`: "full" or "partial" (default: "partial")
- `feed`: Number of lines to feed before cutting (optional)

### Hardware Control

#### `<drawer>` - Open cash drawer
```xml
<drawer pin="2" onTime="120" offTime="240"/>
```

**Attributes:**
- `pin`: 2 or 5 (default: 2)
- `onTime`: Pulse on time in ms (default: 120)
- `offTime`: Pulse off time in ms (default: 240)

#### `<beep>` - Sound beeper
```xml
<beep times="2" duration="5"/>
```

**Attributes:**
- `times`: Number of beeps (default: 1)
- `duration`: Duration value (default: 5)

### Layout Control

#### `<spacing>` - Set spacing
```xml
<spacing character="5" line="30"/>
```

**Attributes:**
- `character`: Right-side character spacing (optional)
- `line`: Line spacing (optional)

#### `<margin>` - Set left margin
```xml
<margin left="50"/>
```

**Attributes:**
- `left`: Left margin in dots (required)

### Special Commands

#### `<initialize>` - Initialize printer
```xml
<initialize/>
```

#### `<codepage>` - Select code page
```xml
<codepage value="16"/>  <!-- Set to CP858 -->
```

**Attributes:**
- `value`: Code page number (required)

### Control Flow

#### `<for>` - Iterate over collections
```xml
<for var="item" in="Order.Items">
  <line>${item.Quantity}x ${item.Name} - $${item.Price}</line>
</for>
```

**Attributes:**
- `var`: Variable name for each item (required)
- `in`: Path to the collection (required)

**Features:**
- Works with any `IEnumerable` type (arrays, lists, etc.)
- Supports nested loops
- Loop variable accessible via `${varName}`
- Can access nested properties: `${item.Property.SubProperty}`

**See [ReceiptLoops.md](ReceiptLoops.md) for complete examples.**

#### `<if>` - Conditional rendering
```xml
<if condition="HasDiscount">
  <line>** Discount Applied **</line>
</if>
```

**Attributes:**
- `condition`: Path to a boolean variable (required)

**Features:**
- Evaluates boolean, string, numeric, and null values
- Supports nested properties: `Customer.IsMember`
- Works inside `<for>` loops
- Can be nested for complex logic
- Non-existent variables evaluate to false

**Condition Evaluation:**
- `true` / non-zero numbers / non-empty strings → renders content
- `false` / `0` / `null` / empty strings → skips content

**See [ReceiptConditionals.md](ReceiptConditionals.md) for complete examples.**

## Nested Elements

Many formatting elements can be nested:

```xml
<align value="center">
  <bold>
    <size width="2" height="2">
      <underline mode="1">
        <line>Multiple styles!</line>
      </underline>
    </size>
  </bold>
</align>
```

## Complete Example

```xml
<?xml version="1.0" encoding="utf-8"?>
<receipt xmlns="http://webefinity.com/escpos/receipt">
  <initialize/>
  
  <!-- Store Header -->
  <align value="center">
    <image path="logo.bmp"/>
    <line/>
    <size width="2" height="2">
      <bold><line>Restaurant Name</line></bold>
    </size>
    <line>123 Main Street</line>
    <line>City, State 12345</line>
    <line>Phone: (555) 123-4567</line>
  </align>
  
  <line>================================</line>
  
  <!-- Order Details -->
  <bold><text>Order #1234</text></bold>
  <line> - Table 5</line>
  <line>Date: 2024-02-04 14:30:00</line>
  <line>Server: John Doe</line>
  
  <line>================================</line>
  
  <!-- Items -->
  <line>1x Burger.................$12.99</line>
  <line>  - Extra cheese</line>
  <line>2x French Fries...........$6.98</line>
  <line>1x Soft Drink.............$2.50</line>
  
  <line>--------------------------------</line>
  
  <!-- Totals -->
  <align value="right">
    <line>Subtotal:         $22.47</line>
    <line>Tax (10%):         $2.25</line>
    <line>--------------------------------</line>
    <bold>
      <size width="2" height="1">
        <line>TOTAL:         $24.72</line>
      </size>
    </bold>
  </align>
  
  <line>================================</line>
  
  <!-- Payment -->
  <line>Payment Method: Credit Card</line>
  <line>Card: **** **** **** 1234</line>
  <line>Auth Code: 123456</line>
  
  <line>================================</line>
  
  <!-- QR Code for Digital Receipt -->
  <align value="center">
    <line/>
    <qrcode data="https://restaurant.com/receipt/1234" 
            size="4" 
            errorLevel="M"/>
    <line/>
    <line>Scan for digital receipt</line>
  </align>
  
  <!-- Footer -->
  <align value="center">
    <line/>
    <line>Thank you for your order!</line>
    <line>Visit us again soon</line>
    <line/>
    <line>www.restaurant.com</line>
  </align>
  
  <!-- Cut with feed -->
  <feed lines="3"/>
  <cut type="partial"/>
</receipt>
```

## Error Handling

The parser will throw exceptions for:

- **Invalid XML**: Malformed XML syntax
- **Schema Validation**: Elements/attributes that don't match schema
- **Missing Files**: Image files that don't exist
- **Invalid Values**: Out-of-range attribute values

```csharp
try
{
    var commands = ReceiptXmlParser.Parse(xml, validate: true);
    // Send to printer
}
catch (XmlSchemaValidationException ex)
{
    Console.WriteLine($"XML validation error: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Image file not found: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Tips

1. **Use templates**: Store XML templates and substitute values dynamically
2. **Validate during development**: Enable validation during testing to catch errors early
3. **Disable validation in production**: Set `validate: false` for better performance
4. **Test with actual printer**: Different printers may have slightly different capabilities
5. **Keep it simple**: Start with basic layouts and add complexity as needed

## Schema Location

The XSD schema is embedded as a resource in the assembly. To use it with XML editors for IntelliSense:

1. Extract `ReceiptSchema.xsd` from the assembly resources
2. Reference it in your XML:
   ```xml
   <receipt xmlns="http://webefinity.com/escpos/receipt"
            xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            xsi:schemaLocation="http://webefinity.com/escpos/receipt ReceiptSchema.xsd">
   ```

## License

MIT License - See project LICENSE file for details.

