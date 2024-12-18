namespace FacadeAccountCreation.API.Configs;

[ExcludeFromCodeCoverage]
public class ApiConfig
{
    public const string SectionName = "ApiConfig";

    public string AddressLookupBaseUrl { get; set; } = null!;

    public string CompaniesHouseLookupBaseUrl { get; set; } = null!;

    public string AccountServiceBaseUrl { get; set; } = null!;

    public string PayCalBaseUrl { get; set; } = null!;

    public string AccountServiceClientId { get; set; } = null!;

    public string Certificate { get; set; } = null!;

    public int Timeout { get; set; }

    public string TenantId { get; set; }

    public string Scope { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}
