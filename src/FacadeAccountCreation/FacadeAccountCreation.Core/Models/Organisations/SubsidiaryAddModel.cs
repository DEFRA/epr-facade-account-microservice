namespace FacadeAccountCreation.Core.Models.Organisations;

[ExcludeFromCodeCoverage]
public class SubsidiaryAddModel
{
    public Guid? ParentOrganisationId { get; init; }

    public Guid? ChildOrganisationId { get; init; }

    public Guid? UserId { get; set; }
}
