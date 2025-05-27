using System.Text;
using System.Text.RegularExpressions;
using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Models.Messaging;
using Notify.Interfaces;
using PersonRole = FacadeAccountCreation.Core.Models.Connections.PersonRole;

namespace FacadeAccountCreation.Core.Services.Messaging;

public class MessagingService(
    INotificationClient notificationClient,
    IOptions<MessagingConfig> messagingConfig,
    IOptions<RegulatorEmailConfig> regulatorEmailConfig,
    IOptions<EprPackagingRegulatorEmailConfig> eprPackagingRegulatorEmailConfig,
    ILogger<MessagingService> logger)
    : IMessagingService
{
    private readonly MessagingConfig _messagingConfig = messagingConfig.Value;
    private readonly RegulatorEmailConfig _regulatorEmailConfig = regulatorEmailConfig.Value;
    private readonly EprPackagingRegulatorEmailConfig _eprPackagingRegulatorEmailConfig = eprPackagingRegulatorEmailConfig.Value;
    private const int timeoutInSeconds = 60;
    private const string ExceptionLogMessage = "GOV UK NOTIFY ERROR. Method: SendEmail, Organisation ID: {OrganisationId}, User ID: {UserId}, Template: {TemplateId}";

    public string? SendPoMResubmissionEmailToRegulator(ResubmissionNotificationEmailInput input)
    {
        var nationLookup = new NationLookup();
        var nationName = nationLookup.GetNationName(input.NationId);
        var regulatorEmail = GetRegulatorEmail(nationName);

        var templateId = input.IsComplianceScheme ?
            _messagingConfig.ComplianceSchemeResubmissionTemplateId : _messagingConfig.ProducerResubmissionTemplateId;

        Dictionary<string, object> parameters = PopulateEmailParameters(input);

        string? notificationId = null;

        try
        {
            var response = notificationClient.SendEmail(regulatorEmail, templateId, parameters);
            notificationId = response.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GOV UK NOTIFY ERROR. Method: SendEmail, Nation ID: {NationId}, Organisation Number: {OrganisationNumber}, Template: {TemplateId}", input.NationId, input.OrganisationNumber, templateId);
        }

        return notificationId;
    }

    public string? SendAccountCreationConfirmation(
        Guid userId,
        string firstName,
        string lastName,
        string recipient,
        string organisationNumber,
        Guid organisationId,
        bool companyIsComplianceScheme = false)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("Cannot be empty string", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Cannot be empty string", nameof(lastName));
        }

        if (string.IsNullOrWhiteSpace(organisationNumber))
        {
            throw new ArgumentException("Cannot be empty string", nameof(organisationNumber));
        }

        var parameters = new Dictionary<string, object>();

        parameters.Add("firstName", firstName);
        parameters.Add("lastName", lastName);
        parameters.Add("organisationNumber", organisationNumber.ToReferenceNumberFormat());

        var templateId = companyIsComplianceScheme
            ? _messagingConfig.ComplianceSchemeAccountConfirmationTemplateId
            : _messagingConfig.ProducerAccountConfirmationTemplateId;

        string? notificationId = null;

        try
        {
            var response = notificationClient.SendEmail(recipient, templateId, parameters);
            notificationId = response.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, organisationId, userId, templateId);
        }

        return notificationId;
    }

    public string? SendInviteToUser(InviteUserEmailInput input)
    {
        if (input.UserId == Guid.Empty)
        {
            throw new ArgumentException("input.UserId cannot be empty guid", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.FirstName))
        {
            throw new ArgumentException("input.FirstName cannot be empty string", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.LastName))
        {
            throw new ArgumentException("input.LastName cannot be empty string", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.Recipient))
        {
            throw new ArgumentException("input.Recipient cannot be empty string", nameof(input));
        }
        if (input.OrganisationId == Guid.Empty)
        {
            throw new ArgumentException("input.OrganisationId cannot be empty guid", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.OrganisationName))
        {
            throw new ArgumentException("input.OrganisationName cannot be empty string", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.JoinTheTeamLink))
        {
            throw new ArgumentException("input.JoinTheTeamLink cannot be empty string", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.TemplateId))
        {
            throw new ArgumentException("input.TemplateId cannot be empty templateId", nameof(input));
        }

        var parameters = new Dictionary<string, object>
        {
            {"firstName", input.FirstName},
            {"lastName", input.LastName},
            {"organisationName", input.OrganisationName},
            {"joinTheTeamlink", input.JoinTheTeamLink}
        };

        string? notificationId = null;
        try
        {
            var response = notificationClient.SendEmail(input.Recipient, input.TemplateId, parameters);
            notificationId = response.id;
        }
        catch (Exception e)
        {
            logger.LogError(e, ExceptionLogMessage, input.OrganisationId, input.UserId, input.TemplateId);
        }

        return notificationId;
    }

    public string? SendDelegatedUserNomination(NominateUserEmailInput input)
    {
        var argumentExceptionMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(input.FirstName))
        {
            argumentExceptionMessage += "FirstName cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.LastName))
        {
            argumentExceptionMessage += "LastName cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.Recipient))
        {
            argumentExceptionMessage += "Recipient cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.OrganisationNumber))
        {
            argumentExceptionMessage += "OrganisationNumber cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.OrganisationName))
        {
            argumentExceptionMessage += "OrganisationName cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.NominatorFirstName))
        {
            argumentExceptionMessage += "NominatorFirstName cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.NominatorFirstName))
        {
            argumentExceptionMessage += "NominatorFirstName cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.TemplateId))
        {
            argumentExceptionMessage += "TemplateId cannot be empty string. ";
        }

        if (input.UserId == Guid.Empty)
        {
            argumentExceptionMessage += "UserId is required. ";
        }

        if (input.OrganisationId == Guid.Empty)
        {
            argumentExceptionMessage += "OrganisationId is required. ";
        }

        if (argumentExceptionMessage.Length > 0)
        {
            throw new ArgumentException(argumentExceptionMessage.TrimEnd(), nameof(input));
        }

        var parameters = new Dictionary<string, object>
        {
            { "firstName", input.FirstName },
            { "lastName", input.LastName },
            { "organisationNumber", input.OrganisationNumber.ToReferenceNumberFormat() },
            { "organisationName", input.OrganisationName },
            { "approvedPersonFirstName", input.NominatorFirstName },
            { "approvedPersonLastName", input.NominatorLastName },
            { "accountLoginUrl", input.AccountLoginUrl }
        };

        string? notificationId = null;

        try
        {
            var response = notificationClient.SendEmail(input.Recipient, _messagingConfig.NominateDelegatedUserTemplateId, parameters);
            notificationId = response.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, input.OrganisationId, input.UserId, _messagingConfig.NominateDelegatedUserTemplateId);
        }

        return notificationId;
    }

    public string? SendDelegatedRoleRemovedNotification(DelegatedRoleEmailInput input)
    {
        return SendDelegatedRoleNotification(input);
    }

    public string? SendNominationCancelledNotification(DelegatedRoleEmailInput input)
    {
        return SendDelegatedRoleNotification(input);
    }

    public string? SendRemovedUserNotification(RemovedUserNotificationEmailModel input)
    {
        var argumentExceptionMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(input.FirstName))
        {
            argumentExceptionMessage += "FirstName cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.LastName))
        {
            argumentExceptionMessage += "LastName cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.RecipientEmail))
        {
            argumentExceptionMessage += "Recipient Email cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.OrganisationId))
        {
            argumentExceptionMessage += "OrganisationId cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.CompanyName))
        {
            argumentExceptionMessage += "CompanyName cannot be empty string. ";
        }

        if (string.IsNullOrWhiteSpace(input.TemplateId))
        {
            argumentExceptionMessage += "TemplateId cannot be empty string. ";
        }

        if (input.UserId == Guid.Empty)
        {
            argumentExceptionMessage += "UserId is required. ";
        }

        if (string.IsNullOrEmpty(input.OrganisationId))
        {
            argumentExceptionMessage += "OrganisationId is required. ";
        }

        if (argumentExceptionMessage.Length > 0)
        {
            throw new ArgumentException(argumentExceptionMessage.TrimEnd(), nameof(input));
        }


        var parameters = new Dictionary<string, object>
        {
            { "firstName", input.FirstName },
            { "lastName", input.LastName },
            { "organisationNumber", input.OrganisationId.ToReferenceNumberFormat() },
            { "companyName", input.CompanyName }
        };

        string? notificationId = null;

        try
        {
            var response = notificationClient.SendEmail(input.RecipientEmail, _messagingConfig.RemovedUserNotificationTemplateId, parameters);
            notificationId = response.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, input.OrganisationId, input.UserId, _messagingConfig.RemovedUserNotificationTemplateId);
        }

        return notificationId;
    }

    public string? SendDelegatedRoleNotification(DelegatedRoleEmailInput input)
    {
        input.EnsureInitialised();

        var parameters = new Dictionary<string, object>
        {
            { "firstName", input.FirstName },
            { "lastName", input.LastName },
            { "organisationNumber", input.OrganisationNumber.ToReferenceNumberFormat() },
            { "organisationName", input.OrganisationName },
            { "approvedPersonFirstName", input.NominatorFirstName },
            { "approvedPersonLastName", input.NominatorLastName },
            { "levelOfAccountPermissions", input.PersonRole switch
                {
                    PersonRole.Admin => "manage the team and upload data",
                    PersonRole.Employee => "upload data",
                    _ => throw new NotImplementedException()
                }
            }
        };

        string? notificationId = null;

        try
        {
            var response = notificationClient.SendEmail(input.Recipient, input.TemplateId, parameters);

            notificationId = response.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, input.OrganisationId, input.UserId, input.TemplateId);
        }

        return notificationId;
    }

    public List<string>? SendMemberDissociationProducersNotification(NotifyComplianceSchemeProducerEmailInput input)
    {
        if (input.Recipients == null)
        {
            throw new ArgumentException("Cannot be empty string", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.OrganisationName))
        {
            throw new ArgumentException("input.OrganisationName cannot be empty string", nameof(input));
        }
        if (string.IsNullOrWhiteSpace(input.ComplianceScheme))
        {
            throw new ArgumentException("input.OrganisationName cannot be empty string", nameof(input));
        }

        List<string> notificationId = new List<string>();

        foreach (var recipient in input.Recipients)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"firstName", recipient.FirstName},
                    {"lastName",recipient.LastName},
                    {"organisationID", input.OrganisationId},
                    {"complianceScheme", input.ComplianceScheme},
                    {"organisationName", input.OrganisationName}
                };

                var response = notificationClient.SendEmail(recipient.Email, _messagingConfig.MemberDissociationProducersTemplateId, parameters);
                notificationId.Add(response.id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ExceptionLogMessage, input.OrganisationId, input.UserId, _messagingConfig.MemberDissociationRegulatorsTemplateId);
            }
        }
        return notificationId;
    }

    public List<string>? SendMemberDissociationRegulatorsNotification(MemberDissociationRegulatorsEmailInput input)
    {
        input.EnsureInitialised();

        var parameters = new Dictionary<string, object>
        {
            { "organisationName", input.OrganisationName },
            { "complianceScheme", input.ComplianceSchemeName },
            { "organisationID", input.OrganisationNumber }
        };

        List<string> notificationId = [];
        List<string> recipients = [];

        var complianceSchemeRegulatorEmail = GetRegulatorEmail(input.ComplianceSchemeNation);
        var organisationRegulatorEmail = GetRegulatorEmail(input.OrganisationNation);

        if (!string.IsNullOrEmpty(complianceSchemeRegulatorEmail))
        {
            recipients.Add(complianceSchemeRegulatorEmail);
        }

        if (complianceSchemeRegulatorEmail != organisationRegulatorEmail
            && !string.IsNullOrEmpty(organisationRegulatorEmail))
        {
            recipients.Add(organisationRegulatorEmail);
        }

        foreach (var recipient in recipients)
        {
            try
            {
                var response = notificationClient.SendEmail(recipient, _messagingConfig.MemberDissociationRegulatorsTemplateId, parameters);
                notificationId.Add(response.id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ExceptionLogMessage, input.OrganisationNumber, input.UserId, _messagingConfig.MemberDissociationRegulatorsTemplateId);
            }
        }
        return notificationId;
    }

    private string GetRegulatorEmail(string nation)
    {
        try
        {
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            nation = Regex.Replace(nation, @"\s", string.Empty, RegexOptions.None, timeout);
            switch (nation)
            {
                case "England":
                    return _regulatorEmailConfig.England.ToLower();
                case "Scotland":
                    return _regulatorEmailConfig.Scotland.ToLower();
                case "Wales":
                    return _regulatorEmailConfig.Wales.ToLower();
                case "NorthernIreland":
                    return _regulatorEmailConfig.NorthernIreland.ToLower();
                default:
                    throw new ArgumentException("Nation not valid");
            }
        }
        catch (RegexMatchTimeoutException ex)
        {
            logger.LogError(ex, "Regular Expression timeout out {Nation}", nation);
            throw new ArgumentException("Nation not valid");
        }
    }
    private string GetEprPackagingRegulatorEmail(string nation)
    {
        try
        {
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            nation = Regex.Replace(nation, @"\s", string.Empty, RegexOptions.None, timeout);
            switch (nation)
            {
                case "England":
                    return _eprPackagingRegulatorEmailConfig.England.ToLower();
                case "Scotland":
                    return _eprPackagingRegulatorEmailConfig.Scotland.ToLower();
                case "Wales":
                    return _eprPackagingRegulatorEmailConfig.Wales.ToLower();
                case "NorthernIreland":
                    return _eprPackagingRegulatorEmailConfig.NorthernIreland.ToLower();
                default:
                    throw new ArgumentException("Nation not valid");
            }
        }
        catch (RegexMatchTimeoutException ex)
        {
            logger.LogError(ex, "Regular Expression timeout out {Nation}", nation);
            throw new ArgumentException("Nation not valid");
        }
    }

    public string? SendApprovedUserAccountCreationConfirmation(
        string companyName,
        string firstName,
        string lastName,
        string? organisationNumber,
        string recipient)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException("Cannot be empty string", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("Cannot be empty string", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Cannot be empty string", nameof(lastName));
        }

        var parameters = new Dictionary<string, object>();
        parameters.Add("companyName", companyName);
        parameters.Add("firstName", firstName);
        parameters.Add("lastName", lastName);
        parameters.Add("organisationNumber", organisationNumber.ToReferenceNumberFormat());

        var templateId = _messagingConfig.ApprovedUserAccountConfirmationTemplateId;
        string? notificationId = null;

        try
        {
            var response = notificationClient.SendEmail(recipient, templateId, parameters);
            notificationId = response.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, organisationNumber, $"{firstName} {lastName}", templateId);
        }

        return notificationId;
    }

    public string? SendUserDetailChangeRequestEmailToRegulator(UserDetailsChangeNotificationEmailInput input)
    {
        var parameters = new Dictionary<string, object>();
        parameters.Add("firstName", input.OldFirstName);
        parameters.Add("lastName", input.OldLastName);
        parameters.Add("organisationName", input.OrganisationName);
        parameters.Add("organisationNumber", input.OrganisationNumber);
        parameters.Add("jobTitle", input.OldJobTitle);
        parameters.Add("updatedfirstName", input.NewFirstName);
        parameters.Add("updatedlastName", input.NewLastName);
        parameters.Add("updatedjobTitle", input.NewJobTitle);
        parameters.Add("email", input.ContactEmailAddress);
        parameters.Add("telephoneNumber", input.ContactTelephone);
        var templateId = _messagingConfig.UserDetailChangeRequestTemplateId;
        var recipient = GetEprPackagingRegulatorEmail(input.Nation);
        string? notificationId = null;
        try
        {
            var response = notificationClient.SendEmail(recipient, templateId, parameters);
            notificationId = response.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, input.OrganisationNumber, $"{input.OldFirstName} {input.OldLastName}", templateId);
        }
        return notificationId;
    }

    private static Dictionary<string, object> PopulateEmailParameters(ResubmissionNotificationEmailInput input)
    {
        if (input.IsComplianceScheme)
        {
            return new Dictionary<string, object>
            {
                { "complianceSchemePersonName", input.ComplianceSchemePersonName },
                { "regulatorOrganisationName", input.RegulatorOrganisationName },
                { "complianceSchemeName", input.ComplianceSchemeName },
                { "submissionPeriod", input.SubmissionPeriod },
                { "organisationNumber", input.OrganisationNumber },
            };
        }

        return new Dictionary<string, object>
        {
            { "producerName", input.ProducerOrganisationName },
            { "regulatorOrganisationName", input.RegulatorOrganisationName },
            { "submissionPeriod", input.SubmissionPeriod },
            { "organisationNumber", input.OrganisationNumber },
        };
    }

    #region Reprocessor and Exporter email notification

    /// <summary>
    /// Sends an invitation email to be approved person within an organisation with an invite link
    /// </summary>
    /// <param name="reExNotification"></param>
    /// <returns> list of response id with email id, or empty list if any exception</returns>
    /// <exception cref="ArgumentException"></exception>
    public List<(string email, string notificationResponseId)> SendReExInvitationToBeApprovedPerson(ReExNotificationModel reExNotification)
    {
        var notificationList = new List<(string email, string responseId)>();

        foreach (var member in reExNotification.ReExInvitedApprovedPersons)
        {
            if (string.IsNullOrWhiteSpace(member.FirstName))
            {
                throw new ArgumentException("First name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(member.LastName))
            {
                throw new ArgumentException("Last name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(member.Email))
            {
                throw new ArgumentException("email cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(reExNotification.OrganisationId))
            {
                throw new ArgumentException("Organisation Id cannot be empty");
            }

            var parameters = new Dictionary<string, object>
            {
                { "fullName", $"{member.FirstName} {member.LastName}" },
                { "organisationName", reExNotification.CompanyName },
                { "organisationId", reExNotification.OrganisationId },
                { "acceptRejectLink", member.InviteToken }
            };

            var templateId = _messagingConfig.ReExApprovedPersonInvitationTemplateId;

            try
            {
                var response = notificationClient.SendEmail(member.Email, templateId, parameters);
                notificationList.Add((member.Email, response.id));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ExceptionLogMessage, reExNotification.OrganisationId, reExNotification.UserId, templateId);
            }
        }
        return notificationList;
    }

    /// <summary>
    /// Send notification to inviter(s) that emails has been sent to relevant Approved Person(s)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userFullName"></param>
    /// <param name="userEmail"></param>
    /// <param name="organisationId"></param>
    /// <param name="invitedList"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public string? SendReExInvitationConfirmationToInviter(string userId, string userFullName, string userEmail, string organisationId, string organisationName, IEnumerable<(string email, string notificationResponseId)> invitedList)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(userFullName))
        {
            throw new ArgumentException("Full name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new ArgumentException("User email cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(organisationId))
        {
            throw new ArgumentException("Organisation Id cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(organisationName))
        {
            throw new ArgumentException("Organisation name cannot be empty");
        }

        var parameters = new Dictionary<string, object>
        {
            { "inviteeFullName", userFullName },
            { "organisationName", organisationName }
        };

        // IF HAS INVITED PERSON EMAILS
        StringBuilder invitedEmails = new();

        foreach (var invitedPerson in invitedList)
        {
            invitedEmails.AppendLine(invitedPerson.email);
        }

        if (invitedEmails.Length > 0)
        {
            parameters.Add("invitedPersonEmails", invitedEmails.ToString());
        }

        var templateId = _messagingConfig.ReExInvitationConfirmationToInviterTemplateId;

        try
        {
            return notificationClient.SendEmail(userEmail, templateId, parameters).id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, organisationId, userId, templateId);
            return null;
        }
    }

    /// <summary>
    /// Send confirmation to inviter that the Person invited to be an AP has accepted
    /// </summary>
    /// <param name="reExNotification"></param>
    /// <returns>response id or null if any exception</returns>
    public string? SendReExConfirmationOfAnApprovedPerson(ReExNotificationModel reExNotification)
    {
        if (string.IsNullOrWhiteSpace(reExNotification.UserFirstName))
        {
            throw new ArgumentException("First name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(reExNotification.UserLastName))
        {
            throw new ArgumentException("Last name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(reExNotification.OrganisationId))
        {
            throw new ArgumentException("Organisation Id cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(reExNotification.CompanyName))
        {
            throw new ArgumentException("Company name cannot be empty");
        }

        var parameters = new Dictionary<string, object>
        {
            { "fullName", $"{reExNotification.UserFirstName} {reExNotification.UserLastName}" },
            { "organisationName", reExNotification.CompanyName },
            { "organisationId", reExNotification.OrganisationId }
        };

        var templateId = _messagingConfig.ReExApprovedPersonAcceptedInvitationTemplateId;

        try
        {
            return notificationClient.SendEmail(reExNotification.UserEmail, templateId, parameters).id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, reExNotification.OrganisationId, reExNotification.UserId, templateId);
            return null;
        }
    }

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
    /// <exception cref="ArgumentException"></exception>
    public string? SendRejectionEmailFromInvitedAP(string userId, string userFullName, string userEmail, string organisationId, string organisationName, string rejectedByName)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(userFullName))
        {
            throw new ArgumentException("Full name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new ArgumentException("User email cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(organisationId))
        {
            throw new ArgumentException("Organisation Id cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(organisationName))
        {
            throw new ArgumentException("Organisation name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(rejectedByName))
        {
            throw new ArgumentException("Approved person email cannot be empty");
        }

        var parameters = new Dictionary<string, object>
        {
            { "inviteeFullName", userFullName },
            { "rejectedAPName", rejectedByName },
            { "organisationName", organisationName },
            { "organisationId", organisationId }
        };

        var templateId = _messagingConfig.ReExApprovedPersonRejectedInvitationTemplateId;

        try
        {
            var response = notificationClient.SendEmail(userEmail, templateId, parameters);
            return response.id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, organisationId, userId, templateId);
            return null;
        }
    }

    /// <summary>
    /// Email confirmation to invitee that they have rejected the AP invitation
    /// </summary>
    /// <param name="organisationId"></param>
    /// <param name="organisationName"></param>
    /// <param name="rejectedByName"></param>
    /// <param name="rejectedAPEmail"></param>
    /// <returns>response id</returns>
    /// <exception cref="ArgumentException"></exception>
    public string? SendRejectionConfirmationToApprovedPerson(string organisationId, string organisationName, string rejectedByName, string rejectedAPEmail)
    {
        if (string.IsNullOrWhiteSpace(rejectedByName))
        {
            throw new ArgumentException("Approved person name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(rejectedAPEmail))
        {
            throw new ArgumentException("Approved person email cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(organisationId))
        {
            throw new ArgumentException("Organisation Id cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(organisationName))
        {
            throw new ArgumentException("Organisation name cannot be empty");
        }

        var parameters = new Dictionary<string, object>
        {            
            { "organisationId", organisationId },
            { "organisationName", organisationName },
            { "rejectedAPName", rejectedByName }            
        };

        var templateId = _messagingConfig.ReExApprovedPersonRejectedInvitationTemplateId;

        try
        {
            return notificationClient.SendEmail(rejectedAPEmail, templateId, parameters).id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionLogMessage, organisationId, rejectedAPEmail, templateId);
            return null;
        }
    }

    #endregion
}