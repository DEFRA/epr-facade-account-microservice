using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Models.PaymentCalculation;
using FacadeAccountCreation.Core.Services.PaymentCalculation;
using Microsoft.AspNetCore.Mvc;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/paycal")]
public class PaymentCalculationController(
    ILogger<PaymentCalculationController> logger,
    IPaymentCalculationService paymentCalculationService)
    : ControllerBase
{
    [HttpPost]
    [Route("producer-registration-fees")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProducerRegistrationFees(PaymentCalculationRequest request)
    {
        try
        {
            var response = await paymentCalculationService.ProducerRegistrationFees(request);
            if (response == null)
            {
                return NoContent();
            }

            return Ok(response);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error calculating registration fee for application {Reference}", request.ApplicationReferenceNumber);
            return HandleError.Handle(exception);
        }
    }

    [HttpPost]
    [Route("Initiate-Payment")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PaymentInitiation(PaymentInitiationRequest request)
    {
        try
        {
            var response = await paymentCalculationService.PaymentInitiation(request);

            return response == null ? NoContent() : Ok(response);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to Initialise Payment for application Reference : {Reference}", request.Reference);
            return HandleError.Handle(exception);
        }
    }
}