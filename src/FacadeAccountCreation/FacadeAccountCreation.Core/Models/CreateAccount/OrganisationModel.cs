using System.ComponentModel.DataAnnotations;

namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class OrganisationModel
{
    public OrganisationType OrganisationType { get; set; }

    public ProducerType? ProducerType { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public AddressModel Address { get; set; } = null!;

    public bool ValidatedWithCompaniesHouse { get; set; }
    
    public bool IsComplianceScheme { get; set; }

    public Nation Nation { get; set; }

    public string? OrganisationId { get; set; } = null!;
}
