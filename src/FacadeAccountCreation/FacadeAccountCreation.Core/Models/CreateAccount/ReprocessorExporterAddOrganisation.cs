namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterAddOrganisation
{
    public ReprocessorExporterUserModel User { get; set; } = null!;

    [Required]
    public OrganisationModel Organisation { get; set; } = null!;

    public List<PartnerModel> Partners { get; set; } = new();

    public List<InvitedApprovedUserModel> InvitedApprovedUsers { get; set; } = [];

    public DateTime? DeclarationTimeStamp { get; set; }
}
