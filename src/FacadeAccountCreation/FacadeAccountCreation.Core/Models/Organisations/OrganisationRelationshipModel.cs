using FacadeAccountCreation.Core.Models.User;
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.Organisations;

[ExcludeFromCodeCoverage]
public class OrganisationRelationshipModel
{
    public OrganisationDetailModel Organisation { get; set; }

    public List<RelationshipResponseModel> Relationships { get; set; } = null!;
}