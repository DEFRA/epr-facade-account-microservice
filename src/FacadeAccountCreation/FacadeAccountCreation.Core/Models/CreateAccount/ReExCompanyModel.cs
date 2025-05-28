namespace FacadeAccountCreation.Core.Models.CreateAccount;
public class ReExCompanyModel
{
    public string? OrganisationId { get; set; }

    public string? OrganisationExternalId { get; set; }

    public OrganisationType? OrganisationType { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    public string CompanyName { get; set; }

    public AddressModel? CompanyRegisteredAddress { get; set; }

    public bool ValidatedWithCompaniesHouse { get; set; }

    public Nation? Nation { get; set; }

    public bool IsComplianceScheme { get; set; }
}
