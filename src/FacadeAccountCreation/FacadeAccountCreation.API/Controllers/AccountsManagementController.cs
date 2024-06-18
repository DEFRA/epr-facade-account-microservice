using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Services.CreateAccount;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.Resource;
using System.Net;
using System.Web;

namespace FacadeAccountCreation.API.Controllers
{
    using System.Text.Json;

    [ApiController]
    [RequiredScope("account-creation")]
    [Route("api/accounts-management")]
    public class AccountsManagementController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMessagingService _messagingService;
        private readonly MessagingConfig _messagingConfig;
        private readonly IServiceRolesLookupService _serviceRolesLookupService;
        private readonly ILogger<AccountsManagementController> _logger;
        
        public AccountsManagementController(
            IAccountService accountService, 
            IMessagingService messagingService, 
            IOptions<MessagingConfig> messagingConfig, 
            IServiceRolesLookupService serviceRolesLookupService,
            ILogger<AccountsManagementController> logger)
        {
            _accountService = accountService;
            _messagingService = messagingService;
            _messagingConfig = messagingConfig.Value;
            _serviceRolesLookupService = serviceRolesLookupService;
            _logger = logger;
        }

        [HttpPost]
        [Route("invite-user")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InviteUser(AccountInvitationModel invitation)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid request. Request model: {invitation}", JsonSerializer.Serialize(invitation));
                return Problem("Invalid Request", statusCode: StatusCodes.Status400BadRequest);
            }

            invitation.InvitingUser.UserId = User.UserId();
            invitation.InvitingUser.Email = User.Email();
            
            var serviceRolesLookupModel = _serviceRolesLookupService.GetServiceRoles().SingleOrDefault(x =>
                x.Key == invitation.InvitedUser.Rolekey && !string.IsNullOrWhiteSpace(x.InvitationTemplateId));

            if (serviceRolesLookupModel is null)
            {
                _logger.LogError("Invalid role key: {key}", invitation.InvitedUser.Rolekey);
                return Problem("Invalid role key", statusCode: StatusCodes.Status500InternalServerError);
            }
 
            invitation.InvitedUser.ServiceRoleId = serviceRolesLookupModel.ServiceRoleId;
            invitation.InvitedUser.PersonRoleId = serviceRolesLookupModel.PersonRoleId;

            var sendInviteResponse = await _accountService.SaveInviteAsync(invitation);
            var inviteToken = await sendInviteResponse.Content.ReadAsStringAsync();

            var invitedUserEmail = invitation.InvitedUser.Email;
            if (sendInviteResponse.StatusCode == HttpStatusCode.BadRequest &&
                (inviteToken.Contains($"User '{invitedUserEmail}' is already invited") ||
                 inviteToken.Contains($"Invited user '{invitedUserEmail}' is enrolled already")))
            {
                _logger.LogError("User already invited");
                return Problem("User already invited", statusCode: StatusCodes.Status400BadRequest);
            }
            
            if (sendInviteResponse.StatusCode == HttpStatusCode.BadRequest &&
                inviteToken.Contains($"Invited user '{invitedUserEmail}' doesn't belong to the same organisation"))
            {
                _logger.LogError("Invited user doesn't belong to the same organisation");
                return Problem("Invited user doesn't belong to the same organisation", statusCode: StatusCodes.Status400BadRequest);
            }
            
            if (!sendInviteResponse.IsSuccessStatusCode || string.IsNullOrWhiteSpace(inviteToken))
            {
                _logger.LogError("Failed to save invitation");
                return Problem("Failed to save invitation", statusCode: StatusCodes.Status500InternalServerError);
            }

            var notificationId = _messagingService.SendInviteToUser(new InviteUserEmailInput()
            {
                UserId = User.UserId(),
                FirstName = invitation.InvitingUser.FirstName,
                LastName = invitation.InvitingUser.LastName,
                Recipient = invitedUserEmail,
                OrganisationId = invitation.InvitedUser.OrganisationId,
                OrganisationName = invitation.InvitedUser.OrganisationName,
                JoinTheTeamLink = $"{_messagingConfig.AccountCreationUrl}{HttpUtility.UrlEncode(inviteToken)}",
                TemplateId = serviceRolesLookupModel.InvitationTemplateId
            });

            return Ok(notificationId);
        }
        
        [HttpPost]
        [Route("enrol-invited-user")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnrolInvitedUser(EnrolInvitedUserModel enrolInvitedUserModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid request. Request model: {enrolInvitedUserModel}", JsonSerializer.Serialize(enrolInvitedUserModel));
                return Problem("Invalid Request", statusCode: StatusCodes.Status400BadRequest);
            }
        
            enrolInvitedUserModel.UserId = User.UserId();
            enrolInvitedUserModel.Email = User.Email();
    
            var enrolInvitedUserResponse = await _accountService.EnrolInvitedUserAsync(enrolInvitedUserModel);

            if (enrolInvitedUserResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogError("Failed to enrol user. Request model: {enrolInvitedUserModel}", JsonSerializer.Serialize(enrolInvitedUserModel));
                return Problem("Failed to enrol user", statusCode: StatusCodes.Status400BadRequest);
            }
    
            if (!enrolInvitedUserResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to enrol user. Request model: {enrolInvitedUserModel}", JsonSerializer.Serialize(enrolInvitedUserModel));
                return Problem("Failed to enrol user", statusCode: StatusCodes.Status500InternalServerError); 
            }
            
            return NoContent();
        }
    }
}