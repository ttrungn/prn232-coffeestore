using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Mappers;

public static class PaymentMapper
{
    public static PaymentResponse? ToPaymentResponse(this Payment? payment)
    {
        if (payment == null)
            return null;

        return new PaymentResponse
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = nameof(payment.PaymentMethod)
        };
    }
}
