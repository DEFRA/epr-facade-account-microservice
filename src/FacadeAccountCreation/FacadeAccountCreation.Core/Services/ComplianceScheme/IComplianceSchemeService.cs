using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.Organisations;

namespace FacadeAccountCreation.Core.Services.ComplianceScheme;

public interface IComplianceSchemeService
{
    Task<HttpResponseMessage> RemoveComplianceScheme(RemoveComplianceSchemeModel model);
    Task<HttpResponseMessage> SelectComplianceSchemeAsync(SelectSchemeWithUserModel model);
    Task<HttpResponseMessage> UpdateComplianceSchemeAsync(UpdateSchemeWithUserModel model);
    Task<HttpResponseMessage> GetAllComplianceSchemesAsync();
    Task<HttpResponseMessage> GetComplianceSchemeForProducerAsync(Guid organisationId, Guid userOid);
    Task<HttpResponseMessage> GetComplianceSchemesForOperatorAsync(Guid operatorOrganisationId);
    Task<HttpResponseMessage> GetComplianceSchemeMembersAsync(Guid userId, Guid organisationId, Guid selectedSchemeId, string? query, int pageSize, int page);
    Task<HttpResponseMessage> GetComplianceSchemeMemberDetailsAsync(Guid userId, Guid organisationId, Guid selectedSchemeId);
    Task<ComplianceSchemeSummary?> GetComplianceSchemesSummary(Guid complianceSchemeId, Guid organisationId, Guid userId);
    Task<HttpResponseMessage> GetAllReasonsForRemovalsAsync();
    Task<InfoForSelectedSchemeRemovalResponse> GetInfoForSelectedSchemeRemoval(Guid organisationId, Guid selectedSchemeId, Guid userId);
    Task<RemoveComplianceSchemeMemberResponse> RemoveComplianceSchemeMember(Guid organisationId, Guid selectedSchemeId, Guid userId, RemoveComplianceSchemeMemberModel model);
    Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportComplianceSchemeSubsidiaries(Guid userId, Guid organisationId, Guid complianceSchemeId);
}
