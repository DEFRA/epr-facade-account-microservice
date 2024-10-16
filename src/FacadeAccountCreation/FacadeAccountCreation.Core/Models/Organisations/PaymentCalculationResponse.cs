using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.Organisations
{
    [ExcludeFromCodeCoverage]
    public class PaymentCalculationResponse
    {
        public decimal ProducerRegistrationFee { get; set; }
        public decimal ProducerOnlineMarketPlaceFee { get; set; }
        public decimal SubsidiariesFee { get; set; }
        public decimal TotalFee { get; set; }
        public decimal PreviousPayment { get; set; }
        public decimal OutstandingPayment { get; set; }
        public SubsidiariesFeeBreakdown SubsidiariesFeeBreakdown { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SubsidiariesFeeBreakdown
    {
        public decimal TotalSubsidiariesOMPFees { get; set; }
        public int CountOfOMPSubsidiaries { get; set; }
        public decimal UnitOMPFees { get; set; }
        public FeeBreakdown FeeBreakdowns { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FeeBreakdown
    {
        public int BandNumber { get; set; }
        public int UnitCount { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
