using System.Diagnostics.CodeAnalysis;
using FacadeAccountCreation;
using FacadeAccountCreation.Core;
using FacadeAccountCreation.Core.Models;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.PaymentCalculation;

namespace FacadeAccountCreation.Core.Models.PaymentCalculation
{
    [ExcludeFromCodeCoverage]
    public class PaymentCalculationRequest
    {
        public string ProducerType { get; set; }
        public int NumberOfSubsidiaries { get; set; }
        public string Regulator { get; set; }
        public int NoOfSubsidiariesOnlineMarketplace { get; set; }
        public bool IsProducerOnlineMarketplace { get; set; }
        public bool IsLateFeeApplicable { get; set; }
        public string ApplicationReferenceNumber { get; set; }
    }
}
