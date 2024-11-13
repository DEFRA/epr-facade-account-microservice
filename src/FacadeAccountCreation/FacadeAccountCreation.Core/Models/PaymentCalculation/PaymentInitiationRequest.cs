using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.PaymentCalculation;

[ExcludeFromCodeCoverage]
public class PaymentInitiationRequest
{
    public Guid UserId { get; set; }
    public Guid OrganisationId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Regulator { get; set; } = string.Empty;
    public int Amount { get; set; }
}