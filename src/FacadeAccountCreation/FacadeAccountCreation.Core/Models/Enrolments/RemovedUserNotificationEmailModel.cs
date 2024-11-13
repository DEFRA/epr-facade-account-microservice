namespace FacadeAccountCreation.Core.Models.Enrolments;

[ExcludeFromCodeCoverage]
public class RemovedUserNotificationEmailModel
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string RecipientEmail { get; set; } = default!;
    public string OrganisationId { get; set; }
    
    public string CompanyName { get; set; } = default!;
    
    public string TemplateId { get; set; }
}