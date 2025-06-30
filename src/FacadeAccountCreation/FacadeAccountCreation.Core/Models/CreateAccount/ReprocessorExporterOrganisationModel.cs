namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterOrganisationModel
{
    public OrganisationType OrganisationType { get; set; }

    public ProducerType? ProducerType { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(170)]
    public string? TradingName { get; set; }

    [Required]
    public AddressModel Address { get; set; } = null!;

    public bool ValidatedWithCompaniesHouse { get; set; }
    
    public bool IsComplianceScheme { get; set; }

    public Nation Nation { get; set; }

    public string? OrganisationId { get; set; } = null!;
}
