using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Models.Person;
using FacadeAccountCreation.Core.Services.Connection;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.Core.Services.Person;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Net;

namespace FacadeAccountCreation.API.Controllers
{
    [Route("api/connections")]
    public class ConnectionsController : Controller
    {
        private readonly IRoleManagementService _roleManagementService;
        private readonly IMessagingService _messagingService;
        private readonly IPersonService _personService;
        private readonly ILogger<ConnectionsController> _logger;
        private readonly MessagingConfig _messagingConfig;

        public ConnectionsController(
            ILogger<ConnectionsController> logger,
            IRoleManagementService roleManagementService, 
            IMessagingService messagingService, 
            IPersonService personService,
            IOptions<MessagingConfig> messagingConfig)
        {
            _logger = logger;
            _roleManagementService = roleManagementService;
            _messagingService = messagingService;
            _personService = personService;
            _messagingConfig = messagingConfig.Value;
        }

        [HttpGet]
        [Route("{connectionId:guid}/person")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConnectionPersonModel>> GetConnectionPerson(
           Guid connectionId,
           [BindRequired, FromQuery] string serviceKey,
           [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
        {
            try
            {
                var userId = User.UserId();

                var person = await _roleManagementService.GetPerson(connectionId, serviceKey, userId, organisationId);
                if (person == null)
                {
                    _logger.LogError("Failed to fetch the person for connection {connectionId}", connectionId);
                    return HandleError.HandleErrorWithStatusCode(HttpStatusCode.NotFound);
                }
                
                _logger.LogInformation("Fetched the person for connection {connectionId}", connectionId);
                return Ok(person);
            }
            catch (Exception e)
            {
                _logger.LogError("Error fetching the person for connection {connectionId}", connectionId);
                return HandleError.Handle(e);
            }
        }

        [HttpGet]
        [Route("{connectionId:guid}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConnectionWithEnrolmentsModel>> GetConnectionEnrolements(
           Guid connectionId,
           [BindRequired, FromQuery] string serviceKey,
           [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
        {
            try
            {
                var userId = User.UserId();

                var connectionWithEnrolments = await _roleManagementService.GetEnrolments(connectionId, serviceKey, userId, organisationId);
                if (connectionWithEnrolments == null)
                {
                    _logger.LogError("Failed to fetch the enrolments for connection {connectionId}", connectionId);
                    return HandleError.HandleErrorWithStatusCode(HttpStatusCode.NotFound);
                }
                else
                {
                    _logger.LogInformation("Fetched the enrolments for connection {connectionId}", connectionId);

                    return Ok(connectionWithEnrolments);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error fetching the enrolments for connection {connectionId}", connectionId);
                return HandleError.Handle(e);
            }
        }

        [HttpPut]
        [Produces("application/json")]
        [Route("{connectionId:guid}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdatePersonRole(
            Guid connectionId,
            [BindRequired, FromBody] UpdatePersonRoleRequest updateRequest,
            [BindRequired, FromQuery] string serviceKey,
            [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
        {
            var userId = User.UserId();
            
            try
            {
                var result = await _roleManagementService.UpdatePersonRole(connectionId, userId, organisationId, serviceKey, updateRequest);

                if (result.RemovedServiceRoles?.Any(role => role.ServiceRoleKey == ServiceRoles.Packaging.DelegatedPerson) == true)
                {
                    var delegatedPerson = await _roleManagementService.GetPerson(connectionId, serviceKey, userId, organisationId);
                    if (delegatedPerson == null)
                    {
                        _logger.LogError("Unable to send email notification of removal of {PersonRole} Delegated Person role. " +
                                         "User {UserId} from organisation {OrganisationId} failed to find Delegated Person " +
                                         "for connection {ConnectionId} of service '{ServiceKey}'",
                            updateRequest.PersonRole, userId, organisationId, connectionId, serviceKey);
                        return HandleError.HandleErrorWithStatusCode(HttpStatusCode.NotFound);
                    }
                    
                    var approvedPerson = await _personService.GetPersonByUserIdAsync(userId);
                    if (approvedPerson == null)
                    {
                        _logger.LogError("Unable to send email notification of removal of Delegated Person role. Approved User {UserId} not found", userId);
                        return HandleError.HandleErrorWithStatusCode(HttpStatusCode.NotFound);
                    }
                    await SendNotificationEmail(result.RemovedServiceRoles, connectionId, userId, organisationId, serviceKey, updateRequest.PersonRole, delegatedPerson, approvedPerson);
                }

                _logger.LogInformation(
                    "User {UserId} from organisation {OrganisationId} updated person role to '{PersonRole}' " +
                    "for connection {ConnectionId} of service '{ServiceKey}'",
                    userId, organisationId, updateRequest.PersonRole, connectionId, serviceKey);

                return Ok();
            }
            catch (HttpRequestException requestException)
            {
                _logger.LogError(
                    "User {UserId} from organisation {OrganisationId} failed to update person role '{PersonRole}' " +
                    "for connection {ConnectionId} of service '{ServiceKey}'",
                    userId, organisationId, updateRequest.PersonRole, connectionId, serviceKey);

                return HandleError.Handle(requestException);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "User {UserId} from organisation {OrganisationId} failed to update person role '{PersonRole}' " +
                    "for connection {ConnectionId} of service '{ServiceKey}'",
                    userId, organisationId, updateRequest.PersonRole, connectionId, serviceKey);
            
                return HandleError.Handle(exception);
            }
        }

        [HttpPut]
        [Consumes("application/json")]
        [Route("{connectionId:guid}/delegated-person-nomination")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> NominateToDelegatedPerson(
            Guid connectionId,
            [BindRequired, FromBody] DelegatedPersonNominationRequest nominationRequest,
            [BindRequired, FromQuery] string serviceKey,
            [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
        {
            var userId = User.UserId();
   
            try
            {
                var result = await _roleManagementService.NominateToDelegatedPerson(connectionId, userId, organisationId, serviceKey, nominationRequest);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation(
                        "User {UserId} from organisation {OrganisationId} nominated to Delegated Person a user " +
                        "for connection {ConnectionId} of service '{ServiceKey}'",
                        userId, organisationId, connectionId, serviceKey);

                    await SendDelegatedUserNomination(connectionId, serviceKey, userId, organisationId);

                    return Ok();
                }

                _logger.LogError(
                    "User {UserId} from organisation {OrganisationId} failed to nominate to Delegated Person a user " +
                    "for connection {ConnectionId} of service '{ServiceKey}'. StatusCode: {StatusCode}",
                    userId, organisationId, connectionId, serviceKey, result.StatusCode);

                return HandleError.HandleErrorWithStatusCode(result.StatusCode);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "User {UserId} from organisation {OrganisationId} failed to nominate to Delegated Person a user " +
                    "for connection {ConnectionId} of service '{ServiceKey}'",
                    userId, organisationId, connectionId, serviceKey);

                return HandleError.Handle(exception);
            }
        }

        private async Task SendDelegatedUserNomination(Guid connectionId, string serviceKey, Guid userId, Guid organisationId)
        {
            var nominatedPerson = await _roleManagementService.GetPerson(connectionId, serviceKey, userId, organisationId);
            var nominatingPerson = await _personService.GetPersonByUserIdAsync(userId);

            if (nominatedPerson == null)
            { 
                _logger.LogError("Unable to send Delegated User Nomination Email. " +
                    "User {UserId} from organisation {OrganisationId} failed to find nominated person " +
                    "for connection {ConnectionId} of service '{ServiceKey}'",
                    userId, organisationId, connectionId, serviceKey);
            }
            if (nominatingPerson == null)
            {
                _logger.LogError("Unable to send Delegated User Nomination Email. User {UserId} not found", userId);
            }

            if (nominatedPerson != null && nominatingPerson != null)
            {
                _messagingService.SendDelegatedUserNomination(new NominateUserEmailInput()
                {
                    FirstName = nominatedPerson.FirstName,
                    LastName = nominatedPerson.LastName,
                    OrganisationName = nominatedPerson.OrganisationName,
                    OrganisationId = organisationId,
                    UserId = userId,
                    NominatorFirstName = nominatingPerson.FirstName,
                    NominatorLastName = nominatingPerson.LastName,
                    OrganisationNumber = nominatedPerson.OrganisationReferenceNumber.ToReferenceNumberFormat(),
                    Recipient = nominatedPerson.Email,
                    TemplateId = _messagingConfig.NominateDelegatedUserTemplateId,
                    AccountLoginUrl = _messagingConfig.AccountLoginUrl
                });
            }
        }

        private async Task SendNotificationEmail(List<RemovedServiceRole> removedServiceRoles, Guid connectionId, Guid userId, Guid organisationId, string serviceKey, PersonRole personRole, ConnectionPersonModel delegatedPerson, PersonResponseModel approvedPerson)
        {
            var removedServiceRole = removedServiceRoles.FirstOrDefault(role => role.ServiceRoleKey == ServiceRoles.Packaging.DelegatedPerson && role.EnrolmentStatus == EnrolmentStatus.Approved);

            removedServiceRole ??= removedServiceRoles.FirstOrDefault(role => role.ServiceRoleKey == ServiceRoles.Packaging.DelegatedPerson);

            if (removedServiceRole == null)
            {
                return;
            }

            var delegatedRoleEmailInput = new DelegatedRoleEmailInput()
            {
                FirstName = delegatedPerson.FirstName,
                LastName = delegatedPerson.LastName,
                OrganisationName = delegatedPerson.OrganisationName,
                OrganisationId = organisationId,
                UserId = userId,
                NominatorFirstName = approvedPerson.FirstName,
                NominatorLastName = approvedPerson.LastName,
                OrganisationNumber = delegatedPerson.OrganisationReferenceNumber.ToReferenceNumberFormat(),
                Recipient = delegatedPerson.Email,
                PersonRole = personRole
            };

            switch (removedServiceRole.EnrolmentStatus)
            {
                case EnrolmentStatus.Approved:
                    {
                        delegatedRoleEmailInput.TemplateId = _messagingConfig.DelegatedRoleRemovedTemplateId;
                        break;
                    }
                case EnrolmentStatus.Pending:
                case EnrolmentStatus.Nominated:
                    {
                        delegatedRoleEmailInput.TemplateId = _messagingConfig.NominationCancelledTemplateId;
                        break;
                    }
            }

            _ = _messagingService.SendNominationCancelledNotification(delegatedRoleEmailInput);
        }
    }
}
