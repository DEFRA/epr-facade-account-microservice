namespace FacadeAccountCreation.Core.Models.ServiceRolesLookup;

[ExcludeFromCodeCoverage]
public class ServiceRolesLookupModel
{
    public string Key { get; set; }
    
    public int ServiceRoleId { get; set; }

    public EnrolmentStatus EnrolmentStatus { get; set; }
    
    public int PersonRoleId { get; set; }
    
    public string? InvitationTemplateId { get; set; }

    public string? DescriptionKey { get; set; }
}
