using FacadeAccountCreation.Core.Models.PaymentCalculation;
using System.Threading.Tasks;

namespace FacadeAccountCreation.Core.Services.PaymentCalculation
{
    public interface IPaymentCalculationService
    {
        Task<PaymentCalculationResponse> ProducerRegistrationFees(PaymentCalculationRequest paymentCalculationRequest);
    }
}
