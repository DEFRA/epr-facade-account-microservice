using System.Diagnostics.CodeAnalysis;
using FacadeAccountCreation.Core.Models.CreateAccount;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class MemberDetailsModel
{
    public string OrganisationName { get; set; }
    public string OrganisationNumber { get; set; }
    public Nation RegisteredNation { get; set; }
    public string ComplianceScheme { get; set; }
    public ProducerType? ProducerType { get; set; }
    public string? CompanyHouseNumber { get; set; }
}