// services/payment/tests/RetryPolicyTests.cs

using System;
using System.Diagnostics;
using Ecommerce.Services.Payment;

namespace Ecommerce.Services.Payment.Tests;

public static class RetryPolicyTests
{
    // Minimal self-check style test (no external test framework dependency).
    public static void ShouldRetryForTransientErrors()
    {
        var policy = new RetryPolicy();
        Debug.Assert(policy.ShouldRetry(2, new TimeoutException()));
    }
}
