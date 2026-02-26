// services/order/src/Order.cs

namespace Ecommerce.Services.Order;

public sealed class Order
{
    public double Subtotal { get; set; }
    public double DiscountedSubtotal { get; set; }
    public double Tax { get; set; }
    public double Shipping { get; set; }
    public double FinalTotal { get; set; }

    public string? Status { get; set; }

    public bool IsPaid() => string.Equals("PAID", Status, System.StringComparison.OrdinalIgnoreCase);
}
