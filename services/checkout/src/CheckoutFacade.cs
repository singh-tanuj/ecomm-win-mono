// services/checkout/src/CheckoutFacade.cs

using System;

namespace Ecommerce.Services.Checkout;

/// <summary>
/// CheckoutFacade is the API-facing entry point for checkout flows.
/// In a real app, a REST controller would call this facade.
/// </summary>
public sealed class CheckoutFacade
{
    private readonly CheckoutOrchestrator _orchestrator;

    public CheckoutFacade(CheckoutOrchestrator orchestrator)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator), "orchestrator must not be null");
    }

    /// <summary>
    /// Initiates checkout for a given cart + user.
    /// </summary>
    public CheckoutReceipt Checkout(string cartId, string userId) => _orchestrator.PlaceOrder(cartId, userId);

    /// <summary>
    /// Minimal DTO returned to caller.
    /// </summary>
    public sealed record CheckoutReceipt(string OrderId, string PaymentId, long TotalCents);
}
