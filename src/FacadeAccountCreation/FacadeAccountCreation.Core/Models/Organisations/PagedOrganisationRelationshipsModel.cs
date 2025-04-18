namespace FacadeAccountCreation.Core.Models.Organisations;

using System.Collections.Generic;

[ExcludeFromCodeCoverage]
public class PagedOrganisationRelationshipsModel
{
    public List<RelationshipResponseModel> Items { get; set; }
    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public List<string> SearchTerms { get; set; }
}