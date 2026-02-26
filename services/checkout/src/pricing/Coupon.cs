// services/checkout/src/pricing/Coupon.cs

using System;
using System.Collections.Generic;

namespace Ecommerce.Services.Checkout.Pricing;

public sealed class Coupon
{
    public enum CouponType
    {
        Percentage,
        Fixed
    }

    public Coupon(string code, CouponType type, double value, double minSubtotal, ISet<string>? excludedSkus)
    {
        if (string.IsNullOrEmpty(code)) throw new ArgumentException("Coupon code cannot be empty");
        if (value < 0) throw new ArgumentException("Coupon value cannot be negative");

        Code = code;
        Type = type;
        Value = value;
        MinSubtotal = minSubtotal;
        ExcludedSkus = excludedSkus;
    }

    public string Code { get; }
    public CouponType Type { get; }
    public double Value { get; }
    public double MinSubtotal { get; }
    public ISet<string>? ExcludedSkus { get; }

    public bool IsEligible(double subtotal) => subtotal >= MinSubtotal;

    public bool IsSkuExcluded(string sku) => ExcludedSkus is not null && ExcludedSkus.Contains(sku);

    public bool IsPercentage() => Type == CouponType.Percentage;
}
