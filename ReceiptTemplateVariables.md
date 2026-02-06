# Receipt XML Template Variables

This document describes how to use template variables in Receipt XML documents with the `${variable}` syntax.

## Overview

Template variables allow you to create reusable receipt templates with dynamic content. Variables are specified using `${variableName}` syntax, similar to C# string interpolation.

## Basic Usage

### With Anonymous Objects

```csharp
var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <line>Order #${OrderNumber}</line>
  <line>Total: $${Total}</line>
</receipt>";

var data = new
{
    OrderNumber = "1234",
    Total = "24.99"
};

byte[] commands = ReceiptXmlParser.Parse(xml, data, validate: true);
```

### With ReceiptTemplateContext

```csharp
var context = new ReceiptTemplateContext();
context.Add("OrderNumber", "1234");
context.Add("Total", "24.99");

byte[] commands = ReceiptXmlParser.Parse(xml, context, validate: true);
```

### With Strongly-Typed Classes

```csharp
public class OrderData
{
    public string OrderNumber { get; set; }
    public decimal Total { get; set; }
    public DateTime Date { get; set; }
}

var order = new OrderData
{
    OrderNumber = "1234",
    Total = 24.99m,
    Date = DateTime.Now
};

byte[] commands = ReceiptXmlParser.Parse(xml, order, validate: true);
```

## Nested Properties

Access nested object properties using dot notation:

```csharp
var xml = @"<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <line>${Store.Name}</line>
  <line>${Store.Address.Street}</line>
  <line>${Store.Address.City}, ${Store.Address.State}</line>
</receipt>";

var data = new
{
    Store = new
    {
        Name = "My Store",
        Address = new
        {
            Street = "123 Main St",
            City = "Anytown",
            State = "CA"
        }
    }
};

byte[] commands = ReceiptXmlParser.Parse(xml, data);
```

## Variable Locations

Variables can be used in:

### Text Content
```xml
<text>Order #${OrderNumber}</text>
<line>Total: $${Total}</line>
```

### Element Attributes
```xml
<qrcode data="${OrderUrl}" size="4"/>
<barcode type="code128" data="${BarcodeData}"/>
<image path="${LogoPath}"/>
```

### Nested Content
```xml
<bold>
  <size width="2" height="2">
    <line>${StoreName}</line>
  </size>
</bold>
```

## Complete Example

```csharp
var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <!-- Header with nested properties -->
  <align value=""center"">
    <size width=""2"" height=""2"">
      <bold>
        <line>${Store.Name}</line>
      </bold>
    </size>
    <line>${Store.Address}</line>
    <line>${Store.City}, ${Store.State} ${Store.Zip}</line>
    <line>Phone: ${Store.Phone}</line>
  </align>
  
  <line>================================</line>
  
  <!-- Order details -->
  <bold><text>Order #${Order.Number}</text></bold>
  <line> - Table ${Order.Table}</line>
  <line>Date: ${Order.Date}</line>
  <line>Server: ${Order.Server}</line>
  
  <line>================================</line>
  
  <!-- Items (pre-formatted in your code) -->
  <line>${Item1.Description}</line>
  <line>${Item2.Description}</line>
  <line>${Item3.Description}</line>
  
  <line>--------------------------------</line>
  
  <!-- Totals -->
  <align value=""right"">
    <line>Subtotal:    $${Order.Subtotal}</line>
    <line>Tax:         $${Order.Tax}</line>
    <bold>
      <size width=""2"" height=""1"">
        <line>Total:       $${Order.Total}</line>
      </size>
    </bold>
  </align>
  
  <line>================================</line>
  
  <!-- QR Code with variable URL -->
  <align value=""center"">
    <qrcode data=""${Order.ReceiptUrl}"" size=""4"" errorLevel=""M""/>
    <line>Scan for digital receipt</line>
  </align>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

// Prepare data
var receiptData = new
{
    Store = new
    {
        Name = "RESTAURANT NAME",
        Address = "123 Main Street",
        City = "Anytown",
        State = "CA",
        Zip = "12345",
        Phone = "(555) 123-4567"
    },
    Order = new
    {
        Number = "1234",
        Table = "5",
        Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        Server = "John Doe",
        Subtotal = "22.47",
        Tax = "2.25",
        Total = "24.72",
        ReceiptUrl = "https://restaurant.com/receipt/1234"
    },
    Item1 = new { Description = "1x Burger.................$12.99" },
    Item2 = new { Description = "2x Fries..................$6.98" },
    Item3 = new { Description = "1x Soda...................$2.50" }
};

// Generate receipt
byte[] escposCommands = ReceiptXmlParser.Parse(xml, receiptData, validate: true);

// Send to printer
// SendToPrinter(escposCommands);
```

## ReceiptTemplateContext API

### Constructor

```csharp
// Empty context
var context = new ReceiptTemplateContext();

// From object (reflects properties)
var context = new ReceiptTemplateContext(myObject);

// From anonymous object
var context = ReceiptTemplateContext.FromAnonymous(new { Name = "Value" });

// From dictionary
var dict = new Dictionary<string, object?> { ["Name"] = "Value" };
var context = ReceiptTemplateContext.FromDictionary(dict);
```

### Methods

```csharp
// Add single variable
context.Add("VariableName", "Value");

// Add multiple variables
var vars = new Dictionary<string, object?>
{
    ["Name"] = "John",
    ["Age"] = 30
};
context.AddRange(vars);

// Add all properties from an object
context.AddFromObject(myObject);

// Add with prefix (for namespacing)
context.AddFromObject(myStore, "Store"); // Creates Store.Name, Store.Address, etc.

// Get value
object? value = context.GetValue("VariableName");

// Substitute variables in text
string result = context.Substitute("Hello ${Name}!");
```

## Handling Missing Variables

When a variable is not found, it is replaced with an empty string:

```csharp
var xml = @"<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <line>Name: ${Name}</line>
  <line>Age: ${Age}</line>
</receipt>";

var data = new { Name = "John" }; // Age is missing

// Result: "Name: John\nAge: \n"
```

## Special Cases

### Escaping Dollar Signs

If you need a literal `$` character followed by `{`, currently there's no escape mechanism. Consider using:

```csharp
// Workaround: Add it as a variable
context.Add("Dollar", "$");
// Then use: ${Dollar}{something}
```

### Null Values

Null values are treated as empty strings:

```csharp
var data = new { Name = (string?)null };
// ${Name} becomes ""
```

### IFormattable Types

For types implementing `IFormattable` (like DateTime, decimal), you can include format strings:

```csharp
// Note: Format specifiers in variable names are planned for future enhancement
var data = new
{
    Date = DateTime.Now.ToString("yyyy-MM-dd"),
    Price = 12.99m.ToString("F2")
};
```

## Best Practices

### 1. Use Strong Types for Complex Data

```csharp
public class ReceiptData
{
    public StoreInfo Store { get; set; }
    public OrderInfo Order { get; set; }
    public List<OrderItem> Items { get; set; }
}

// Better than anonymous objects for maintainability
```

### 2. Pre-Format Complex Content

```csharp
// Format items in your code, not in XML
var data = new
{
    Items = string.Join("\n", order.Items.Select(i => 
        $"{i.Quantity}x {i.Name.PadRight(20)}.....${i.Price:F2}"))
};
```

### 3. Validate Data Before Parsing

```csharp
if (string.IsNullOrEmpty(orderNumber))
    throw new ArgumentException("Order number is required");

var data = new { OrderNumber = orderNumber };
var commands = ReceiptXmlParser.Parse(xml, data);
```

### 4. Store Templates as Files

```csharp
// Store template in file system or database
var template = File.ReadAllText("templates/order-receipt.xml");
var commands = ReceiptXmlParser.Parse(template, orderData);
```

### 5. Use Descriptive Variable Names

```xml
<!-- Good -->
<line>Order #${Order.Number}</line>
<line>Date: ${Order.FormattedDate}</line>

<!-- Less clear -->
<line>Order #${ON}</line>
<line>Date: ${FD}</line>
```

## Integration with Template Engines

For complex scenarios with loops and conditionals, consider pre-processing with a template engine:

### With Scriban

```csharp
using Scriban;

var scirbanTemplate = @"<?xml version=""1.0""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  {{ for item in items }}
  <line>{{ item.quantity }}x {{ item.name }}...${{ item.price }}</line>
  {{ end }}
  <line>Total: ${{ total }}</line>
</receipt>";

var template = Template.Parse(scribanTemplate);
var xml = template.Render(new { items = orderItems, total = orderTotal });

// Then parse the rendered XML
byte[] commands = ReceiptXmlParser.Parse(xml);
```

### With Liquid

```csharp
using Fluid;

var liquidTemplate = @"<?xml version=""1.0""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  {% for item in items %}
  <line>{{ item.quantity }}x {{ item.name }}...${{ item.price }}</line>
  {% endfor %}
  <line>Total: ${{ total }}</line>
</receipt>";

var parser = new FluidParser();
var template = parser.Parse(liquidTemplate);
var xml = template.Render(new TemplateContext(data));

byte[] commands = ReceiptXmlParser.Parse(xml);
```

## Error Handling

```csharp
try
{
    var commands = ReceiptXmlParser.Parse(xml, data, validate: true);
    SendToPrinter(commands);
}
catch (XmlSchemaValidationException ex)
{
    // XML doesn't match schema
    Console.WriteLine($"Template error: {ex.Message}");
}
catch (ArgumentNullException ex)
{
    // Required data is null
    Console.WriteLine($"Missing data: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    // Image file not found (if template references images)
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error generating receipt: {ex.Message}");
}
```

## See Also

- [ReceiptXmlParser.md](ReceiptXmlParser.md) - Full XML element reference
- [EscPos.Commands/ReceiptXmlExample.cs](EscPos.Commands/ReceiptXmlExample.cs) - Complete code examples

