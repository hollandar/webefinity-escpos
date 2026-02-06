# For Loops in Receipt XML Templates

The `<for>` element allows you to iterate over collections and arrays in your receipt templates.

## Basic Syntax

```xml
<for var="variableName" in="path.to.collection">
  <!-- Commands that use ${variableName} -->
</for>
```

**Attributes:**
- `var`: The variable name to use for each item in the collection (required)
- `in`: The path to the collection in your template data (required)

## Simple Example

```xml
<?xml version="1.0" encoding="utf-8"?>
<receipt xmlns="http://webefinity.com/escpos/receipt">
  <line>Order Items:</line>
  
  <for var="item" in="Items">
    <line>${item.Quantity}x ${item.Name} - $${item.Price}</line>
  </for>
  
  <cut type="partial"/>
</receipt>
```

```csharp
var data = new
{
    Items = new[]
    {
        new { Quantity = 1, Name = "Burger", Price = "12.99" },
        new { Quantity = 2, Name = "Fries", Price = "6.98" },
        new { Quantity = 1, Name = "Soda", Price = "2.50" }
    }
};

byte[] commands = ReceiptXmlParser.Parse(xml, data);
```

**Output:**
```
Order Items:
1x Burger - $12.99
2x Fries - $6.98
1x Soda - $2.50
```

## Nested Loops

You can nest `<for>` loops to iterate over multi-level collections:

```xml
<for var="category" in="Menu.Categories">
  <bold><line>${category.Name}</line></bold>
  
  <for var="item" in="category.Items">
    <line>  ${item.Name} - $${item.Price}</line>
  </for>
  
  <line/>
</for>
```

```csharp
var data = new
{
    Menu = new
    {
        Categories = new object[]
        {
            new
            {
                Name = "Burgers",
                Items = new object[]
                {
                    new { Name = "Classic Burger", Price = "9.99" },
                    new { Name = "Cheese Burger", Price = "10.99" }
                }
            },
            new
            {
                Name = "Sides",
                Items = new object[]
                {
                    new { Name = "Fries", Price = "3.49" },
                    new { Name = "Onion Rings", Price = "3.99" }
                }
            }
        }
    }
};
```

**Output:**
```
Burgers
  Classic Burger - $9.99
  Cheese Burger - $10.99

Sides
  Fries - $3.49
  Onion Rings - $3.99
```

## Complex Example: Order Receipt with Modifiers

```xml
<?xml version="1.0" encoding="utf-8"?>
<receipt xmlns="http://webefinity.com/escpos/receipt">
  <initialize/>
  
  <align value="center">
    <bold>
      <size width="2" height="2">
        <line>${Store.Name}</line>
      </size>
    </bold>
  </align>
  
  <line>================================</line>
  
  <bold><text>Order #${Order.Number}</text></bold>
  <line> - Table ${Order.Table}</line>
  
  <line>================================</line>
  
  <!-- Loop through order items -->
  <for var="item" in="Order.Items">
    <text>${item.Quantity}x ${item.Name}</text>
    <align value="right">
      <line>$${item.Price}</line>
    </align>
    
    <!-- Nested loop for item modifiers -->
    <for var="mod" in="item.Modifiers">
      <line>  - ${mod.Name}</line>
    </for>
  </for>
  
  <line>--------------------------------</line>
  
  <align value="right">
    <line>Subtotal:    $${Order.Subtotal}</line>
    <line>Tax:         $${Order.Tax}</line>
    <bold>
      <line>Total:       $${Order.Total}</line>
    </bold>
  </align>
  
  <feed lines="3"/>
  <cut type="partial"/>
</receipt>
```

```csharp
var orderData = new
{
    Store = new { Name = "RESTAURANT" },
    Order = new
    {
        Number = "1234",
        Table = "5",
        Items = new object[]
        {
            new
            {
                Quantity = 1,
                Name = "Burger",
                Price = "12.99",
                Modifiers = new object[]
                {
                    new { Name = "Extra Cheese" },
                    new { Name = "No Onions" }
                }
            },
            new
            {
                Quantity = 2,
                Name = "Fries",
                Price = "6.98",
                Modifiers = Array.Empty<object>()
            }
        },
        Subtotal = "19.97",
        Tax = "2.00",
        Total = "21.97"
    }
};

byte[] commands = ReceiptXmlParser.Parse(xml, orderData);
```

**Output:**
```
        RESTAURANT
================================
Order #1234 - Table 5
================================
1x Burger                $12.99
  - Extra Cheese
  - No Onions
2x Fries                  $6.98
--------------------------------
         Subtotal:    $19.97
         Tax:          $2.00
         Total:       $21.97
```

## Formatting Within Loops

All text formatting commands work within loops:

```xml
<for var="item" in="Items">
  <bold>
    <size width="2" height="1">
      <line>${item.Name}</line>
    </size>
  </bold>
  <line>Price: $${item.Price}</line>
  <line/>
</for>
```

## Loops with Alignment

```xml
<for var="item" in="Items">
  <align value="left">
    <text>${item.Quantity}x ${item.Name}</text>
  </align>
  <align value="right">
    <line>$${item.Price}</line>
  </align>
</for>
```

## Empty Collections

If the collection is empty or null, the loop body is simply skipped:

```xml
<line>Items:</line>
<for var="item" in="Items">
  <line>${item.Name}</line>
</for>
<line>End of items</line>
```

If `Items` is empty:
```
Items:
End of items
```

## Working with Different Collection Types

The `<for>` element works with any type that implements `IEnumerable`:

### Arrays
```csharp
var data = new { Items = new[] { "Item1", "Item2", "Item3" } };
```

### Lists
```csharp
var data = new { Items = new List<string> { "Item1", "Item2" } };
```

### Collections
```csharp
var data = new { Items = new Collection<Product> { /* ... */ } };
```

### LINQ Results
```csharp
var data = new 
{ 
    Items = dbContext.Products
        .Where(p => p.InStock)
        .Select(p => new { p.Name, p.Price })
        .ToList()
};
```

## Variable Scoping

Loop variables are scoped to the loop body and don't affect outer variables:

```xml
<line>Outer: ${item}</line>

<for var="item" in="Items">
  <line>Loop: ${item.Name}</line>
</for>

<line>Outer: ${item}</line>
```

The outer `${item}` references remain unchanged by the loop.

## Accessing Loop Item Properties

You can access nested properties on loop items:

```xml
<for var="order" in="Orders">
  <line>Order #${order.Id}</line>
  <line>Customer: ${order.Customer.Name}</line>
  <line>Total: $${order.Total}</line>
  <line/>
</for>
```

## Advanced: Conditional Logic with Empty Collections

While there's no `if` statement, you can use empty collections to conditionally include content:

```csharp
var data = new
{
    // Only include special offer if applicable
    SpecialOffers = hasSpecialOffer 
        ? new[] { new { Message = "10% off next order!" } }
        : Array.Empty<object>()
};
```

```xml
<for var="offer" in="SpecialOffers">
  <align value="center">
    <bold><line>${offer.Message}</line></bold>
  </align>
</for>
```

## Best Practices

### 1. Keep Loop Bodies Simple

```xml
<!-- Good: Simple and readable -->
<for var="item" in="Items">
  <line>${item.Quantity}x ${item.Name} - $${item.Price}</line>
</for>

<!-- Avoid: Too complex -->
<for var="item" in="Items">
  <size width="1" height="1">
    <bold>
      <underline>
        <align value="center">
          <line>${item.Name}</line>
        </align>
      </underline>
    </bold>
  </size>
</for>
```

### 2. Pre-format Complex Data

```csharp
// Format data before passing to template
var data = new
{
    Items = orders.Select(o => new
    {
        // Format the line as needed
        DisplayLine = $"{o.Quantity}x {o.Name.PadRight(20)} ${o.Price:F2}"
    })
};
```

```xml
<for var="item" in="Items">
  <line>${item.DisplayLine}</line>
</for>
```

### 3. Use Meaningful Variable Names

```xml
<!-- Good -->
<for var="product" in="Products">
  <line>${product.Name}</line>
</for>

<!-- Less clear -->
<for var="p" in="Products">
  <line>${p.Name}</line>
</for>
```

### 4. Handle Empty Collections Gracefully

```xml
<line>Your Order:</line>
<for var="item" in="Order.Items">
  <line>${item.Name}</line>
</for>
<!-- Add empty line check in your code if needed -->
```

## Error Handling

### Collection Not Found
If the path doesn't resolve to a value, the loop is skipped silently.

### Not Enumerable
If the value at the path is not `IEnumerable`, an exception is thrown:
```
InvalidOperationException: The value at 'Items' is not enumerable.
```

### Missing Template Context
Loops require a template context:
```csharp
// This will throw an exception:
byte[] commands = ReceiptXmlParser.Parse(xmlWithLoops); // No data provided

// Correct:
byte[] commands = ReceiptXmlParser.Parse(xmlWithLoops, data);
```

## Performance Considerations

- Loops are processed at parse time, not print time
- Large collections (1000+ items) may take time to process
- Consider paginating very large datasets before rendering

## See Also

- [ReceiptTemplateVariables.md](ReceiptTemplateVariables.md) - Template variable syntax
- [ReceiptXmlParser.md](ReceiptXmlParser.md) - Full XML element reference
- [EscPos.Commands/ReceiptXmlExample.cs](EscPos.Commands/ReceiptXmlExample.cs) - Complete code examples

