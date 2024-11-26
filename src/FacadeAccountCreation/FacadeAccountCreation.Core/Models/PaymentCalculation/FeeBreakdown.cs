namespace FacadeAccountCreation.Core.Models.PaymentCalculation;

[ExcludeFromCodeCoverage]
public class FeeBreakdown
{
    public int BandNumber { get; set; }
    public int UnitCount { get; set; }
    public int UnitPrice { get; set; }
    public int TotalPrice { get; set; }
}
