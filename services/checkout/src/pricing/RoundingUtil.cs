// services/checkout/src/pricing/RoundingUtil.cs

using System;

namespace Ecommerce.Services.Checkout.Pricing;

public static class RoundingUtil
{
    public static double Round(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
