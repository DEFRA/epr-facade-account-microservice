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
    /// <param name="reExNotification">model with data for the email</param>
    /// <returns> list of response id with email id, or empty list if any exception</returns>
    List<(string email, string notificationResponseId)> SendReExInvitationToBeApprovedPerson(ReExNotificationModel reExNotification);

    /// <summary>
    /// Send notification to inviter(s) that emails has been sent to relevant Approved Person(s)
    /// </summary>
    /// <returns>response id or null if any issue</returns>
    string? SendReExInvitationConfirmationToInviter(string userId, string inviterFirstName, string inviterLastName, string inviterEmail, string organisationName, IEnumerable<(string email, string notificationResponseId)> invitedList);

    /// <summary>
    /// Send confirmation to inviter that the Person invited to be an AP has accepted
    /// </summary>
    /// <returns>response id or null if any exception</returns>
    string? SendReExConfirmationOfAnApprovedPerson(string userId, string inviterEmail, string inviteeFirstName, string inviteeLastName, string companyName, string inviterFirstName, string inviterLastName);

    /// <summary>
    /// Email to inviter that invitee has rejected AP invitation
    /// </summary>
    /// <returns>response id or null if any issue</returns>
    string? SendRejectionEmailFromInvitedAP(string userId, string inviterFullName, string inviterEmail, string organisationId, string organisationName, string rejectedByName);

    /// <summary>
    /// Email confirmation to invitee that they have rejected the AP invitation
    /// </summary>
    /// <returns>response id o rnull if any issue</returns>
    string? SendRejectionConfirmationToApprovedPerson(string userId, string organisationId, string organisationName, string rejectedByName, string rejectedAPEmail);

    /// <summary>
    /// Confirmation email for re-ex account creation
    /// </summary>
    string? SendReExAccountCreationConfirmation(string userId, string firstName, string lastName, string contactEmail);

    #endregion
}