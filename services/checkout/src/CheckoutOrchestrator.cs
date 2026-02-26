// services/checkout/src/CheckoutOrchestrator.cs

using System;

namespace Ecommerce.Services.Checkout;

/// <summary>
/// CheckoutOrchestrator coordinates dependent services:
/// - Cart totals
/// - Pricing (discount/tax/shipping)
/// - Payment processing
/// - Order state updates
///
/// This is where cross-service impact becomes obvious for Agent 2.
/// </summary>
public sealed class CheckoutOrchestrator
{
    private readonly ICartPort _cartPort;
    private readonly IPricingPort _pricingPort;
    private readonly CheckoutService _checkoutService;

    public CheckoutOrchestrator(ICartPort cartPort, IPricingPort pricingPort, CheckoutService checkoutService)
    {
        _cartPort = cartPort ?? throw new ArgumentNullException(nameof(cartPort), "cartPort must not be null");
        _pricingPort = pricingPort ?? throw new ArgumentNullException(nameof(pricingPort), "pricingPort must not be null");
        _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService), "checkoutService must not be null");
    }

    public CheckoutFacade.CheckoutReceipt PlaceOrder(string cartId, string userId)
    {
        // Pull cart subtotal from cart domain
        long subtotalCents = _cartPort.GetSubtotalCents(cartId);

        // Pricing step: apply discount/tax/shipping (kept simple and deterministic)
        var breakdown = _pricingPort.CalculateTotals(subtotalCents, userId);

        // Delegate to CheckoutService for payment + order state changes
        var result = _checkoutService.Checkout(cartId, userId, breakdown.TotalCents);

        return new CheckoutFacade.CheckoutReceipt(result.OrderId, result.PaymentId, breakdown.TotalCents);
    }

    // ---- Ports (interfaces) so this monorepo stays modular & testable ----

    public interface ICartPort
    {
        long GetSubtotalCents(string cartId);
    }

    public interface IPricingPort
    {
        PricingBreakdown CalculateTotals(long subtotalCents, string userId);

        public sealed record PricingBreakdown(long SubtotalCents, long DiscountCents, long TaxCents, long ShippingCents)
        {
            public long TotalCents
            {
                get
                {
                    long discounted = Math.Max(0, SubtotalCents - DiscountCents);
                    return discounted + TaxCents + ShippingCents;
                }
            }
        }
    }
}
