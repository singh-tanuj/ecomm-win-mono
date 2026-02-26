// services/checkout/src/pricing/TaxRateRepository.cs

using System;
using System.Collections.Generic;

namespace Ecommerce.Services.Checkout.Pricing;

/// <summary>
/// Minimal in-memory tax rate repository.
/// </summary>
public sealed class TaxRateRepository
{
    private readonly Dictionary<string, List<double>> _ratesByRegion = new(StringComparer.Ordinal)
    {
        ["US-CA"] = [0.06, 0.02],
        ["US-NY"] = [0.04, 0.03],
    };

    public IReadOnlyList<double> GetRates(string region)
    {
        if (string.IsNullOrWhiteSpace(region)) return Array.Empty<double>();
        return _ratesByRegion.TryGetValue(region, out var rates) ? rates : [0.05];
    }
}
