﻿using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.Enrolments;
using FacadeAccountCreation.Core.Models.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using Notify.Models;
using System.Text.RegularExpressions;

namespace FacadeAccountCreation.Core.Services.Messaging
{
    public class MessagingService : IMessagingService
    {
        private readonly INotificationClient _notificationClient;
        private readonly MessagingConfig _messagingConfig;
        private readonly RegulatorEmailConfig _regulatorEmailConfig;
        private readonly EprPackagingRegulatorEmailConfig _eprPackagingRegulatorEmailConfig;
        private readonly ILogger<MessagingService> _logger;
        private const int timeoutInSeconds = 60;
        private const string ExceptionLogMessage = "GOV UK NOTIFY ERROR. Method: SendEmail, Organisation ID: {OrganisationId}, User ID: {UserId}, Template: {TemplateId}";

        public MessagingService(INotificationClient notificationClient, IOptions<MessagingConfig> messagingConfig,
            IOptions<RegulatorEmailConfig> regulatorEmailConfig,
            IOptions<EprPackagingRegulatorEmailConfig> eprPackagingRegulatorEmailConfig,
            ILogger<MessagingService> logger)
        {
            _notificationClient = notificationClient;
            _messagingConfig = messagingConfig.Value;
            _regulatorEmailConfig = regulatorEmailConfig.Value;
            _eprPackagingRegulatorEmailConfig = eprPackagingRegulatorEmailConfig.Value;
            _logger = logger;
        }

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
                var response = _notificationClient.SendEmail(regulatorEmail, templateId, parameters);
                notificationId = response.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GOV UK NOTIFY ERROR. Method: SendEmail, Nation ID: {NationId}, Organisation Number: {OrganisationNumber}, Template: {templateId}", input.NationId, input.OrganisationNumber, templateId);
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
                var response = _notificationClient.SendEmail(recipient, templateId, parameters);
                notificationId = response.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionLogMessage, organisationId, userId, templateId);
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
                var response = _notificationClient.SendEmail(input.Recipient, input.TemplateId, parameters);
                notificationId = response.id;
            }
            catch (Exception e)
            {
                _logger.LogError(e, ExceptionLogMessage, input.OrganisationId, input.UserId, input.TemplateId);
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
                var response = _notificationClient.SendEmail(input.Recipient, _messagingConfig.NominateDelegatedUserTemplateId, parameters);
                notificationId = response.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionLogMessage, input.OrganisationId, input.UserId, _messagingConfig.NominateDelegatedUserTemplateId);
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
                var response = _notificationClient.SendEmail(input.RecipientEmail, _messagingConfig.RemovedUserNotificationTemplateId, parameters);
                notificationId = response.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionLogMessage, input.OrganisationId, input.UserId, _messagingConfig.RemovedUserNotificationTemplateId);
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
                        Models.Connections.PersonRole.Admin => "manage the team and upload data",
                        Models.Connections.PersonRole.Employee => "upload data",
                        _ => throw new NotImplementedException()
                    }
                }
            };

            string? notificationId = null;

            try
            {
                var response = _notificationClient.SendEmail(input.Recipient, input.TemplateId, parameters);

                notificationId = response.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionLogMessage, input.OrganisationId, input.UserId, input.TemplateId);
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

                    var response = _notificationClient.SendEmail(recipient.Email, _messagingConfig.MemberDissociationProducersTemplateId, parameters);
                    notificationId.Add(response.id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ExceptionLogMessage, input.OrganisationId, input.UserId, _messagingConfig.MemberDissociationRegulatorsTemplateId);
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

            List<string> notificationId = new List<string>();
            List<string> recipients = new();

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
                    var response = _notificationClient.SendEmail(recipient, _messagingConfig.MemberDissociationRegulatorsTemplateId, parameters);
                    notificationId.Add(response.id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ExceptionLogMessage, input.OrganisationNumber, input.UserId, _messagingConfig.MemberDissociationRegulatorsTemplateId);
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
                _logger.LogError(ex, "Regular Expression timeout out {Nation}", nation);
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
                _logger.LogError(ex, "Regular Expression timeout out {Nation}", nation);
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
                var response = _notificationClient.SendEmail(recipient, templateId, parameters);
                notificationId = response.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionLogMessage, organisationNumber, $"{firstName} {lastName}", templateId);
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
                var response = _notificationClient.SendEmail(recipient, templateId, parameters);
                notificationId = response.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionLogMessage, input.OrganisationNumber, $"{input.OldFirstName} {input.OldLastName}", templateId);
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
            else
            {
                return new Dictionary<string, object>
                {
                    { "producerName", input.ProducerOrganisationName },
                    { "regulatorOrganisationName", input.RegulatorOrganisationName },
                    { "submissionPeriod", input.SubmissionPeriod },
                    { "organisationNumber", input.OrganisationNumber },
                };
            }
        }
    }
}