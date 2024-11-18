using FacadeAccountCreation.Core.Models.Regulators;
using Newtonsoft.Json;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/regulators")]
public class RegulatorController(
    ILogger<RegulatorController> logger,
    IOrganisationService organisationService,
    IMessagingService messagingService)
    : ControllerBase
{
    [HttpPost]
    [Route("resubmission-notify")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SendNotificationOfResubmissionToUser(ResubmissionNotificationEmailModel request)
    {
        logger.LogDebug("{Request}", JsonConvert.SerializeObject(request));
        var regulatorOrganisation = organisationService.GetRegulatorOrganisationByNationId(request.NationId).Result;

        var resubmissionInput = new ResubmissionNotificationEmailInput
        {
            NationId = request.NationId,
            OrganisationNumber = request.OrganisationNumber,
            SubmissionPeriod = request.SubmissionPeriod,
            RegulatorOrganisationName = regulatorOrganisation?.OrganisationName,
            ProducerOrganisationName = request.ProducerOrganisationName,
            IsComplianceScheme = request.IsComplianceScheme
        };

        if (request.IsComplianceScheme)
        {
            resubmissionInput.ComplianceSchemeName = request.ComplianceSchemeName;
            resubmissionInput.ComplianceSchemePersonName = request.ComplianceSchemePersonName;
        }

        var notificationId = messagingService.SendPoMResubmissionEmailToRegulator(resubmissionInput);
        return Ok(notificationId);
    }
}