using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Subsidiary;

namespace FacadeAccountCreation.Core.Services.Organisation;

public interface IOrganisationService
{
    Task<HttpResponseMessage> GetOrganisationUserList(Guid userId, Guid organisationId, int serviceRoleId);
    
    Task<HttpResponseMessage> GetNationIdByOrganisationId(Guid organisationId);

    Task<RemovedUserOrganisationModel?> GetOrganisationByExternalId(Guid organisationExternalId);

    Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteToken(string token);

    Task<CheckRegulatorOrganisationExistResponseModel> GetRegulatorOrganisationByNationId(int nationId);

    Task<string?> CreateAndAddSubsidiaryAsync(LinkOrganisationModel linkOrganisationModel);

    Task<string?> AddSubsidiaryAsync(SubsidiaryAddModel subsidiaryAddModel);

    Task<OrganisationRelationshipModel> GetOrganisationRelationshipsByOrganisationId(Guid organisationExternalId);

    Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportOrganisationSubsidiaries(Guid organisationExternalId);
}
