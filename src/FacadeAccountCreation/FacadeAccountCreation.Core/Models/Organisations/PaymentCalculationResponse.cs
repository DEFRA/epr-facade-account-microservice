using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.Organisations
{
    [ExcludeFromCodeCoverage]
    public class PaymentCalculationResponse
    {
        public int ProducerRegistrationFee { get; set; }
        public int ProducerOnlineMarketPlaceFee { get; set; }
        public int SubsidiariesFee { get; set; }
        public int TotalFee { get; set; }
        public int PreviousPayment { get; set; }
        public int OutstandingPayment { get; set; }
        public SubsidiariesFeeBreakdown SubsidiariesFeeBreakdown { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SubsidiariesFeeBreakdown
    {
        public int TotalSubsidiariesOMPFees { get; set; }
        public int CountOfOMPSubsidiaries { get; set; }
        public int UnitOMPFees { get; set; }
        public List<FeeBreakdown> FeeBreakdowns { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FeeBreakdown
    {
        public int BandNumber { get; set; }
        public int UnitCount { get; set; }
        public int UnitPrice { get; set; }
        public int TotalPrice { get; set; }
    }
}
