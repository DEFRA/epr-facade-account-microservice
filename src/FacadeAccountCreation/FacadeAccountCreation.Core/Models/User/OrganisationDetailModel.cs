namespace FacadeAccountCreation.Core.Models.User;

public class OrganisationDetailModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string OrganisationRole { get; set; }
    
    public string OrganisationType { get; set; }
    
    public string OrganisationNumber { get; set; }
}