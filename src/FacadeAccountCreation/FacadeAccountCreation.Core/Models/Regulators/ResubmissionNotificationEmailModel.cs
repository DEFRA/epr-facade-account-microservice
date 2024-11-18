namespace FacadeAccountCreation.Core.Models.Regulators;

[ExcludeFromCodeCoverage]
public class ResubmissionNotificationEmailModel
{
    [Required]
    public int NationId { get; set; }
    public string ProducerOrganisationName { get; set; }
    public string OrganisationNumber { get; set; }
    public string SubmissionPeriod { get; set; }
    [Required]
    public bool IsComplianceScheme { get; set; }
    public string ComplianceSchemeName { get; set; }
    public string ComplianceSchemePersonName { get; set; }

}