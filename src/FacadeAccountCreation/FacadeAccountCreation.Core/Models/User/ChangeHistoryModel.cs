using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.User;
[ExcludeFromCodeCoverage]
public class ChangeHistoryModel
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public int OrganisationId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public bool IsActive { get; set; }

    public string? ApproverComments { get; set; }

    public int? ApprovedById { get; set; }

    public DateTimeOffset? DecisionDate { get; set; }

    public DateTimeOffset DeclarationDate { get; set; }

    public Guid ExternalId { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset LastUpdatedOn { get; set; }

    public bool IsDeleted { get; set; }
}