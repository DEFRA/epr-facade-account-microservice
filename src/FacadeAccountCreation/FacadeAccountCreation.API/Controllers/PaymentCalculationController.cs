using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Models.PaymentCalculation;
using FacadeAccountCreation.Core.Services.PaymentCalculation;
using Microsoft.AspNetCore.Mvc;

namespace FacadeAccountCreation.API.Controllers
{
    [ApiController]
    [Route("api/paycal")]
    public class PaymentCalculationController : Controller
    {
        private readonly ILogger<PaymentCalculationController> _logger;
        private readonly IPaymentCalculationService _paymentCalculationService;

        public PaymentCalculationController(ILogger<PaymentCalculationController> logger, IPaymentCalculationService paymentCalculationService)
        {
            _logger = logger;
            _paymentCalculationService = paymentCalculationService;
        }

        [HttpPost]
        [Route("producer-registration-fees")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProducerRegistrationFees(PaymentCalculationRequest request)
        {
            try
            {
                var response = await _paymentCalculationService.ProducerRegistrationFees(request);
                if (response == null)
                {
                    return NoContent();
                }

                return Ok(response);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error calculating registration fee for application {ApplicationReferenceNumber}", request.ApplicationReferenceNumber);
                return HandleError.Handle(exception);
            }
        }
    }
}
