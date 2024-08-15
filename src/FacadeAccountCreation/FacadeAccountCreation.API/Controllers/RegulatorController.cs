using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Models.Regulators;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.Core.Services.Organisation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FacadeAccountCreation.API.Controllers
{
    [ApiController]
    [Route("api/regulators")]
    public class RegulatorController : Controller
    {
        private readonly ILogger<RegulatorController> _logger;
        private readonly IOrganisationService _organisationService;
        private readonly IMessagingService _messagingService;

        public RegulatorController(ILogger<RegulatorController> logger, IOrganisationService organisationService, IMessagingService messagingService)
        {
            _logger = logger;
            _organisationService = organisationService;
            _messagingService = messagingService;
        }

        [HttpPost]
        [Route("resubmission-notify")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendNotificationOfResubmissionToUser(ResubmissionNotificationEmailModel request)
        {
            _logger.LogDebug("{Request}", JsonConvert.SerializeObject(request));
            var regulatorOrganisation = _organisationService.GetRegulatorOrganisationByNationId(request.NationId).Result;

            var resubmissionInput = new ResubmissionNotificationEmailInput
            {
                NationId = request.NationId,
                OrganisationNumber = request.OrganisationNumber,
                SubmissionPeriod = request.SubmissionPeriod,
                RegulatorOrganisationName = regulatorOrganisation?.OrganisationName,
                ProducerOrganisationName = request.ProducerOrganisationName,
                IsComplianceScheme = request.IsComplianceScheme
            };

            if (request.IsComplianceScheme)
            {
                resubmissionInput.ComplianceSchemeName = request.ComplianceSchemeName;
                resubmissionInput.ComplianceSchemePersonName = request.ComplianceSchemePersonName;
            }

            var notificationId = _messagingService.SendPoMResubmissionEmailToRegulator(resubmissionInput);
            return Ok(notificationId);
        }
    }
}