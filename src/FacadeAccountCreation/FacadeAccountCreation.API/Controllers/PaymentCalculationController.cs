using FacadeAccountCreation.Core.Models.PaymentCalculations;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/paycal")]
public class PaymentCalculationController : ControllerBase
{
    private readonly ILogger<PaymentCalculationController> _logger;

    public PaymentCalculationController(ILogger<PaymentCalculationController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("producer-registration-fees")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ProducerRegistrationFees(PaymentCalculationRequest request)
    {
        var response = new PaymentCalculationResponse
        {
            ProducerRegistrationFee = 262000,
            ProducerOnlineMarketPlaceFee = 257900,
            ProducerLateRegistrationFee = 33200,
            SubsidiariesFee = 9071100,
            TotalFee = 9591000,
            PreviousPayment = 150000,
            SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdown
            {
                TotalSubsidiariesOMPFees = 7479100,
                CountOfOMPSubsidiaries = 29,
                UnitOMPFees = 257900,
                FeeBreakdowns = new FeeBreakdown[]
                {
                    new() { BandNumber = 1, UnitCount = 20, UnitPrice = 55800, TotalPrice = 1116000 }
                }
            }
        };

        return Ok(response);
    }
}
