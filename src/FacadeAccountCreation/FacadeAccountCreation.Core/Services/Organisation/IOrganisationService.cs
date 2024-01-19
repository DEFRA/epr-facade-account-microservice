using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Models.Organisations;

namespace FacadeAccountCreation.Core.Services.Organisation;

public interface IOrganisationService
{
    Task<HttpResponseMessage> GetOrganisationUserList(Guid userId, Guid organisationId, int serviceRoleId);
    
    Task<HttpResponseMessage> GetNationIdByOrganisationId(Guid organisationId);

    Task<RemovedUserOrganisationModel?> GetOrganisationByExternalId(Guid organisationExternalId);

    Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteToken(string token);

    Task<CheckRegulatorOrganisationExistResponseModel> GetRegulatorOrganisationByNationId(int nationId);
}
