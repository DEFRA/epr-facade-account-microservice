namespace FacadeAccountCreation.Core.Models.CompaniesHouse;

public class OrganisationDto
{
    public string? Name { get; set; }

    public string? RegistrationNumber { get; set; }

    public AddressDto? RegisteredOffice { get; set; }

    public OrganisationDataDto? OrganisationData { get; set; }
}
