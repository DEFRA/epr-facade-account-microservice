using FacadeAccountCreation.Core.Models.Enrolments;
using FacadeAccountCreation.Core.Services.Enrolments;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/enrolments")]
public class EnrolmentsController(
    IEnrolmentService enrolmentService,
    ILogger<EnrolmentsController> logger,
    IPersonService personService,
    IOrganisationService organisationService,
    IMessagingService messagingService,
    IOptions<MessagingConfig> messagingConfig,
    IServiceRolesLookupService serviceRolesLookup)
    : ControllerBase
{
    private readonly MessagingConfig _messagingConfig = messagingConfig.Value;

    [HttpDelete]
    [Route("{personExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid personExternalId, Guid organisationId, int serviceRoleId)
    {
        if (personExternalId == Guid.Empty || organisationId == Guid.Empty || serviceRoleId == default)
        {
            var errorMessage = $"Invalid request to delete user {personExternalId} with service role {serviceRoleId}";
            logger.LogError("Invalid request to delete user {PersonExternalId} with service role {ServiceRoleId}", personExternalId, serviceRoleId);
            return Problem(errorMessage);
        }
        var deletedPersonToSendEmail = serviceRolesLookup.IsProducer(serviceRoleId)
                                                                        ? GetPersonDetailsToSendEmail(personExternalId, organisationId)
                                                                        : null;

        var response = await enrolmentService.DeleteUser(new DeleteUserModel
        {
            PersonExternalIdToDelete = personExternalId,
            LoggedInUserId = User.UserId(),
            OrganisationId = organisationId,
            ServiceRoleId = serviceRoleId
        });

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Deleted user {PersonExternalId} with service role {ServiceRoleId} successfully", personExternalId, serviceRoleId);

            if (deletedPersonToSendEmail != null)
            {
                var emailSent = SendNotificationEmailToDeletedPerson(deletedPersonToSendEmail.Result);
                if (emailSent)
                {
                    logger.LogError("Error sending the delete notification to user {FirstName} {LastName} for company {CompanyName}", deletedPersonToSendEmail.Result.FirstName, deletedPersonToSendEmail.Result.LastName, deletedPersonToSendEmail.Result.CompanyName);
                }
            }

            return NoContent();
        }

        logger.LogError("Error deleting user {PersonExternalId} with service role {ServiceRoleId}",personExternalId, serviceRoleId);
        return Problem($"Error deleting user {personExternalId} with service role {serviceRoleId}");
    }
    
    [HttpDelete]
    [Route("v1/{personExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePersonConnectionAndEnrolment([FromRoute] Guid personExternalId, Guid organisationId, int enrolmentId)
    {
        if (personExternalId == Guid.Empty || organisationId == Guid.Empty || enrolmentId == 0)
        {
            var errorMessage = $"Invalid request to delete user {personExternalId} with enrolmentId {enrolmentId}";
            logger.LogError("Invalid request to delete user {PersonExternalId} with enrolmentId {EnrolmentId}", personExternalId, enrolmentId);
            return Problem(errorMessage);
        }

        var response = await enrolmentService.DeletePersonConnectionAndEnrolment(new DeleteUserModel
        {
            PersonExternalIdToDelete = personExternalId,
            LoggedInUserId = User.UserId(),
            OrganisationId = organisationId,
            EnrolmentId = enrolmentId
        });

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Deleted user {PersonExternalId} with EnrolmentId {EnrolmentId} successfully", personExternalId, enrolmentId);
            
            return NoContent();
        }

        logger.LogError("Error deleting user {PersonExternalId} with EnrolmentId {EnrolmentId}",personExternalId, enrolmentId);
        return Problem($"Error deleting user {personExternalId} with EnrolmentId {enrolmentId}");
    }

    private async Task<RemovedUserNotificationEmailModel> GetPersonDetailsToSendEmail(Guid personExternalId, Guid organisationId)
    {
        var personResponse = await personService.GetPersonByExternalIdAsync(personExternalId);
        var organisationResponse = await organisationService.GetOrganisationByExternalId(organisationId);

        RemovedUserNotificationEmailModel personToBeDeleted = null;

        if (personResponse != null && organisationResponse != null)
        {
            personToBeDeleted = new RemovedUserNotificationEmailModel
            {
                UserId = User.UserId(),
                FirstName = personResponse.FirstName,
                LastName = personResponse.LastName,
                RecipientEmail = personResponse.ContactEmail,
                OrganisationId = organisationResponse.OrganisationNumber,
                CompanyName = organisationResponse.Name,
                TemplateId = _messagingConfig.NominateDelegatedUserTemplateId
            };
            return personToBeDeleted;
        }

        return personToBeDeleted;
    }

    private bool SendNotificationEmailToDeletedPerson(RemovedUserNotificationEmailModel deletedPersonToSendEmail)
    {
        return messagingService.SendRemovedUserNotification(deletedPersonToSendEmail) != null;
    }
}
