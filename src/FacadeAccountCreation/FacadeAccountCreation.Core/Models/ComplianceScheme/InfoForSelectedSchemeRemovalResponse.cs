
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class InfoForSelectedSchemeRemovalResponse
{
    public string ComplianceSchemeName { get; set; }
    public string ComplianceSchemeNation { get; set; }
    public string OrganisationName { get; set; }
    public string OrganisationNation { get; set; }
    public string? OrganisationNumber { get; set; }
    public List<EmailRecipient> RemovalNotificationRecipients { get; set; }
}