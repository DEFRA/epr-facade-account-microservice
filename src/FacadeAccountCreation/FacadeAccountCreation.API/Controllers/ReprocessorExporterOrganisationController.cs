using FacadeAccountCreation.Core.Models.Messaging;
using System.Web;
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

        // Send Email Notification        
        var notificationModelMapper = ReExNotificationMapper.MapOrganisationModelToReExNotificationModel(reExOrganisationModel, response, messagingConfig.Value.ReExAccountCreationUrl);

        // TO DO: check if Invited user 'email' is enrolled already
        var notifications = messagingService.SendReExInvitationToBeApprovedPerson(notificationModelMapper); 

        // send email notification to inviter(s)       
        var notifyInviter = messagingService.SendReExInvitationConfirmationToInviter(
            reExOrganisationModel.ReExUser.UserId.ToString(),
            $"{response.UserFirstName} {response.UserLastName}",
            reExOrganisationModel.ReExUser.UserEmail, 
            response.OrganisationId.ToString(), 
            reExOrganisationModel.Company.CompanyName, 
            notifications);

        return Ok(response.OrganisationId); // TO DO - need to return more data
    }
}
