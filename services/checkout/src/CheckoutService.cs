// services/checkout/src/CheckoutService.cs

using System;
using System.Collections.Generic;
using Ecommerce.Services.Checkout.Pricing;
using Ecommerce.Services.Payment;

namespace Ecommerce.Services.Checkout;

/// <summary>
/// Combined C# port of the repo's CheckoutService.java.
/// Note: The original monorepo had two usage shapes:
/// 1) region/subtotal/coupon/paymentMethod flow
/// 2) cartId/userId/totalCents flow (used by CheckoutOrchestrator)
/// This class supports both via overloads.
/// </summary>
public sealed class CheckoutService
{
    private readonly PricingEngine _pricingEngine;
    private readonly PaymentService _paymentService;
    private readonly IShippingService _shippingService;
    private readonly IOrderPort? _orderPort;

    public CheckoutService(PricingEngine pricingEngine,
                           PaymentService paymentService,
                           IShippingService shippingService,
                           IOrderPort? orderPort = null)
    {
        _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _shippingService = shippingService ?? throw new ArgumentNullException(nameof(shippingService));
        _orderPort = orderPort;
    }

    // === Shape #1: region/subtotal/coupon/paymentMethod ===

    public CheckoutReceipt Checkout(string region, double subtotal, string? coupon, string paymentMethod)
    {
        // Apply discount (Story 3)
        double discountedSubtotal = _pricingEngine.ApplyDiscount(region, subtotal, coupon);

        // Multi-jurisdiction tax (Story 12)
        List<double> taxRates = GetTaxRates(region);

        double totalTax = _pricingEngine.ComputeTotalTax(region, discountedSubtotal, taxRates);
        double totalWithTax = discountedSubtotal + totalTax;

        // Shipping AFTER discount (corrected)
        double shippingCost = _shippingService.CalculateShipping(region, discountedSubtotal);

        double finalTotal = totalWithTax - shippingCost;

        // Payment
        _paymentService.ProcessPayment(finalTotal, paymentMethod);

        return new CheckoutReceipt(discountedSubtotal, totalTax, shippingCost, finalTotal);
    }

    private static List<double> GetTaxRates(string region)
    {
        if (string.Equals(region, "US-CA", StringComparison.Ordinal)) return [0.06, 0.02];
        if (string.Equals(region, "US-NY", StringComparison.Ordinal)) return [0.04, 0.03];
        return [0.05];
    }

    public sealed record CheckoutReceipt(double DiscountedSubtotal, double TotalTax, double ShippingCost, double FinalTotal);

    // === Shape #2: cartId/userId/totalCents (used by CheckoutOrchestrator) ===

    public CheckoutResult Checkout(string cartId, string userId, long totalCents)
    {
        // In a real system this method would:
        // - create order
        // - call payment
        // - update order state
        // Here we keep it deterministic and compile-friendly.

        string orderId = _orderPort?.CreateOrder(cartId, userId, totalCents)
                         ?? $"ord_{Guid.NewGuid():N}"[..16];

        string paymentId = _paymentService.ProcessPayment(totalCents / 100.0, paymentMethod: "default");

        _orderPort?.MarkPaymentSucceeded(orderId, paymentId);

        return new CheckoutResult(orderId, paymentId);
    }

    public sealed record CheckoutResult(string OrderId, string PaymentId);

    public interface IShippingService
    {
        double CalculateShipping(string region, double discountedSubtotal);
    }

    public interface IOrderPort
    {
        string CreateOrder(string cartId, string userId, long totalCents);
        void MarkPaymentSucceeded(string orderId, string paymentId);
    }
}
