// services/cart/src/CartService.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ecommerce.Services.Cart;

public sealed class CartService
{
    private readonly CartValidator _validator;

    // In-memory cart store: cartId -> cart
    private readonly Dictionary<string, Cart> _carts = new(StringComparer.Ordinal);

    public CartService(CartValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator), "validator must not be null");
    }

    public Cart GetOrCreateCart(string cartId)
    {
        RequireNonBlank(cartId, "cartId must not be blank");

        if (!_carts.TryGetValue(cartId, out var cart))
        {
            cart = new Cart(cartId);
            _carts[cartId] = cart;
        }

        return cart;
    }

    public Cart AddItem(string cartId, CartValidator.CartItem item)
    {
        _validator.ValidateAddItem(item);

        var cart = GetOrCreateCart(cartId);
        cart.AddOrIncrement(item.Sku, item.Quantity, item.UnitPriceCents);
        return cart;
    }

    public Cart UpdateQuantity(string cartId, string sku, int newQuantity)
    {
        _validator.ValidateUpdateQuantity(sku, newQuantity);

        var cart = GetOrCreateCart(cartId);
        cart.SetQuantity(sku, newQuantity);
        return cart;
    }

    public Cart RemoveItem(string cartId, string sku)
    {
        _validator.ValidateRemoveItem(sku);

        var cart = GetOrCreateCart(cartId);
        cart.Remove(sku);
        return cart;
    }

    public long CalculateSubtotalCents(string cartId)
    {
        var cart = GetOrCreateCart(cartId);
        return cart.SubtotalCents();
    }

    private static void RequireNonBlank(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message);
        }
    }

    // ---- Minimal domain model (kept in-file for easy copy/paste) ----

    public sealed class Cart
    {
        private readonly Dictionary<string, LineItem> _items = new(StringComparer.Ordinal);

        public Cart(string cartId)
        {
            CartId = cartId;
        }

        public string CartId { get; }

        public IReadOnlyDictionary<string, LineItem> Items => new ReadOnlyDictionary<string, LineItem>(_items);

        public void AddOrIncrement(string sku, int qtyToAdd, long unitPriceCents)
        {
            if (!_items.TryGetValue(sku, out var existing))
            {
                _items[sku] = new LineItem(sku, qtyToAdd, unitPriceCents);
            }
            else
            {
                // Keep last known unit price (simple)
                existing.Quantity += qtyToAdd;
                existing.UnitPriceCents = unitPriceCents;
            }
        }

        public void SetQuantity(string sku, int newQty)
        {
            if (!_items.TryGetValue(sku, out var existing))
            {
                throw new CartNotFoundException($"SKU not found in cart: {sku}");
            }

            existing.Quantity = newQty;
        }

        public void Remove(string sku) => _items.Remove(sku);

        public long SubtotalCents()
        {
            long sum = 0;
            foreach (var li in _items.Values)
            {
                sum += li.UnitPriceCents * (long)li.Quantity;
            }
            return sum;
        }
    }

    public sealed class LineItem
    {
        public LineItem(string sku, int quantity, long unitPriceCents)
        {
            Sku = sku;
            Quantity = quantity;
            UnitPriceCents = unitPriceCents;
        }

        public string Sku { get; }
        public int Quantity { get; set; }
        public long UnitPriceCents { get; set; }
    }

    public sealed class CartNotFoundException : Exception
    {
        public CartNotFoundException(string message) : base(message) { }
    }
}
