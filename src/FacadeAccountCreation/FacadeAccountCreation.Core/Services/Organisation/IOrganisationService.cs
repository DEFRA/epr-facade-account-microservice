using FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;

namespace FacadeAccountCreation.Core.Services.Organisation;

public interface IOrganisationService
{
    Task<HttpResponseMessage> GetOrganisationUserList(Guid userId, Guid organisationId, int serviceRoleId);

    Task<HttpResponseMessage> GetOrganisationAllUsersList(Guid userId, Guid organisationId, int serviceRoleId);

    Task<HttpResponseMessage> GetNationIdByOrganisationId(Guid organisationId);

    /// <summary>
    /// Updates the details of an organisation
    /// </summary>
    /// <param name="organisationId">The id of the organisation</param>
    /// <param name="organisationDetails">The new organisation details for the organisation</param>
    /// <returns>Async task indicating success</returns>
    Task UpdateOrganisationDetails(
        Guid userId,
        Guid organisationId,
        OrganisationUpdateDto organisationDetails);

    Task<RemovedUserOrganisationModel?> GetOrganisationByExternalId(Guid organisationExternalId);

    Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteToken(string token);

    Task<OrganisationDto> GetOrganisationByReferenceNumber(string referenceNumber);

    Task<CheckRegulatorOrganisationExistResponseModel> GetRegulatorOrganisationByNationId(int nationId);

    Task<string?> CreateAndAddSubsidiaryAsync(LinkOrganisationModel linkOrganisationModel);

    Task<string?> AddSubsidiaryAsync(SubsidiaryAddModel subsidiaryAddModel);

    Task TerminateSubsidiaryAsync(SubsidiaryTerminateModel subsidiaryTerminateModel);

    Task<PagedOrganisationRelationshipsModel> GetPagedOrganisationRelationships(int page, int showPerPage, string search = null);

    Task<List<RelationshipResponseModel>> GetUnpagedOrganisationRelationships();

    Task<OrganisationRelationshipModel> GetOrganisationRelationshipsByOrganisationId(Guid organisationExternalId);

    Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportOrganisationSubsidiaries(Guid organisationExternalId);

    Task<string> GetOrganisationNationCodeByExternalIdAsync(Guid organisationExternalId);

    Task<OrganisationDto> GetOrganisationByCompanyHouseNumber(string companyHouseNumber);

    Task<ReExAddOrganisationResponse?> CreateReExOrganisationAsync(ReprocessorExporterAddOrganisation reExOrganisation, string serviceKey);
}
