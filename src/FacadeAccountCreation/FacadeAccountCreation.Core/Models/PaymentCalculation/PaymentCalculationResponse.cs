namespace FacadeAccountCreation.Core.Models.PaymentCalculation;

[ExcludeFromCodeCoverage]
public class PaymentCalculationResponse
{
    public int ProducerRegistrationFee { get; set; }
    public int ProducerOnlineMarketPlaceFee { get; set; }
    public int ProducerLateRegistrationFee { get; set; }
    public int SubsidiariesFee { get; set; }
    public int TotalFee { get; set; }
    public int PreviousPayment { get; set; }
    public int OutstandingPayment { get; set; }
    public SubsidiariesFeeBreakdown SubsidiariesFeeBreakdown { get; set; }
}