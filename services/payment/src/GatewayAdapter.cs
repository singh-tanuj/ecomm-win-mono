// services/payment/src/GatewayAdapter.cs

namespace Ecommerce.Services.Payment;

public sealed class GatewayAdapter : IGateway
{
    public PaymentResult Charge(PaymentRequest request)
    {
        // Deterministic "success" stub
        return new PaymentResult(PaymentId: "pay_stub", Status: "AUTHORIZED");
    }
}
