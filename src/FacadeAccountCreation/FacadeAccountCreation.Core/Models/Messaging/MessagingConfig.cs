namespace FacadeAccountCreation.Core.Models.Messaging;

public class MessagingConfig
{
    public const string SectionName = "MessagingConfig";
    public string ProducerAccountConfirmationTemplateId { get; set; } = string.Empty;
    public string ComplianceSchemeAccountConfirmationTemplateId { get; set; } = string.Empty;
    public string DelegatedRoleRemovedTemplateId { get; set; } = string.Empty;
    public string NominateDelegatedUserTemplateId { get; set; } = string.Empty;
    public string NominationCancelledTemplateId { get; set; } = string.Empty;
    public string MemberDissociationRegulatorsTemplateId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string AccountCreationUrl { get; set; } = string.Empty;
    public string RemovedUserNotificationTemplateId { get; set; } = string.Empty;
    public string MemberDissociationProducersTemplateId { get; set; } = string.Empty;
    public string ApprovedUserAccountConfirmationTemplateId { get; set; } = string.Empty;
    public string ProducerResubmissionTemplateId { get; set; } = string.Empty;
    public string ComplianceSchemeResubmissionTemplateId { get; set; } = string.Empty;
    public string AccountLoginUrl { get; set; }
}