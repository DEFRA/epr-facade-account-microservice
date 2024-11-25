namespace FacadeAccountCreation.Core.Models.Messaging;

[ExcludeFromCodeCoverage]
public class NotifyComplianceSchemeProducerEmailInput
{
    public Guid UserId { get; set; }
    public List<EmailRecipient> Recipients { get; set; }
    public string OrganisationId { get; set; }
    public string OrganisationName { get; set; }
    public string ComplianceScheme { get; set; }
}
