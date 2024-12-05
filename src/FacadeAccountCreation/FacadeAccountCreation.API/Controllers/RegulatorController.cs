using FacadeAccountCreation.Core.Models.Enrolments;
using FacadeAccountCreation.Core.Models.Regulators;
using FacadeAccountCreation.Core.Services.Enrolments;
using Newtonsoft.Json;
using System.Text.Json;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/regulators")]
public class RegulatorController(
    ILogger<RegulatorController> logger,
    IOrganisationService organisationService,
    IEnrolmentService enrolmentService,
    IMessagingService messagingService)
    : ControllerBase
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

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

    [HttpGet]
    [Route("applications/enrolments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingApplicationsForOrganisation(Guid userId, Guid organisationId)
    {
        try
        {
            var response = await enrolmentService.GetOrganisationPendingApplications(userId, organisationId);
            if (response.IsSuccessStatusCode)
            {
                var stringContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ApplicationEnrolmentDetails>(stringContent, _options);
                return Ok(result);
            }

            logger.LogError("Failed to fetch pending enrolment applications");
            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching enrolment applications for organisation {OrganisationId}", organisationId);
            return HandleError.Handle(e);
        }
    }
}