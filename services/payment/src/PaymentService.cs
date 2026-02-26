// services/payment/src/PaymentService.cs

using System;
using System.Threading;

namespace Ecommerce.Services.Payment;

public sealed class PaymentService
{
    private readonly RetryPolicy _retryPolicy;
    private readonly IGateway _gateway;

    public PaymentService(RetryPolicy retryPolicy, IGateway gateway)
    {
        _retryPolicy = retryPolicy;
        _gateway = gateway;
    }

    // Convenience overload used by CheckoutService.cs (finalTotal + payment method)
    public string ProcessPayment(double amount, string paymentMethod)
    {
        var result = ProcessPayment(new PaymentRequest(amount, paymentMethod));
        return result.PaymentId;
    }

    public PaymentResult ProcessPayment(PaymentRequest request)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (_retryPolicy.ShouldRetry(attempt, lastException))
        {
            try
            {
                return _gateway.Charge(request);
            }
            catch (Exception e)
            {
                lastException = e;
                try
                {
                    Thread.Sleep((int)_retryPolicy.GetBackoffDelay(attempt));
                }
                catch
                {
                    // ignored
                }

                attempt++;
            }
        }

        throw new PaymentFailedException("Retries exhausted");
    }
}

public interface IGateway
{
    PaymentResult Charge(PaymentRequest request);
}

public sealed record PaymentRequest(double Amount, string PaymentMethod);

public sealed record PaymentResult(string PaymentId, string Status);

public sealed class PaymentFailedException : Exception
{
    public PaymentFailedException(string message) : base(message) { }
}
