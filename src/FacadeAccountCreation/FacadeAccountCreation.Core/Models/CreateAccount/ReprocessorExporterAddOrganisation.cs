namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterAddOrganisation
{
    public ReprocessorExporterUserModel User { get; set; } = null!;

    [Required]
    public OrganisationModel? Organisation { get; set; } = null; // To DO: need this null if manual input got values

    public ReExManualInputModel? ManualInput { get; set; } // To DoL: need this null if Organisation has company data

    public List<PartnerModel> Partners { get; set; } = [];

    public List<InvitedApprovedUserModel> InvitedApprovedUsers { get; set; } = [];

    public DateTime? DeclarationTimeStamp { get; set; }
}
