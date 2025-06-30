namespace FacadeAccountCreation.Core.Models.Organisations;

[ExcludeFromCodeCoverage]
public class LinkOrganisationModel
{
    public ReprocessorExporterOrganisationModel Subsidiary { get; init; }
    public Guid ParentOrganisationId { get; init; }
    public Guid? UserId { get; set; }
}
