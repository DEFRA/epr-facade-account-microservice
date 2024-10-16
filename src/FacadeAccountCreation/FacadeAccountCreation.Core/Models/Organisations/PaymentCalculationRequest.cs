using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.Organisations
{
    [ExcludeFromCodeCoverage]
    public class PaymentCalculationRequest
    {
        public string ProducerType { get; set; }
        public int NumberOfSubsidiaries { get; set; }
        public int Regulator { get; set; }
        public int NoOfSubsidiariesOnlineMarketplace { get; set; }
        public bool IsProducerOnlineMarketplace { get; set; }
        public string ApplicationReferenceNumber { get; set; }
    }
}
