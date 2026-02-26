// services/order/src/OrderService.cs

using System;
using System.Collections.Generic;

namespace Ecommerce.Services.Order;

/// <summary>
/// Minimal OrderService:
/// - creates orders
/// - marks payment success/failure
///
/// Uses an in-memory store for demo purposes.
/// </summary>
public sealed class OrderService
{
    private readonly OrderStateMachine _stateMachine;
    private readonly Dictionary<string, OrderRecord> _orders = new(StringComparer.Ordinal);

    public OrderService(OrderStateMachine stateMachine)
    {
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine), "stateMachine must not be null");
    }

    public string CreateOrder(string cartId, string userId, long totalCents)
    {
        RequireNonBlank(cartId, "cartId must not be blank");
        RequireNonBlank(userId, "userId must not be blank");
        if (totalCents <= 0) throw new ArgumentException("totalCents must be >= 0");

        string orderId = $"ord_{Guid.NewGuid():N}"[..16];

        var o = new OrderRecord(orderId, cartId, userId, totalCents, _stateMachine.Initial());

        // Common approach: once created, mark as PAYMENT_PENDING
        o.Status = _stateMachine.MarkPaymentPending(o.Status);

        _orders[orderId] = o;
        return orderId;
    }

    public void MarkPaymentSucceeded(string orderId, string paymentId)
    {
        RequireNonBlank(paymentId, "paymentId must not be blank");
        var o = Get(orderId);

        o.PaymentId = paymentId;
        o.Status = _stateMachine.MarkPaid(o.Status);
    }

    public void MarkPaymentFailed(string orderId, string reason)
    {
        RequireNonBlank(reason, "reason must not be blank");
        var o = Get(orderId);

        o.FailureReason = reason;
        o.Status = _stateMachine.MarkPaymentFailed(o.Status);
    }

    public OrderRecord GetOrder(string orderId) => Get(orderId);

    private OrderRecord Get(string orderId)
    {
        RequireNonBlank(orderId, "orderId must not be blank");

        if (!_orders.TryGetValue(orderId, out var o))
        {
            throw new OrderNotFoundException($"Order not found: {orderId}");
        }

        return o;
    }

    private static void RequireNonBlank(string? v, string msg)
    {
        if (string.IsNullOrWhiteSpace(v)) throw new ArgumentException(msg);
    }

    // ---- Domain object (record-like class) ----

    public sealed class OrderRecord
    {
        public OrderRecord(string orderId, string cartId, string userId, long totalCents, OrderStateMachine.OrderStatus status)
        {
            OrderId = orderId;
            CartId = cartId;
            UserId = userId;
            TotalCents = totalCents;
            Status = status;
        }

        public string OrderId { get; }
        public string CartId { get; }
        public string UserId { get; }
        public long TotalCents { get; }

        public OrderStateMachine.OrderStatus Status { get; set; }

        public string? PaymentId { get; set; }
        public string? FailureReason { get; set; }
    }

    public sealed class OrderNotFoundException : Exception
    {
        public OrderNotFoundException(string message) : base(message) { }
    }
}
