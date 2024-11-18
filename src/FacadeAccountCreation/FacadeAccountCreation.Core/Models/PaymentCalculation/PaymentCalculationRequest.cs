namespace FacadeAccountCreation.Core.Models.PaymentCalculation;

[ExcludeFromCodeCoverage]
public class PaymentCalculationRequest
{
    public string ProducerType { get; set; } = string.Empty;
    public int NumberOfSubsidiaries { get; set; }
    public string Regulator { get; set; } = string.Empty;
    public int NoOfSubsidiariesOnlineMarketplace { get; set; }
    public bool IsProducerOnlineMarketplace { get; set; }
    public bool IsLateFeeApplicable { get; set; }
    public string ApplicationReferenceNumber { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
}