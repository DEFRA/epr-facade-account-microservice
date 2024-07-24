using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Models.Organisations;

namespace FacadeAccountCreation.Core.Services.Organisation;

public interface IOrganisationService
{
    Task<HttpResponseMessage> GetOrganisationUserList(Guid userId, Guid organisationId, int serviceRoleId);
    
    Task<HttpResponseMessage> GetNationIdByOrganisationId(Guid organisationId);

    /// <summary>
    /// Updates the nation id for an organisation
    /// </summary>
    /// <param name="organisationId">The id of the organisation</param>
    /// <param name="nationId">The id of the nation</param>
    /// <returns>Async task indicating success</returns>
    Task UpdateNationIdByOrganisationId(
        Guid userId,
        Guid organisationId,
        int nationId);

    Task<RemovedUserOrganisationModel?> GetOrganisationByExternalId(Guid organisationExternalId);

    Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteToken(string token);

    Task<CheckRegulatorOrganisationExistResponseModel> GetRegulatorOrganisationByNationId(int nationId);
}
