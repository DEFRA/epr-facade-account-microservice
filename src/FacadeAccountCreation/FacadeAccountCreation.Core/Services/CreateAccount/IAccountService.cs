namespace FacadeAccountCreation.Core.Services.CreateAccount;

public interface IAccountService
{
    Task<CreateAccountResponse?> AddAccountAsync(AccountWithUserModel accountWithUser);
    Task AddReprocessorExporterAccountAsync(ReprocessorExporterAccountWithUserModel accountWithUser, string serviceKey);
    Task<IReadOnlyCollection<OrganisationResponseModel>?> GetOrganisationsByCompanyHouseNumberAsync(string companiesHouseNumber);
    Task<HttpResponseMessage> SaveInviteAsync(AccountInvitationModel accountInvitation);
    Task<HttpResponseMessage> EnrolInvitedUserAsync(EnrolInvitedUserModel enrolInvitedUserModel);
    Task<CreateAccountResponse?> AddApprovedUserAccountAsync(AccountModel approvedUser);
}
