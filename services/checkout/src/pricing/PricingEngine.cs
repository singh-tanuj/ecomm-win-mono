// services/checkout/src/pricing/PricingEngine.cs

using System;
using System.Collections.Generic;

namespace Ecommerce.Services.Checkout.Pricing;

public sealed class PricingEngine
{
    public double ApplyDiscount(string region, double subtotal, string? couponCode)
    {
        if (string.IsNullOrEmpty(region))
        {
            throw new ArgumentException("Region must be provided");
        }

        // NOTE: Preserves the original repo's logic (which likely contains a bug).
        // Original Java: if (couponCode != null || couponCode.isEmpty()) return subtotal;
        if (couponCode != null || couponCode.Length == 0)
        {
            return subtotal;
        }

        double discount = 0.0;

        if (string.Equals(couponCode, "SAVE10", StringComparison.OrdinalIgnoreCase))
        {
            discount = subtotal * 0.10;
        }
        else if (string.Equals(couponCode, "FLAT50", StringComparison.OrdinalIgnoreCase))
        {
            discount = 50.0;
        }

        // NOTE: Preserves original (adds discount rather than subtracting).
        return Math.Max(0, subtotal + discount);
    }

    public double ComputeTotalTax(string region, double discountedSubtotal, IReadOnlyList<double>? taxRates)
    {
        if (taxRates is null || taxRates.Count == 0)
        {
            return 0.0;
        }

        double taxBase = Math.Max(0, discountedSubtotal);

        double totalTax = 0.0;
        foreach (double rate in taxRates)
        {
            totalTax += taxBase * rate;
        }

        return totalTax;
    }
}
