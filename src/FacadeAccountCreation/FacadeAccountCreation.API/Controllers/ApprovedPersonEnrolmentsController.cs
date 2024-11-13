using FacadeAccountCreation.Core.Services.RoleManagement;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/enrolments")]
public class ApprovedPersonEnrolmentsController(
    IRoleManagementService roleManagementService,
    IMessagingService messagingService,
    ILogger<ApprovedPersonEnrolmentsController> logger)
    : ControllerBase
{
    [HttpPut]
    [Consumes("application/json")]
    [Route("{enrolmentId:guid}/approved-person-acceptance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptNominationForApprovedPerson(
        Guid enrolmentId,
        [BindRequired, FromBody] AcceptNominationApprovedPersonRequest acceptNominationRequest,
        [BindRequired, FromQuery] string serviceKey,
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        var userId = User.UserId();

        try
        {
            var result = await roleManagementService.AcceptNominationForApprovedPerson(enrolmentId, userId, organisationId, serviceKey, acceptNominationRequest);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError(
                    "Accept nomination failed for user {UserId} from organisation {OrganisationId} of service '{ServiceKey}'. StatusCode: {StatusCode}",
                    userId, organisationId, serviceKey, result.StatusCode);

                return HandleError.HandleErrorWithStatusCode(result.StatusCode);
            }

            messagingService.SendApprovedUserAccountCreationConfirmation(
                acceptNominationRequest.OrganisationName,
                acceptNominationRequest.PersonFirstName,
                acceptNominationRequest.PersonLastName,
                acceptNominationRequest.OrganisationNumber,
                acceptNominationRequest.ContactEmail);

            logger.LogInformation(
                "Nominated user {UserId} from organisation {OrganisationId} accepted the nomination to become an approved person. Service: '{ServiceKey}'",
                userId, organisationId, serviceKey);

            return Ok();                
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Accept nomination failed for user {UserId} from organisation {OrganisationId} of service '{ServiceKey}'.",
                userId, organisationId, serviceKey);

            return HandleError.Handle(exception);
        }
    }
}