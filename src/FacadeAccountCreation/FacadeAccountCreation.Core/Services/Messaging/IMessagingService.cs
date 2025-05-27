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

    #region Reprocessor and Exporter email notifications

    /// <summary>
    /// Sends an invitation email to be approved person within an organisation with an invite link
    /// </summary>
    /// <param name="reExNotification"></param>
    /// <returns> list of response id with email id, or empty list if any exception</returns>
    List<(string email, string notificationResponseId)> SendReExInvitationToBeApprovedPerson(ReExNotificationModel reExNotification);

    /// <summary>
    /// Send notification to inviter(s) that emails has been sent to relevant Approved Person(s)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userFullName"></param>
    /// <param name="userEmail"></param>
    /// <param name="organisationId"></param>
    /// <param name="invitedList"></param>
    /// <returns></returns>
    string? SendReExInvitationConfirmationToInviter(string userId, string userFullName, string userEmail, string organisationId, string organisationName, IEnumerable<(string email, string notificationResponseId)> invitedList);

    /// <summary>
    /// Send confirmation to inviter that the Person invited to be an AP has accepted
    /// </summary>
    /// <param name="reExNotification"></param>
    /// <returns>response id or null if any exception</returns>
    string? SendReExConfirmationOfAnApprovedPerson(ReExNotificationModel reExNotification);

    /// <summary>
    /// Email to inviter that invitee has rejected AP invitation
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userFullName"></param>
    /// <param name="userEmail"></param>
    /// <param name="organisationId"></param>
    /// <param name="organisationName"></param>
    /// <param name="rejectedByName"></param>
    /// <returns></returns>
    string? SendRejectionEmailFromInvitedAP(string userId, string userFullName, string userEmail, string organisationId, string organisationName, string rejectedByName);

    /// <summary>
    /// Email confirmation to invitee that they have rejected the AP invitation
    /// </summary>
    /// <param name="organisationId"></param>
    /// <param name="organisationName"></param>
    /// <param name="rejectedByName"></param>
    /// <param name="rejectedAPEmail"></param>
    /// <returns>response id</returns>
    string? SendRejectionConfirmationToApprovedPerson(string organisationId, string organisationName, string rejectedByName, string rejectedAPEmail);

    #endregion
}