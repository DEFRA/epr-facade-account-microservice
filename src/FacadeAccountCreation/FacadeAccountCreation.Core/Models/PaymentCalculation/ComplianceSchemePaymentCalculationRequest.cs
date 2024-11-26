namespace FacadeAccountCreation.Core.Models.PaymentCalculation;

[ExcludeFromCodeCoverage]
public class ComplianceSchemePaymentCalculationRequest
{
    public string Regulator { get; set; }
    public string ApplicationReferenceNumber { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }

    public List<ComplianceSchemePaymentCalculationRequestMember> ComplianceSchemeMembers { get; set; }
}