// services/checkout/src/pricing/TaxService.cs

namespace Ecommerce.Services.Checkout.Pricing;

public sealed class TaxService
{
    private readonly TaxRateRepository _repo;

    public TaxService(TaxRateRepository repo)
    {
        _repo = repo;
    }

    public double ComputeTotalTax(string region, double discountedSubtotal)
    {
        var rates = _repo.GetRates(region);
        var engine = new PricingEngine();
        return engine.ComputeTotalTax(region, discountedSubtotal, rates);
    }
}
