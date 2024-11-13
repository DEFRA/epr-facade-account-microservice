using FacadeAccountCreation.Core.Models.PaymentCalculation;

namespace FacadeAccountCreation.Core.Services.PaymentCalculation;

public interface IPaymentCalculationService
{
    Task<PaymentCalculationResponse?> ProducerRegistrationFees(PaymentCalculationRequest paymentCalculationRequest);
    Task<string?> PaymentInitiation(PaymentInitiationRequest paymentInitiationRequest);
}