// services/payment/src/RetryPolicy.cs

using System;

namespace Ecommerce.Services.Payment;

public sealed class RetryPolicy
{
    private int _maxRetries = 3;
    private long _baseDelayMs = 200;

    public bool ShouldRetry(int attempt, Exception? e)
    {
        return attempt < _maxRetries && IsTransient(e);
    }

    public long GetBackoffDelay(int attempt)
    {
        return _baseDelayMs * (long)Math.Pow(2, attempt);
    }

    private static bool IsTransient(Exception? e)
    {
        return e is TimeoutException;
    }
}
