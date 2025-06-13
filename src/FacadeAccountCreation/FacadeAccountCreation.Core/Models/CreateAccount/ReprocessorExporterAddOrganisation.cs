namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterAddOrganisation
{
    public ReprocessorExporterUserModel User { get; set; } = null!;

    public OrganisationModel Organisation { get; set; } = null!;

    public List<PartnerModel> Partners { get; set; } = [];

    public List<InvitedApprovedUserModel> InvitedApprovedUsers { get; set; } = [];

    public DateTime? DeclarationTimeStamp { get; set; }
}
