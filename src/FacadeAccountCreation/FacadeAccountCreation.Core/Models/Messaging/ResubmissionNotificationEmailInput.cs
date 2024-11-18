namespace FacadeAccountCreation.Core.Models.Messaging;

[ExcludeFromCodeCoverage]
public class ResubmissionNotificationEmailInput
{
    public string OrganisationNumber { get; set; }
    public string RegulatorOrganisationName { get; set; }
    public int NationId { get; set; }
    public string ProducerOrganisationName { get; set; }
    public string SubmissionPeriod { get; set; }
    public bool IsComplianceScheme { get; set; }
    public string ComplianceSchemeName { get; set; }
    public string ComplianceSchemePersonName { get; set; }
}