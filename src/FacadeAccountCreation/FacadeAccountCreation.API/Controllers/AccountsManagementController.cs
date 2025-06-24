using System.Text.Json;
using System.Web;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[RequiredScope("account-creation")]
[Route("api/accounts-management")]
public class AccountsManagementController(
    IAccountService accountService,
    IMessagingService messagingService,
    IOptions<MessagingConfig> messagingConfig,
    IServiceRolesLookupService serviceRolesLookupService,
    ILogger<AccountsManagementController> logger)
    : ControllerBase
{
    private readonly MessagingConfig _messagingConfig = messagingConfig.Value;

    [HttpPost]
    [Route("invite-user")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InviteUser(AccountInvitationModel invitation)
    {
        if (!ModelState.IsValid)
        {
            logger.LogError("Invalid request. Request model: {Invitation}", JsonSerializer.Serialize(invitation));
            return Problem("Invalid Request", statusCode: StatusCodes.Status400BadRequest);
        }

        invitation.InvitingUser.UserId = User.UserId();
        invitation.InvitingUser.Email = User.Email();
            
        var serviceRolesLookupModel = serviceRolesLookupService.GetServiceRoles().SingleOrDefault(x =>
            x.Key == invitation.InvitedUser.Rolekey && !string.IsNullOrWhiteSpace(x.InvitationTemplateId));

        if (serviceRolesLookupModel is null)
        {
            logger.LogError("Invalid role key: {Key}", invitation.InvitedUser.Rolekey);
            return Problem("Invalid role key", statusCode: StatusCodes.Status500InternalServerError);
        }
 
        invitation.InvitedUser.ServiceRoleId = serviceRolesLookupModel.ServiceRoleId;
        invitation.InvitedUser.PersonRoleId = serviceRolesLookupModel.PersonRoleId;

        var sendInviteResponse = await accountService.SaveInviteAsync(invitation);
        var inviteToken = await sendInviteResponse.Content.ReadAsStringAsync();

        var invitedUserEmail = invitation.InvitedUser.Email;
        if (sendInviteResponse.StatusCode == HttpStatusCode.BadRequest &&
            (inviteToken.Contains($"User '{invitedUserEmail}' is already invited") ||
             inviteToken.Contains($"Invited user '{invitedUserEmail}' is enrolled already")))
        {
            logger.LogError("User already invited");
            return Problem("User already invited", statusCode: StatusCodes.Status400BadRequest);
        }
            
        if (sendInviteResponse.StatusCode == HttpStatusCode.BadRequest &&
            inviteToken.Contains($"Invited user '{invitedUserEmail}' doesn't belong to the same organisation"))
        {
            logger.LogError("Invited user doesn't belong to the same organisation");
            return Problem("Invited user doesn't belong to the same organisation", statusCode: StatusCodes.Status400BadRequest);
        }
            
        if (!sendInviteResponse.IsSuccessStatusCode || string.IsNullOrWhiteSpace(inviteToken))
        {
            logger.LogError("Failed to save invitation");
            return Problem("Failed to save invitation", statusCode: StatusCodes.Status500InternalServerError);
        }

        var notificationId = messagingService.SendInviteToUser(new InviteUserEmailInput
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EnrolInvitedUser(EnrolInvitedUserModel enrolInvitedUserModel)
    {
        if (!ModelState.IsValid)
        {
            logger.LogError("Invalid request. Request model: {EnrolInvitedUserModel}", JsonSerializer.Serialize(enrolInvitedUserModel));
            return Problem("Invalid Request", statusCode: StatusCodes.Status400BadRequest);
        }
        
        enrolInvitedUserModel.UserId = User.UserId();
        enrolInvitedUserModel.Email = User.Email();
    
        var enrolInvitedUserResponse = await accountService.EnrolInvitedUserAsync(enrolInvitedUserModel);

        if (enrolInvitedUserResponse.StatusCode == HttpStatusCode.BadRequest)
        {
            logger.LogError("Failed to enrol user. Request model: {EnrolInvitedUserModel}", JsonSerializer.Serialize(enrolInvitedUserModel));
            return Problem("Failed to enrol user", statusCode: StatusCodes.Status400BadRequest);
        }
    
        if (!enrolInvitedUserResponse.IsSuccessStatusCode)
        {
            logger.LogError("Failed to enrol user. Request model: {EnrolInvitedUserModel}", JsonSerializer.Serialize(enrolInvitedUserModel));
            return Problem("Failed to enrol user", statusCode: StatusCodes.Status500InternalServerError); 
        }
            
        return NoContent();
    }
}