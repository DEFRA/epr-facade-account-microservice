using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.Organisations;

[ExcludeFromCodeCoverage]
public class SubsidiaryOrganisationModel
{
    public int OrganisationId { get; set; }

    [MaxLength(4000)]
    public string SubsidiaryId { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset LastUpdatedOn { get; set; }

    public Guid UserId { get; set; }
}
