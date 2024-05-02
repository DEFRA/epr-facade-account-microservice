using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Services.Connection;
using FacadeAccountCreation.API.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacadeAccountCreation.API.Controllers
{
    [ApiController]
    [Route("api/enrolments")]
    public class ApprovedPersonEnrolmentsController : Controller
    {
        private readonly IRoleManagementService _roleManagementService;
        private readonly ILogger<ApprovedPersonEnrolmentsController> _logger;

        public ApprovedPersonEnrolmentsController(IRoleManagementService roleManagementService, ILogger<ApprovedPersonEnrolmentsController> logger)
        {
            _roleManagementService = roleManagementService;
            _logger = logger;
        }
         
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
                var result = await _roleManagementService.AcceptNominationForApprovedPerson(enrolmentId, userId, organisationId, serviceKey, acceptNominationRequest);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation(
                        "Nominated user {UserId} from organisation {OrganisationId} accepted the nomination to Delegated Person of service '{ServiceKey}'",
                        userId, organisationId, serviceKey);

                    return Ok();
                }

                _logger.LogError(
                    "Accept nomination failed for user {UserId} from organisation {OrganisationId} of service '{ServiceKey}'. StatusCode: {StatusCode}",
                    userId, organisationId, serviceKey, result.StatusCode);

                return HandleError.HandleErrorWithStatusCode(result.StatusCode);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "Accept nomination failed for user {UserId} from organisation {OrganisationId} of service '{ServiceKey}'.",
                    userId, organisationId, serviceKey);

                return HandleError.Handle(exception);
            }
        }
    }
}
