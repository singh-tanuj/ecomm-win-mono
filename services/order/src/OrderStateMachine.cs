// services/order/src/OrderStateMachine.cs

using System;

namespace Ecommerce.Services.Order;

/// <summary>
/// Stub state machine to match how OrderService uses it.
/// </summary>
public sealed class OrderStateMachine
{
    public enum OrderStatus
    {
        Created,
        PaymentPending,
        Paid,
        PaymentFailed
    }

    public OrderStatus Initial() => OrderStatus.Created;

    public OrderStatus MarkPaymentPending(OrderStatus current)
    {
        // Deterministic transition; keep permissive for demo.
        return OrderStatus.PaymentPending;
    }

    public OrderStatus MarkPaid(OrderStatus current) => OrderStatus.Paid;

    public OrderStatus MarkPaymentFailed(OrderStatus current) => OrderStatus.PaymentFailed;
}
