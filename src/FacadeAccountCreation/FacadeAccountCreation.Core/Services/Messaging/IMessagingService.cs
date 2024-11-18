using FacadeAccountCreation.Core.Models.Messaging;

namespace FacadeAccountCreation.Core.Services.Messaging;

public interface IMessagingService
{
    string? SendAccountCreationConfirmation(Guid userId, string firstName, string lastName, string recipient, string organisationNumber, Guid organisationId, bool companyIsComplianceScheme = false);
    string? SendInviteToUser(InviteUserEmailInput input);
    string? SendDelegatedUserNomination(NominateUserEmailInput input);
    string? SendDelegatedRoleRemovedNotification(DelegatedRoleEmailInput input);
    string? SendNominationCancelledNotification(DelegatedRoleEmailInput input);
    string? SendRemovedUserNotification(RemovedUserNotificationEmailModel input);
    List<string?> SendMemberDissociationRegulatorsNotification(MemberDissociationRegulatorsEmailInput input);
    List<string?> SendMemberDissociationProducersNotification(NotifyComplianceSchemeProducerEmailInput input);
    string? SendApprovedUserAccountCreationConfirmation(string companyName, string firstName, string lastName, string? organisationNumber, string recipient);
    string? SendPoMResubmissionEmailToRegulator(ResubmissionNotificationEmailInput input);
    string? SendUserDetailChangeRequestEmailToRegulator(UserDetailsChangeNotificationEmailInput input);

}