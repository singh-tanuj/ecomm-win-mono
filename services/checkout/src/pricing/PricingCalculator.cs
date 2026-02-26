// services/checkout/src/pricing/PricingCalculator.cs

using System;

namespace Ecommerce.Services.Checkout.Pricing;

public sealed class PricingCalculator
{
    private readonly TaxService _taxService;

    public PricingCalculator(TaxService taxService)
    {
        _taxService = taxService ?? throw new ArgumentNullException(nameof(taxService), "taxService must not be null");
    }

    public double ApplyCoupon(double subtotal, Coupon? coupon)
    {
        if (coupon is null) return RoundingUtil.Round(subtotal);

        CouponValidator.Validate(coupon, subtotal);

        double discount;
        if (coupon.IsPercentage())
        {
            discount = subtotal * (coupon.Value / 100.0);
        }
        else
        {
            discount = coupon.Value;
        }

        double discountedSubtotal = SafeSubtract(subtotal, discount);
        return RoundingUtil.Round(discountedSubtotal);
    }

    public double ComputeTax(string region, double discountedSubtotal) => _taxService.ComputeTotalTax(region, discountedSubtotal);

    public double ComputeFinalTotal(double discountedSubtotal, double totalTax, double shippingCost)
    {
        double finalTotal = discountedSubtotal + totalTax + shippingCost;
        return RoundingUtil.Round(Math.Max(0, finalTotal));
    }

    private static double SafeSubtract(double a, double b) => Math.Max(0, a - b);
}
