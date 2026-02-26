// services/checkout/src/pricing/CouponValidator.cs

using System;

namespace Ecommerce.Services.Checkout.Pricing;

public static class CouponValidator
{
    public static void Validate(Coupon coupon, double subtotal)
    {
        if (coupon is null) throw new ArgumentNullException(nameof(coupon));

        if (!coupon.IsEligible(subtotal))
        {
            throw new InvalidOperationException($"Coupon {coupon.Code} requires min subtotal {coupon.MinSubtotal}");
        }

        if (coupon.Value < 0)
        {
            throw new InvalidOperationException("Coupon value cannot be negative");
        }
    }
}
