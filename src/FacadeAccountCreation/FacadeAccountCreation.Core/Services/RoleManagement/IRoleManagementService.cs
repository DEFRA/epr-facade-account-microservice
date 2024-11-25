using FacadeAccountCreation.Core.Models.DelegatedPerson;

namespace FacadeAccountCreation.Core.Services.RoleManagement;

public interface IRoleManagementService
{
    Task<ConnectionPersonModel> GetPerson(Guid connectionId, string serviceKey, Guid userId, Guid organisationId);

    Task<ConnectionWithEnrolmentsModel> GetEnrolments(Guid connectionId, string serviceKey, Guid userId, Guid organisationId);

    Task<UpdatePersonRoleResponse> UpdatePersonRole(Guid connectionId, Guid userId, Guid organisationId, string serviceKey, UpdatePersonRoleRequest updateRequest);

    /// <summary>
    /// Approved Person (userId) from organisation (organisationId) nominates a person in the same organisation (connectionId) to become Delegated Person.
    /// </summary>
    Task<HttpResponseMessage> NominateToDelegatedPerson(Guid connectionId, Guid userId, Guid organisationId, string serviceKey, DelegatedPersonNominationRequest nominationRequest);
        
    /// <summary>
    /// User (userId) from organisation (organisationId) updates their EnrolmentState from Nominated to Pending.
    /// </summary>
    Task<HttpResponseMessage> AcceptNominationToDelegatedPerson(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey, AcceptNominationRequest acceptNominationRequest);

    /// <summary>
    /// Nominated approved person with user (userId) from organisation (organisationId) updates their EnrolmentState from Nominated to Pending.
    /// </summary>
    Task<HttpResponseMessage> AcceptNominationForApprovedPerson(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey, AcceptNominationApprovedPersonRequest acceptNominationRequest);

    Task<DelegatedPersonNominatorModel> GetDelegatedPersonNominator(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey);
}