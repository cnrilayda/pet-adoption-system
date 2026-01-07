using PetAdoptionPlatform.Application.Interfaces;

namespace PetAdoptionPlatform.Infrastructure.Services.PaymentGateway;

public class MockedPaymentGateway : IPaymentGateway
{
    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string description, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        var random = new Random();
        var success = random.Next(1, 11) <= 9;

        if (success)
        {
            var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            return new PaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                Status = "Completed"
            };
        }
        else
        {
            return new PaymentResult
            {
                Success = false,
                TransactionId = string.Empty,
                Status = "Failed",
                ErrorMessage = "Mocked payment failure - insufficient funds"
            };
        }
    }
}

