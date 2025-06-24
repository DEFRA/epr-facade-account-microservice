using FacadeAccountCreation.Core.Models.DelegatedPerson;
using FacadeAccountCreation.Core.Services.RoleManagement;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/enrolments")]
public class DelegatedPersonEnrolmentsController(
    IRoleManagementService roleManagementService,
    ILogger<DelegatedPersonEnrolmentsController> logger)
    : ControllerBase
{
    [HttpPut]
    [Consumes("application/json")]
    [Route("{enrolmentId:guid}/delegated-person-acceptance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptNominationToDelegatedPerson(
        Guid enrolmentId,
        [BindRequired, FromBody] AcceptNominationRequest acceptNominationRequest,
        [BindRequired, FromQuery] string serviceKey,
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        var userId = User.UserId();

        try
        {
            var result = await roleManagementService.AcceptNominationToDelegatedPerson(enrolmentId, userId, organisationId, serviceKey, acceptNominationRequest);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                logger.LogInformation(
                    "Nominated user {UserId} from organisation {OrganisationId} accepted the nomination to Delegated Person of service '{ServiceKey}'",
                    userId, organisationId, serviceKey);

                return Ok();
            }

            logger.LogError(
                "Accept nomination failed for user {UserId} from organisation {OrganisationId} of service '{ServiceKey}'. StatusCode: {StatusCode}",
                userId, organisationId, serviceKey, result.StatusCode);

            return HandleError.HandleErrorWithStatusCode(result.StatusCode);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Accept nomination failed for user {UserId} from organisation {OrganisationId} of service '{ServiceKey}'.", userId, organisationId, serviceKey);

            return HandleError.Handle(exception);
        }
    }

    [HttpGet]
    [Route("{enrolmentId:guid}/delegated-person-nominator")]
    [ProducesResponseType(typeof(DelegatedPersonNominatorModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DelegatedPersonNominatorModel>> Get(
        Guid enrolmentId,
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId,
        [BindRequired, FromQuery] string serviceKey)
    {
        var userId = User.UserId();
            
        try
        {

            var person = await roleManagementService.GetDelegatedPersonNominator(enrolmentId, userId, organisationId, serviceKey);
            if (person == null)
            {
                logger.LogError("Failed to fetch the delegated person nominator");
                return HandleError.HandleErrorWithStatusCode(HttpStatusCode.NotFound);
            }
                    
            logger.LogInformation("Fetched the delegated person nominator");
            return Ok(person);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching the delegated person nominator");
            return HandleError.Handle(e);
        }
    }
}