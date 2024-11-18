namespace FacadeAccountCreation.Core.Models.ServiceRolesLookup;

[ExcludeFromCodeCoverage]
public class ServiceRolesConfig
{
    public const string SectionName = "RolesConfig";
    
    public List<ServiceRolesLookupModel> Roles { get; set; }
}