using System;
using System.Collections.Generic;
using Ecommerce.Services.Checkout.Pricing;
using Ecommerce.Services.Payment;

namespace Ecommerce.Services.Checkout;

public sealed class CheckoutService
{
    private readonly PricingEngine _pricingEngine;
    private readonly PaymentService _paymentService;
    private readonly IShippingService _shippingService;
    private readonly IOrderPort? _orderPort;

    private const string RegionUsCa = "US-CA";
    private const string RegionUsNy = "US-NY";

    public CheckoutService(
        PricingEngine pricingEngine,
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
        var discountedSubtotal = _pricingEngine.ApplyDiscount(region, subtotal, coupon);

        var taxRates = GetTaxRates(region);
        var totalTax = _pricingEngine.ComputeTotalTax(region, discountedSubtotal, taxRates);

        var totalWithTax = discountedSubtotal + totalTax;

        // Shipping is calculated after discount (existing behavior preserved)
        var shippingCost = _shippingService.CalculateShipping(region, discountedSubtotal);

        var finalTotal = totalWithTax + shippingCost;

        _paymentService.ProcessPayment(finalTotal, paymentMethod);

        return new CheckoutReceipt(
            discountedSubtotal,
            totalTax,
            shippingCost,
            finalTotal);
    }

    private static List<double> GetTaxRates(string region)
    {
        return region switch
        {
            RegionUsCa => [0.06, 0.02],
            RegionUsNy => [0.04, 0.03],
            _ => [0.05]
        };
    }

    public sealed record CheckoutReceipt(
        double DiscountedSubtotal,
        double TotalTax,
        double ShippingCost,
        double FinalTotal);

    // === Shape #2: cartId/userId/totalCents ===

    public CheckoutResult Checkout(string cartId, string userId, long totalCents)
    {
        var orderId = _orderPort?.CreateOrder(cartId, userId, totalCents)
                      ?? GenerateFallbackOrderId();

        var paymentAmount = totalCents / 100.0;
        var paymentId = _paymentService.ProcessPayment(paymentAmount, "default");

        _orderPort?.MarkPaymentSucceeded(orderId, paymentId);

        return new CheckoutResult(orderId, paymentId);
    }

    private static string GenerateFallbackOrderId()
        => $"ord_{Guid.NewGuid():N}"[..16];

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
