// services/cart/src/CartValidator.cs

using System;

namespace Ecommerce.Services.Cart;

public sealed class CartValidator
{
    public void ValidateAddItem(CartItem item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item), "item must not be null");
        RequireNonBlank(item.Sku, "sku must not be blank");
        RequirePositive(item.Quantity, "quantity must be > 0");
        RequireNonNegative(item.UnitPriceCents, "unitPriceCents must be >= 0");
    }

    public void ValidateUpdateQuantity(string sku, int newQuantity)
    {
        RequireNonBlank(sku, "sku must not be blank");
        if (newQuantity < 0)
        {
            // Note: original Java message says "> 0" while allowing 0; preserved as-is.
            throw new ValidationException("newQuantity must be > 0");
        }
    }

    public void ValidateRemoveItem(string sku)
    {
        RequireNonBlank(sku, "sku must not be blank");
    }

    private static void RequireNonBlank(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(message);
        }
    }

    private static void RequirePositive(int value, string message)
    {
        if (value <= 0)
        {
            throw new ValidationException(message);
        }
    }

    private static void RequireNonNegative(long value, string message)
    {
        if (value < 0)
        {
            throw new ValidationException(message);
        }
    }

    // ---- Minimal supporting types ----

    public sealed class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public sealed class CartItem
    {
        public CartItem(string sku, int quantity, long unitPriceCents)
        {
            Sku = sku;
            Quantity = quantity;
            UnitPriceCents = unitPriceCents;
        }

        public string Sku { get; }
        public int Quantity { get; }
        public long UnitPriceCents { get; }
    }
}
