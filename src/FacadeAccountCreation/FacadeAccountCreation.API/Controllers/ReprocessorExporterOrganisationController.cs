using FacadeAccountCreation.Core.Models.Organisations.Mappers;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/v1/reprocessor-exporter-org")]
public class ReprocessorExporterOrganisationController(
    IOrganisationService organisationService, 
    IMessagingService messagingService, 
    IOptions<MessagingConfig> messagingConfig, 
    ILogger<ReprocessorExporterOrganisationController> logger) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateReExAccount(ReExOrganisationModel reExOrganisationModel, [BindRequired, FromQuery] string serviceKey)
    {
        reExOrganisationModel.ReExUser.UserEmail = User.Email();
        reExOrganisationModel.ReExUser.UserId = User.UserId();
        var addOrganisationMapper = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(reExOrganisationModel);

        // post data to BackEndService and get related response object
        var response = await organisationService.CreateReExOrganisationAsync(addOrganisationMapper, serviceKey);

        if (response is null)
        {
            logger.LogError("Response can not be null");
            return Problem("Response can not be null", statusCode: StatusCodes.Status204NoContent);
        }

        if (response.OrganisationId  == Guid.Empty)
        {
            logger.LogError("Organisation id can not be empty");
            return Problem("Organisation id can not be empty", statusCode: StatusCodes.Status204NoContent);
        }

        // Send Email Notification(s)        
        var emailNotificationMapper = ReExNotificationMapper.MapOrganisationModelToReExNotificationModel(reExOrganisationModel, response, messagingConfig.Value.ReExAccountCreationUrl);

        // TO DO: check if Invited approved person 'email' is enrolled already
        if (emailNotificationMapper.ReExInvitedApprovedPersons.Count != 0)
        {
            var notificationResponse = messagingService.SendReExInvitationToBeApprovedPerson(emailNotificationMapper);

            if (notificationResponse.Count > 0)
            {
                // send acknowledgement to the inviter that AP invitation has been sent      
                messagingService.SendReExInvitationConfirmationToInviter(
                    emailNotificationMapper.UserId.ToString(),
                    emailNotificationMapper.UserFirstName,
                    emailNotificationMapper.UserLastName,
                    emailNotificationMapper.UserEmail,
                    emailNotificationMapper.CompanyName,
                    notificationResponse);
            }
        }

        return Ok(response.OrganisationId);
    }
}
