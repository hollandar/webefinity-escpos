# Conditional Rendering in Receipt XML Templates

The `<if>` element allows you to conditionally include content in your receipt templates based on boolean expressions.

## Basic Syntax

```xml
<if condition="variableName">
  <!-- Content to render if condition is true -->
</if>
```

**Attributes:**
- `condition`: The path to a variable that will be evaluated as a boolean (required)
- `not`: Negates the condition when present. Can be used as:
  - `not=""` - negates the condition (empty value)
  - `not="true"` - negates the condition
  - `not="false"` - does NOT negate (regular condition)
  - If omitted - does NOT negate (regular condition)
  - Any other value throws an `ArgumentException`

## Simple Examples

### Boolean Condition

```xml
<if condition="HasDiscount">
  <line>** 10% DISCOUNT APPLIED **</line>
</if>
```

```csharp
var data = new { HasDiscount = true };
byte[] commands = ReceiptXmlParser.Parse(xml, data);
```

### Nested Properties

```xml
<if condition="Customer.IsMember">
  <line>Loyalty Points: ${Customer.Points}</line>
</if>
```

```csharp
var data = new
{
    Customer = new
    {
        IsMember = true,
        Points = 150
    }
};
```

## Negated Conditions

Use the `not` attribute to invert a condition. This is useful for "if not" scenarios. The `not` attribute can be used in three ways:

1. **Attribute without value**: `<if condition="..." not="">`
2. **Attribute with "true" value**: `<if condition="..." not="true">`
3. **Attribute with "false" value**: `<if condition="..." not="false">` (does NOT negate)

### Basic Negation (with empty value)

```xml
<if condition="HasDiscount" not="">
  <line>No discount available</line>
</if>
```

```csharp
var data = new { HasDiscount = false };
// Renders "No discount available" because HasDiscount is false and we negate it to true
```

### Basic Negation (with "true" value)

```xml
<if condition="HasDiscount" not="true">
  <line>No discount available</line>
</if>
```

```csharp
var data = new { HasDiscount = false };
// Renders "No discount available" because HasDiscount is false and we negate it to true
```

### Explicitly Disabling Negation

```xml
<if condition="HasDiscount" not="false">
  <line>Discount available</line>
</if>
```

```csharp
var data = new { HasDiscount = true };
// Renders "Discount available" because HasDiscount is true and not="false" means no negation
```

### Negating Null or Empty Values

```xml
<if condition="Message" not="">
  <line>No message provided</line>
</if>
```

```csharp
var data = new { Message = (string?)null };
// Renders "No message provided" because Message is null (false) and negated to true
```

### Negating Numeric Values

```xml
<if condition="ItemCount" not="">
  <line>Cart is empty</line>
</if>
```

```csharp
var data = new { ItemCount = 0 };
// Renders "Cart is empty" because ItemCount is 0 (false) and negated to true
```

## Condition Evaluation Rules

The condition is evaluated using the following rules:

### Boolean Values
```csharp
new { HasDiscount = true }  // Renders content
new { HasDiscount = false } // Skips content
```

### String Values
```csharp
new { Status = "true" }  // Renders (string "true")
new { Status = "false" } // Skips (string "false")
new { Message = "Hello" } // Renders (non-empty string)
new { Message = "" }      // Skips (empty string)
new { Message = "   " }   // Skips (whitespace only)
```

### Numeric Values
```csharp
new { Count = 5 }    // Renders (non-zero)
new { Count = 0 }    // Skips (zero)
new { Amount = 1.5 } // Renders (non-zero)
new { Amount = 0.0 } // Skips (zero)
```

### Null Values
```csharp
new { Value = (string?)null } // Skips (null)
new { Missing properties }    // Skips (variable not found)
```

### Other Objects
```csharp
new { Data = someObject } // Renders (any non-null object)
```

## Complex Examples

### Conditional Discount

```xml
<?xml version="1.0"?>
<receipt xmlns="http://webefinity.com/escpos/receipt">
  <line>Order Total: $${Total}</line>
  
  <if condition="HasDiscount">
    <line>Discount Applied: -$${DiscountAmount}</line>
    <line>Final Total: $${FinalTotal}</line>
  </if>
  
  <!-- Show message when no discount (using negation with empty value) -->
  <if condition="HasDiscount" not="">
    <line>No discounts applied</line>
  </if>
</receipt>
```

### Loyalty Program

```xml
<if condition="Customer.IsMember">
  <align value="center">
    <line>LOYALTY MEMBER</line>
    <line>Points Earned: ${Customer.PointsEarned}</line>
    <line>Total Points: ${Customer.TotalPoints}</line>
  </align>
  <line>================================</line>
</if>
```

### Special Offers

```xml
<if condition="Promotion.Active">
  <align value="center">
    <bold>
      <line>** SPECIAL OFFER **</line>
      <line>${Promotion.Message}</line>
    </bold>
  </align>
</if>
```

## Nested Conditions

You can nest `<if>` conditions for more complex logic:

```xml
<if condition="Order.HasDiscount">
  <line>Discount Applied!</line>
  
  <if condition="Order.DiscountType">
    <line>Type: ${Order.DiscountType}</line>
  </if>
</if>
```

## Combining with For Loops

Conditional rendering works inside `<for>` loops:

```xml
<for var="item" in="Order.Items">
  <text>${item.Quantity}x ${item.Name}</text>
  <align value="right">
    <line>$${item.Price}</line>
  </align>
  
  <!-- Show sale indicator only for sale items -->
  <if condition="item.OnSale">
    <line>  ** ON SALE **</line>
  </if>
  
  <!-- Show modifiers if any -->
  <if condition="item.HasModifiers">
    <for var="mod" in="item.Modifiers">
      <line>  + ${mod.Name}</line>
    </for>
  </if>
</for>
```

## Real-World Example

```xml
<?xml version="1.0"?>
<receipt xmlns="http://webefinity.com/escpos/receipt">
  <initialize/>
  
  <!-- Header -->
  <align value="center">
    <bold>
      <size width="2" height="2">
        <line>${Store.Name}</line>
      </size>
    </bold>
  </align>
  
  <line>================================</line>
  
  <!-- Order Details -->
  <line>Order #${Order.Number}</line>
  <line>Date: ${Order.Date}</line>
  
  <!-- Show table number if dine-in -->
  <if condition="Order.IsDineIn">
    <line>Table: ${Order.TableNumber}</line>
  </if>
  
  <!-- Show delivery address if delivery -->
  <if condition="Order.IsDelivery">
    <line>Deliver to:</line>
    <line>${Order.DeliveryAddress}</line>
  </if>
  
  <line>================================</line>
  
  <!-- Order Items -->
  <for var="item" in="Order.Items">
    <text>${item.Quantity}x ${item.Name}</text>
    <align value="right">
      <line>$${item.Price}</line>
    </align>
    
    <!-- Show special instructions if any -->
    <if condition="item.HasSpecialInstructions">
      <line>  Note: ${item.SpecialInstructions}</line>
    </if>
  </for>
  
  <line>--------------------------------</line>
  
  <!-- Totals -->
  <align value="right">
    <line>Subtotal:    $${Order.Subtotal}</line>
    
    <!-- Show discount if applied -->
    <if condition="Order.HasDiscount">
      <line>Discount:    -$${Order.DiscountAmount}</line>
    </if>
    
    <!-- Show delivery fee if delivery order -->
    <if condition="Order.IsDelivery">
      <line>Delivery:    $${Order.DeliveryFee}</line>
    </if>
    
    <line>Tax:         $${Order.Tax}</line>
    
    <bold>
      <line>Total:       $${Order.Total}</line>
    </bold>
  </align>
  
  <line>================================</line>
  
  <!-- Loyalty Program Info -->
  <if condition="Customer.IsMember">
    <align value="center">
      <line>Loyalty Points Earned: ${Customer.PointsEarned}</line>
      <line>Total Points: ${Customer.TotalPoints}</line>
      
      <!-- Show reward if available -->
      <if condition="Customer.HasAvailableReward">
        <line>Reward Available: ${Customer.RewardMessage}</line>
      </if>
    </align>
    <line>================================</line>
  </if>
  
  <!-- Promotional Offers -->
  <if condition="Promotion.Active">
    <align value="center">
      <bold>
        <line>SPECIAL OFFER!</line>
        <line>${Promotion.Message}</line>
      </bold>
      
      <!-- Show promo code if applicable -->
      <if condition="Promotion.HasCode">
        <line>Use code: ${Promotion.Code}</line>
      </if>
    </align>
    <line>================================</line>
  </if>
  
  <!-- QR Code -->
  <if condition="Order.HasDigitalReceipt">
    <align value="center">
      <qrcode data="${Order.ReceiptUrl}" size="4" errorLevel="M"/>
      <line>Scan for digital receipt</line>
    </align>
    <line>================================</line>
  </if>
  
  <!-- Footer -->
  <align value="center">
    <line>Thank you for your order!</line>
    
    <!-- Show custom message if set -->
    <if condition="Store.HasCustomMessage">
      <line>${Store.CustomMessage}</line>
    </if>
  </align>
  
  <feed lines="3"/>
  <cut type="partial"/>
</receipt>
```

## Data Model Example

```csharp
var receiptData = new
{
    Store = new
    {
        Name = "RESTAURANT",
        HasCustomMessage = true,
        CustomMessage = "Visit us again soon!"
    },
    Customer = new
    {
        IsMember = true,
        PointsEarned = 50,
        TotalPoints = 650,
        HasAvailableReward = true,
        RewardMessage = "Free Appetizer!"
    },
    Order = new
    {
        Number = "1234",
        Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
        IsDineIn = true,
        IsDelivery = false,
        TableNumber = "5",
        HasDiscount = true,
        DiscountAmount = "2.50",
        Items = new object[]
        {
            new
            {
                Quantity = 1,
                Name = "Burger",
                Price = "12.99",
                HasSpecialInstructions = true,
                SpecialInstructions = "No pickles"
            },
            new
            {
                Quantity = 2,
                Name = "Fries",
                Price = "6.98",
                HasSpecialInstructions = false
            }
        },
        Subtotal = "19.97",
        Tax = "1.60",
        Total = "19.07",
        HasDigitalReceipt = true,
        ReceiptUrl = "https://restaurant.com/receipt/1234"
    },
    Promotion = new
    {
        Active = true,
        Message = "Buy 2 mains, get 1 dessert free!",
        HasCode = true,
        Code = "DESSERT2024"
    }
};

byte[] commands = ReceiptXmlParser.Parse(xml, receiptData);
```

## Best Practices

### 1. Use Descriptive Condition Names

```xml
<!-- Good -->
<if condition="HasDiscount">
<if condition="Customer.IsMember">
<if condition="Order.RequiresSignature">

<!-- Less clear -->
<if condition="Flag1">
<if condition="Status">
<if condition="Check">
```

### 2. Pre-compute Complex Conditions

```csharp
// In your code, compute complex conditions
var receiptData = new
{
    // Simple boolean flags
    ShowPromotion = (order.Total > 50 && customer.IsMember),
    RequireSignature = (order.PaymentMethod == "Credit" && order.Total > 100),
    CanEarnPoints = (customer.IsMember && !order.UsedPoints)
};
```

### 3. Provide Fallbacks for Optional Data

```csharp
var data = new
{
    // Ensure boolean properties are always present
    HasDiscount = order.DiscountAmount > 0,
    IsMember = customer?.IsMember ?? false,
    ShowPromotion = promotion?.IsActive ?? false
};
```

### 4. Keep Nested Conditions Shallow

```xml
<!-- Prefer this -->
<if condition="CanShowOffer">
  <line>${Offer.Message}</line>
</if>

<!-- Over this -->
<if condition="HasOffers">
  <if condition="Offers.Length">
    <if condition="Offers.Active">
      <line>Offer</line>
    </if>
  </if>
</if>
```

### 5. Combine Related Conditions

```csharp
// Combine related logic in your data model
var data = new
{
    ShowLoyaltySection = customer.IsMember && customer.TotalPoints > 0,
    ShowDeliveryInfo = order.IsDelivery && !string.IsNullOrEmpty(order.Address)
};
```

## Error Handling

### Missing Template Context

```csharp
// This will throw an exception
byte[] commands = ReceiptXmlParser.Parse(xmlWithIf); // No data provided
```

**Error:** `InvalidOperationException: If conditions require a template context.`

**Solution:** Always provide data when using `<if>`:
```csharp
byte[] commands = ReceiptXmlParser.Parse(xmlWithIf, data);
```

### Missing Condition Variable

If the condition variable doesn't exist, the content is simply skipped (same as if it were false):

```xml
<if condition="NonExistentVariable">
  <line>This won't render</line>
</if>
```

No error is thrown - this allows for flexible templates.

## Performance Tips

1. **Avoid excessive nesting** - Each `<if>` adds a small overhead
2. **Pre-compute conditions** in your application code rather than checking multiple related properties
3. **Use meaningful variable names** to make templates self-documenting

## See Also

- [ReceiptXmlParser.md](ReceiptXmlParser.md) - Complete XML template guide
- [ReceiptTemplateVariables.md](ReceiptTemplateVariables.md) - Variable substitution
- [ReceiptLoops.md](ReceiptLoops.md) - For loop iteration
- [EscPos.Commands/ReceiptXmlExample.cs](EscPos.Commands/ReceiptXmlExample.cs) - Code examples
