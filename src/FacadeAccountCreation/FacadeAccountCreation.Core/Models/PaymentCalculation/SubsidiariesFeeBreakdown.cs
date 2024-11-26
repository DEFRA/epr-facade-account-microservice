namespace FacadeAccountCreation.Core.Models.PaymentCalculation;

[ExcludeFromCodeCoverage]
public class SubsidiariesFeeBreakdown
{
    public int TotalSubsidiariesOMPFees { get; set; }
    public int CountOfOMPSubsidiaries { get; set; }
    public int UnitOMPFees { get; set; }
    public FeeBreakdown[] FeeBreakdowns { get; set; }
}
