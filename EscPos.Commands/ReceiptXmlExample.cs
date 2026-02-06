using System;
using System.IO;
using System.Text;
using EscPos.Commands;

namespace EscPos.Examples;

public static class ReceiptXmlExample
{
    public static void PrintReceiptFromXml()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <size width=""2"" height=""2"">
      <bold>
        <line>RESTAURANT NAME</line>
      </bold>
    </size>
    <line>123 Main Street</line>
    <line>City, State 12345</line>
    <line>Phone: (555) 123-4567</line>
  </align>
  
  <line>================================</line>
  
  <align value=""left"">
    <bold>
      <text>Order #1234</text>
    </bold>
    <line> - Table 5</line>
    <line>Date: 2024-02-04 14:30</line>
  </align>
  
  <line>================================</line>
  
  <line>1x Burger.................$12.99</line>
  <line>2x Fries..................$6.98</line>
  <line>1x Soda...................$2.50</line>
  
  <line>--------------------------------</line>
  
  <align value=""right"">
    <line>Subtotal:      $22.47</line>
    <line>Tax (10%):      $2.25</line>
    <bold>
      <size width=""2"" height=""2"">
        <line>Total:      $24.72</line>
      </size>
    </bold>
  </align>
  
  <line>================================</line>
  
  <align value=""center"">
    <line/>
    <qrcode data=""https://restaurant.com/order/1234"" size=""4"" errorLevel=""M""/>
    <line/>
    <line>Scan for receipt</line>
    <line/>
  </align>
  
  <align value=""center"">
    <line>Thank you for your order!</line>
    <line>Visit us again soon</line>
  </align>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

        try
        {
            var escposData = ReceiptXmlParser.Parse(xml, validate: true);
            
            System.Console.WriteLine($"Generated {escposData.Length} bytes of ESC/POS commands");
            System.Console.WriteLine("Ready to send to printer");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void PrintReceiptWithTemplateVariables()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <size width=""2"" height=""2"">
      <bold>
        <line>${StoreName}</line>
      </bold>
    </size>
    <line>${Store.Address}</line>
    <line>${Store.City}, ${Store.State} ${Store.Zip}</line>
    <line>Phone: ${Store.Phone}</line>
  </align>
  
  <line>================================</line>
  
  <align value=""left"">
    <bold>
      <text>Order #${Order.Number}</text>
    </bold>
    <line> - Table ${Order.Table}</line>
    <line>Date: ${Order.Date}</line>
  </align>
  
  <line>================================</line>
  
  <line>${Item1.Quantity}x ${Item1.Name}...$${Item1.Price}</line>
  <line>${Item2.Quantity}x ${Item2.Name}...$${Item2.Price}</line>
  <line>${Item3.Quantity}x ${Item3.Name}...$${Item3.Price}</line>
  
  <line>--------------------------------</line>
  
  <align value=""right"">
    <line>Subtotal:      $${Order.Subtotal}</line>
    <line>Tax (${Order.TaxRate}%):      $${Order.Tax}</line>
    <bold>
      <size width=""2"" height=""2"">
        <line>Total:      $${Order.Total}</line>
      </size>
    </bold>
  </align>
  
  <line>================================</line>
  
  <align value=""center"">
    <line/>
    <qrcode data=""${Order.Url}"" size=""4"" errorLevel=""M""/>
    <line/>
    <line>Scan for receipt</line>
    <line/>
  </align>
  
  <align value=""center"">
    <line>Thank you for your order!</line>
    <line>Visit us again soon</line>
  </align>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

        var templateData = new
        {
            StoreName = "RESTAURANT NAME",
            Store = new
            {
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
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                Subtotal = "22.47",
                TaxRate = "10",
                Tax = "2.25",
                Total = "24.72",
                Url = "https://restaurant.com/order/1234"
            },
            Item1 = new { Quantity = 1, Name = "Burger", Price = "12.99" },
            Item2 = new { Quantity = 2, Name = "Fries", Price = "6.98" },
            Item3 = new { Quantity = 1, Name = "Soda", Price = "2.50" }
        };

        try
        {
            var escposData = ReceiptXmlParser.Parse(xml, templateData, validate: true);
            
            System.Console.WriteLine($"Generated {escposData.Length} bytes of ESC/POS commands");
            System.Console.WriteLine("Ready to send to printer");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void PrintReceiptWithDictionary()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <bold>
      <size width=""2"" height=""2"">
        <line>${Title}</line>
      </size>
    </bold>
  </align>
  
  <line>Order #${OrderNumber}</line>
  <line>Total: $${Total}</line>
  
  <qrcode data=""${QrUrl}"" size=""4""/>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

        var context = new ReceiptTemplateContext();
        context.Add("Title", "MY STORE");
        context.Add("OrderNumber", "5678");
        context.Add("Total", "99.99");
        context.Add("QrUrl", "https://store.com/order/5678");

        try
        {
            var escposData = ReceiptXmlParser.Parse(xml, context, validate: true);
            
            System.Console.WriteLine($"Generated {escposData.Length} bytes of ESC/POS commands");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void PrintReceiptFromFile(string xmlFilePath)
    {
        try
        {
            var escposData = ReceiptXmlParser.ParseFile(xmlFilePath, validate: true);
            
            System.Console.WriteLine($"Generated {escposData.Length} bytes from {xmlFilePath}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void PrintReceiptWithLoops()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <size width=""2"" height=""2"">
      <bold>
        <line>${Store.Name}</line>
      </bold>
    </size>
    <line>${Store.Address}</line>
    <line>${Store.City}, ${Store.State}</line>
  </align>
  
  <line>================================</line>
  
  <bold><text>Order #${Order.Number}</text></bold>
  <line> - Table ${Order.Table}</line>
  <line>Date: ${Order.Date}</line>
  
  <line>================================</line>
  
  <!-- Loop through order items -->
  <for var=""item"" in=""Order.Items"">
    <text>${item.Quantity}x ${item.Name}</text>
    <align value=""right"">
      <line>$${item.Price}</line>
    </align>
    <for var=""mod"" in=""item.Modifiers"">
      <line>  - ${mod.Name}</line>
    </for>
  </for>
  
  <line>--------------------------------</line>
  
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
  
  <align value=""center"">
    <qrcode data=""${Order.Url}"" size=""4"" errorLevel=""M""/>
    <line>Scan for receipt</line>
  </align>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

        var orderData = new
        {
            Store = new
            {
                Name = "RESTAURANT",
                Address = "123 Main St",
                City = "Anytown",
                State = "CA"
            },
            Order = new
            {
                Number = "1234",
                Table = "5",
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
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
                    },
                    new
                    {
                        Quantity = 1,
                        Name = "Soda",
                        Price = "2.50",
                        Modifiers = Array.Empty<object>()
                    }
                },
                Subtotal = "22.47",
                Tax = "2.25",
                Total = "24.72",
                Url = "https://restaurant.com/order/1234"
            }
        };

        try
        {
            var escposData = ReceiptXmlParser.Parse(xml, orderData, validate: true);
            
            System.Console.WriteLine($"Generated {escposData.Length} bytes with loops");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void CreateSampleReceiptTemplate()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <!-- Header -->
  <align value=""center"">
    <size width=""2"" height=""2"">
      <bold><line>${StoreName}</line></bold>
    </size>
    <line>${StoreAddress}</line>
    <line>${StoreCity}</line>
    <line>Phone: ${StorePhone}</line>
  </align>
  
  <line>================================</line>
  
  <!-- Order Info -->
  <bold>
    <text>Order #${OrderNumber}</text>
  </bold>
  <line> - ${TableNumber}</line>
  <line>Date: ${OrderDate}</line>
  
  <line>================================</line>
  
  <!-- NOTE: In a real application, you would loop through items programmatically
       before passing to the parser, or use a template engine like Liquid or Scriban -->
  
  <line>--------------------------------</line>
  
  <!-- Totals -->
  <align value=""right"">
    <line>Subtotal:      $${Subtotal}</line>
    <line>Tax (${TaxRate}%):      $${Tax}</line>
    <bold>
      <size width=""2"" height=""2"">
        <line>Total:      $${Total}</line>
      </size>
    </bold>
  </align>
  
  <line>================================</line>
  
  <!-- QR Code -->
  <align value=""center"">
    <line/>
    <qrcode data=""${OrderUrl}"" size=""4"" errorLevel=""M""/>
    <line/>
    <line>Scan for receipt</line>
  </align>
  
  <!-- Footer -->
  <align value=""center"">
    <line/>
    <line>Thank you for your order!</line>
    <line>Visit us again soon</line>
  </align>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

        File.WriteAllText("receipt_template.xml", xml);
        System.Console.WriteLine("Template saved to receipt_template.xml");
    }

    public static void PrintReceiptWithConditionals()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<receipt xmlns=""http://webefinity.com/escpos/receipt"">
  <initialize/>
  
  <align value=""center"">
    <bold>
      <size width=""2"" height=""2"">
        <line>${Store.Name}</line>
      </size>
    </bold>
  </align>
  
  <line>================================</line>
  <line>Order #${Order.Number}</line>
  
  <!-- Conditional: Show discount if applied -->
  <if condition=""Order.HasDiscount"">
    <line>** 10% DISCOUNT APPLIED **</line>
  </if>
  
  <for var=""item"" in=""Order.Items"">
    <text>${item.Quantity}x ${item.Name}</text>
    <align value=""right"">
      <line>$${item.Price}</line>
    </align>
    
    <!-- Show sale indicator for items on sale -->
    <if condition=""item.OnSale"">
      <line>  (SALE ITEM!)</line>
    </if>
  </for>
  
  <line>--------------------------------</line>
  
  <align value=""right"">
    <line>Subtotal:    $${Order.Subtotal}</line>
    
    <!-- Only show discount line if discount was applied -->
    <if condition=""Order.HasDiscount"">
      <line>Discount:    -$${Order.DiscountAmount}</line>
    </if>
    
    <line>Tax:         $${Order.Tax}</line>
    <bold><line>Total:       $${Order.Total}</line></bold>
  </align>
  
  <line>================================</line>
  
  <!-- Conditional: Show loyalty points if customer is a member -->
  <if condition=""Customer.IsMember"">
    <align value=""center"">
      <line>Loyalty Points Earned: ${Customer.PointsEarned}</line>
      <line>Total Points: ${Customer.TotalPoints}</line>
    </align>
    <line>================================</line>
  </if>
  
  <!-- Conditional: Show special offer if available -->
  <if condition=""Promotion.HasOffer"">
    <align value=""center"">
      <bold>
        <line>SPECIAL OFFER!</line>
        <line>${Promotion.Message}</line>
      </bold>
    </align>
    <line>================================</line>
  </if>
  
  <align value=""center"">
    <line>Thank you for your order!</line>
  </align>
  
  <feed lines=""3""/>
  <cut type=""partial""/>
</receipt>";

        var orderData = new
        {
            Store = new { Name = "CAFE" },
            Order = new
            {
                Number = "1234",
                HasDiscount = true,
                DiscountAmount = "2.50",
                Items = new object[]
                {
                    new { Quantity = 1, Name = "Coffee", Price = "3.50", OnSale = true },
                    new { Quantity = 1, Name = "Muffin", Price = "2.50", OnSale = false }
                },
                Subtotal = "6.00",
                Tax = "0.48",
                Total = "3.98"
            },
            Customer = new
            {
                IsMember = true,
                PointsEarned = 40,
                TotalPoints = 540
            },
            Promotion = new
            {
                HasOffer = true,
                Message = "Buy 2 coffees, get 1 free!"
            }
        };

        try
        {
            var escposData = ReceiptXmlParser.Parse(xml, orderData, validate: true);
            
            System.Console.WriteLine($"Generated {escposData.Length} bytes with conditionals");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
