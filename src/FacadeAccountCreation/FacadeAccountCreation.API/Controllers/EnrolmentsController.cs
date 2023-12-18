using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.Core.Models.Enrolments;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Services.Enrolments;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.Core.Services.Organisation;
using FacadeAccountCreation.Core.Services.Person;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/enrolments")]
public class EnrolmentsController : Controller
{
    private readonly IEnrolmentService _enrolmentService;
    private readonly ILogger<EnrolmentsController> _logger;
    private readonly IPersonService _personService;
    private readonly IOrganisationService _organisationService;
    private readonly IMessagingService _messagingService;
    private readonly MessagingConfig _messagingConfig;
    private readonly IServiceRolesLookupService _serviceRolesLookup;
    
    public EnrolmentsController(
        IEnrolmentService enrolmentService, 
        ILogger<EnrolmentsController> logger,
        IPersonService personService,
        IOrganisationService organisationService,
        IMessagingService messagingService,
        IOptions<MessagingConfig> messagingConfig,
        IServiceRolesLookupService serviceRolesLookup)
    {
        _enrolmentService = enrolmentService;
        _logger = logger;
        _personService = personService;
        _organisationService = organisationService;
        _messagingService = messagingService;
        _messagingConfig = messagingConfig.Value;
        _serviceRolesLookup = serviceRolesLookup;
    }
    
    [HttpDelete]
    [Route("{personExternalId}")]
    public async Task<IActionResult> Delete([FromRoute]Guid personExternalId, Guid organisationId, int serviceRoleId)
    {
        if (personExternalId == default || organisationId == default || serviceRoleId == default)
        {
            var errorMessage = $"Invalid request to delete user {personExternalId} with service role {serviceRoleId}";
            _logger.LogError(errorMessage);
            return Problem(errorMessage);
        }
        var deletedPersonToSendEmail =  _serviceRolesLookup.IsProducer(serviceRoleId)
                                                                        ?  GetPersonDetailsToSendEmail(personExternalId, organisationId)
                                                                        : null;
        
        var response = await _enrolmentService.DeleteUser(new DeleteUserModel
        {
            PersonExternalIdToDelete = personExternalId,
            LoggedInUserId = User.UserId(),
            OrganisationId = organisationId,
            ServiceRoleId = serviceRoleId
        });

       if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Deleted user {PersonExternalId} with service role {ServiceRoleId} successfully", personExternalId, serviceRoleId);

            if (deletedPersonToSendEmail != null)
            {
                bool emailSent = SendNotificationEmailToDeletedPerson(deletedPersonToSendEmail.Result);
                if (emailSent)
                {
                    var errorMessage = $"Error sending the delete notification to user {deletedPersonToSendEmail.Result.FirstName } {deletedPersonToSendEmail.Result.LastName} " +
                                       $" for company {deletedPersonToSendEmail.Result.CompanyName}";
                    _logger.LogError(errorMessage);
                }
            }
            
            return NoContent();
        }
        else
        {
            var errorMessage = $"Error deleting user {personExternalId} with service role {serviceRoleId}";
            _logger.LogError(errorMessage);
            return Problem(errorMessage);
        }
    }
    
    private async Task<RemovedUserNotificationEmailModel> GetPersonDetailsToSendEmail(Guid personExternalId, Guid organisationId)
    {
        var personResponse = await _personService.GetPersonByExternalIdAsync(personExternalId);
        var organisationResponse = await _organisationService.GetOrganisationByExternalId(organisationId);

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
        return _messagingService.SendRemovedUserNotification(deletedPersonToSendEmail) != null;
    }
}
