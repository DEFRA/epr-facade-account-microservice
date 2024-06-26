using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.User;
[ExcludeFromCodeCoverage]
public class OrganisationDetailModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string OrganisationRole { get; set; }
    
    public string OrganisationType { get; set; }
    
    public string OrganisationNumber { get; set; }

    public int? NationId { get; set; }
}