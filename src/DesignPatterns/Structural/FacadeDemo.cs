using System.Globalization;

namespace DesignPatterns.Structural;

/// <summary>
/// Facade: a single simplified entry point over several subsystems that would otherwise each
/// need their own multi-step calls in the right order -- here, placing an order without the
/// caller juggling inventory, payment, and shipping calls itself.
/// </summary>
public static class FacadeDemo
{
    /// <summary>Runs the demo: one call through the facade drives all three subsystems.</summary>
    public static Task RunAsync()
    {
        var facade = new OrderFacade(new InventoryService(), new PaymentService(), new ShippingService());
        var result = facade.PlaceOrder("widget-42", quantity: 3, amount: 59.97m);

        Console.WriteLine(result);

        return Task.CompletedTask;
    }
}

file sealed class InventoryService
{
    public bool Reserve(string sku, int quantity)
    {
        Console.WriteLine($"Inventory: reserved {quantity}x {sku}");
        return true;
    }
}

file sealed class PaymentService
{
    public bool Charge(decimal amount)
    {
        // Explicit en-US, not CultureInfo.InvariantCulture: invariant's currency format uses
        // the generic "¤" symbol rather than "$", which would make this demo's output
        // deterministic but not obviously read as money.
        Console.WriteLine($"Payment: charged {amount.ToString("C", CultureInfo.GetCultureInfo("en-US"))}");
        return true;
    }
}

file sealed class ShippingService
{
    public string Schedule(string sku, int quantity)
    {
        var trackingNumber = $"TRK-{sku}-{quantity}";
        Console.WriteLine($"Shipping: scheduled {quantity}x {sku}, tracking {trackingNumber}");
        return trackingNumber;
    }
}

file sealed class OrderFacade(InventoryService inventory, PaymentService payment, ShippingService shipping)
{
    public string PlaceOrder(string sku, int quantity, decimal amount)
    {
        if (!inventory.Reserve(sku, quantity))
        {
            return "Order failed: insufficient inventory";
        }

        if (!payment.Charge(amount))
        {
            return "Order failed: payment declined";
        }

        var tracking = shipping.Schedule(sku, quantity);
        return $"Order placed. Tracking: {tracking}";
    }
}

/* Expected output
Inventory: reserved 3x widget-42
Payment: charged $59.97
Shipping: scheduled 3x widget-42, tracking TRK-widget-42-3
Order placed. Tracking: TRK-widget-42-3
*/
