// services/cart/src/CartController.cs

using System;

namespace Ecommerce.Services.Cart;

/// <summary>
/// "Controller-style" wrapper for CartService.
/// This is framework-neutral so you can later wrap it with ASP.NET Core controllers if you want.
/// </summary>
public sealed class CartController
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService), "cartService must not be null");
    }

    public CartService.Cart GetCart(string cartId) => _cartService.GetOrCreateCart(cartId);

    public CartService.Cart AddItem(string cartId, string sku, int quantity, long unitPriceCents)
    {
        var item = new CartValidator.CartItem(sku, quantity, unitPriceCents);
        return _cartService.AddItem(cartId, item);
    }

    public CartService.Cart UpdateQuantity(string cartId, string sku, int newQuantity) =>
        _cartService.UpdateQuantity(cartId, sku, newQuantity);

    public CartService.Cart RemoveItem(string cartId, string sku) => _cartService.RemoveItem(cartId, sku);

    public long GetSubtotalCents(string cartId) => _cartService.CalculateSubtotalCents(cartId);
}
