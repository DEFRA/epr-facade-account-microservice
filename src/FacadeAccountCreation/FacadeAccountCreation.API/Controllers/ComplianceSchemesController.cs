using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Models.Subsidiary;
using FacadeAccountCreation.Core.Services.ComplianceScheme;
using FacadeAccountCreation.Core.Services.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.FeatureManagement;
using System.Net;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/compliance-schemes")]
public class ComplianceSchemesController : ControllerBase
{
    private readonly IComplianceSchemeService _complianceSchemeService;
    private readonly ILogger<ComplianceSchemesController> _logger;
    private readonly IMessagingService _messagingService;
    private readonly IFeatureManager _featureManager;
    
    private const int MaxQueryLength = 160;

    public ComplianceSchemesController(IComplianceSchemeService complianceSchemeService, ILogger<ComplianceSchemesController> logger, IMessagingService messagingService, IFeatureManager featureManager)
    {
        _complianceSchemeService = complianceSchemeService;
        _logger = logger;
        _messagingService = messagingService;
        _featureManager = featureManager;
    }



    [Route("{organisationId:guid}/schemes/{complianceSchemeId:guid}/scheme-members")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComplianceSchemeMembers(
        Guid organisationId,
        Guid complianceSchemeId,
        [BindRequired, FromQuery] int pageSize,
        string? query = "",
        int? page = 1)
    {
        if(query == null)
        {
            query = string.Empty;
        }

        if (query.Length > MaxQueryLength)
        {
            var message = $"Length {query.Length} of parameter 'query' exceeds max length {MaxQueryLength}";
            return Problem(message, statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var userId = User.UserId();

            if (userId == Guid.Empty)
            {
                _logger.LogError($"Unable to get the OId for the user when attempting to get compliance scheme members list");
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await _complianceSchemeService.GetComplianceSchemeMembersAsync(userId,organisationId, complianceSchemeId, query, pageSize, page.Value);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Fetched compliance scheme members list successfully");
                return new OkObjectResult(response.Content.ReadFromJsonAsync<ComplianceSchemeMembershipResponse>().Result);
            }

            _logger.LogError($"Fetching compliance scheme members list failed");

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching compliance scheme members list");
            return HandleError.Handle(e);
        }
    }


    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllComplianceSchemes()
    {
        try
        {
            var response = await _complianceSchemeService.GetAllComplianceSchemesAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Fetched compliance scheme list successfully");
                return new OkObjectResult(response.Content.ReadFromJsonAsync<List<ComplianceSchemeModel>>().Result);
            }

            _logger.LogError($"Fetching compliance scheme list failed");

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching compliance scheme list");
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("get-for-operator")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOperatorComplianceSchemes(Guid operatorOrganisationId)
    {
        try
        {
            var response = await _complianceSchemeService.GetComplianceSchemesForOperatorAsync(operatorOrganisationId);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Fetched compliance scheme list for operatorOrganisationId {operatorOrganisationId} successfully", operatorOrganisationId);
                return new OkObjectResult(response.Content.ReadFromJsonAsync<List<ComplianceSchemeModel>>().Result);
            }

            _logger.LogError("Failed retrieving list of compliance Schemes for operatorOrganisationId {operatorOrganisationId}",
                operatorOrganisationId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error fetching compliance scheme list for operatorOrganisationId {operatorOrganisationId}",
                operatorOrganisationId);
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("get-for-producer")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetComplianceSchemeForProducer(Guid producerOrganisationId)
    {
        try
        {
            var response = await _complianceSchemeService.GetComplianceSchemeForProducerAsync(producerOrganisationId, User.UserId());

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError("No current selected compliance scheme for producerOrganisationId {producerOrganisationId}", producerOrganisationId);
                return new NotFoundResult();
            }

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(response.Content.ReadFromJsonAsync<ProducerComplianceSchemeModel>().Result);
            }

            _logger.LogError("Failed retrieving Compliance Scheme for producerOrganisationId {producerOrganisationId}", producerOrganisationId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieving Compliance Scheme for producerOrganisationId {producerOrganisationId}", producerOrganisationId);
            return HandleError.Handle(e);
        }
    }
    
    /// <summary>
    /// Gets compliance scheme summary for an operator organisation: <paramref name="complianceSchemeId"/>.
    /// </summary>
    /// <returns>List&lt;ComplianceSchemeSummary&gt;</returns>
    [HttpGet]
    [Consumes("application/json")]
    [Route("{complianceSchemeId}/summary")]
    [ProducesResponseType(typeof(ComplianceSchemeSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComplianceSchemeSummary(
        Guid complianceSchemeId, 
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        try
        {
            var userId = User.UserId();
            
            var response = await _complianceSchemeService.GetComplianceSchemesSummary(complianceSchemeId, organisationId, userId);

            return Ok(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get compliance scheme summaries for operator {OrganisationId}", complianceSchemeId);

            return HandleError.Handle(e);
        }         
    }

    [HttpGet]
    [Route("member-removal-reasons")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllReasonsForRemoval()
    {
        try
        {
            var response = await _complianceSchemeService.GetAllReasonsForRemovalsAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Fetched reasons for removal list successfully");
                return new OkObjectResult(response.Content.ReadFromJsonAsync<List<ReasonsForRemovalModel>>().Result);
            }

            _logger.LogError($"Fetching reasons for removal list failed");

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching reasons for removal list");
            return HandleError.Handle(e);
        }
    }

    [HttpPost]
    [Route("remove")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove(RemoveComplianceSchemeModel request)
    {
        try
        {
            var oId = User.UserId();

            if (oId == Guid.Empty)
            {
                _logger.LogError("Unable to get the OId for the user when attempting to remove the selected scheme id {SelectedSchemeId}", request.SelectedSchemeId);
                return Problem("UserId not  available", statusCode: StatusCodes.Status500InternalServerError);
            }

            request.UserOId = oId;
            var response = await _complianceSchemeService.RemoveComplianceScheme(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Selected compliance scheme id {SelectedSchemeId} removed successfully", request.SelectedSchemeId);
                return Ok();
            }
            else
            {
                _logger.LogError("Removing the selected scheme id {SelectedSchemeId} failed", request.SelectedSchemeId);
                return HandleError.HandleErrorWithStatusCode(response.StatusCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Removing selected scheme id {SelectedSchemeId} failed", request.SelectedSchemeId);
            return HandleError.Handle(e);
        }
    }

    [HttpPost]
    [Route("select")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SelectComplianceScheme(SelectSchemeModel model)
    {
        try
        {
            var oId = User.UserId();
            if (oId == Guid.Empty)
            {
                _logger.LogError(
                    "Unable to get the OId for the user when attempting to select the compliance scheme with id {ModelComplianceSchemeId}",
                    model.ComplianceSchemeId);
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await _complianceSchemeService.SelectComplianceSchemeAsync(
                new SelectSchemeWithUserModel(model, oId));

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Compliance scheme id {ModelComplianceSchemeId} selected successfully",
                    model.ComplianceSchemeId);
                return new OkObjectResult(response.Content.ReadFromJsonAsync<SelectedSchemeIdModel>().Result);
            }

            _logger.LogError("$Selecting the selected scheme id {model.ComplianceSchemeId} failed",
                model.ComplianceSchemeId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Selecting compliance scheme failed");
            return HandleError.Handle(e);
        }
    }

    [HttpPost]
    [Route("update")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateComplianceScheme(UpdateSchemeModel model)
    {
        try
        {
            var oId = User.UserId();
            if (oId == Guid.Empty)
            {
                _logger.LogError(
                    "Unable to get the OId for the user when attempting to update the selected scheme with id {ModelComplianceSchemeId} and selected compliance scheme with id {RequestedComplianceSchemeId}",
                    model.ComplianceSchemeId,
                    model.ComplianceSchemeId);
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await _complianceSchemeService.UpdateComplianceSchemeAsync(
                new UpdateSchemeWithUserModel(model, oId));

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Compliance scheme id {ModelComplianceSchemeId} selected successfully",
                    model.ComplianceSchemeId);
                return new OkObjectResult(response.Content.ReadFromJsonAsync<SelectedSchemeIdModel>().Result);
            }

            _logger.LogError("Updating the compliance scheme to scheme id {ModelComplianceSchemeId} failed",
                model.ComplianceSchemeId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Updating compliance scheme failed");
            return HandleError.Handle(e);
        }
    }
    
    [HttpGet]
    [Route("{organisationId:guid}/scheme-members/{selectedSchemeId:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetComplianceSchemeMemberDetails(Guid organisationId, Guid selectedSchemeId)
    {
        try
        {
            var uId = User.UserId();
            if (uId == Guid.Empty)
            {
                _logger.LogError(
                    "Unable to get the OId for the user when attempting to member details for the organisation id {OrganisationId} and selected scheme id {SelectedSchemeId}",organisationId, selectedSchemeId);
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await _complianceSchemeService.GetComplianceSchemeMemberDetailsAsync(uId, organisationId, selectedSchemeId);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Members fetched successfully for the compliance scheme organisation id {OrganisationId} and selected scheme id {SelectedSchemeId}", organisationId, selectedSchemeId);
                return new OkObjectResult(response.Content.ReadFromJsonWithEnumsAsync<MemberDetailsModel>().Result);
            }

            _logger.LogError("Members fetched successfully for the compliance scheme organisation id {OrganisationId} and selected scheme id {SelectedSchemeId}", organisationId, selectedSchemeId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fetching member details failed");
            return HandleError.Handle(e);
        }
    }

    [HttpPost]
    [Route("{organisationId:guid}/scheme-members/{selectedSchemeId:guid}/removed")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RemoveComplianceSchemeMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveComplianceSchemeMember(
        [BindRequired, FromRoute] Guid organisationId,
        [BindRequired, FromRoute] Guid selectedSchemeId,
        [BindRequired, FromBody] RemoveComplianceSchemeMemberModel model)
    {
        try
        {
            var userId = User.UserId();
            if (userId == Guid.Empty)
            {
                _logger.LogError("Unable to get the OId for the user when attempting to remove selected scheme id {selectedSchemeId}", selectedSchemeId);
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var removalInfoResponse = await _complianceSchemeService.GetInfoForSelectedSchemeRemoval(organisationId, selectedSchemeId, userId);
            if (removalInfoResponse != null)
            {
                var removeResponse = await _complianceSchemeService.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, model);

                _logger.LogInformation("Selected compliance scheme id {selectedSchemeId} removed successfully", selectedSchemeId);

                if (await _featureManager.IsEnabledAsync(nameof(FeatureFlags.SendDissociationNotificationEmail)))
                {
                    _messagingService.SendMemberDissociationRegulatorsNotification(new MemberDissociationRegulatorsEmailInput
                    {
                        UserId = userId,
                        ComplianceSchemeName = removalInfoResponse.ComplianceSchemeName,
                        ComplianceSchemeNation = removalInfoResponse.ComplianceSchemeNation,
                        OrganisationName = removalInfoResponse.OrganisationName,
                        OrganisationNation = removalInfoResponse.OrganisationNation,
                        OrganisationNumber = removalInfoResponse.OrganisationNumber.ToReferenceNumberFormat()
                    });
                    _messagingService.SendMemberDissociationProducersNotification(new NotifyComplianceSchemeProducerEmailInput
                    {
                        Recipients = removalInfoResponse.RemovalNotificationRecipients,
                        OrganisationId = removalInfoResponse.OrganisationNumber.ToReferenceNumberFormat(),
                        ComplianceScheme = removalInfoResponse.ComplianceSchemeName,
                        OrganisationName = removalInfoResponse.OrganisationName,
                    });
                }

                return Ok(removeResponse);
            }

            _logger.LogError("Removing the selected scheme id {selectedSchemeId} failed", selectedSchemeId);
            return HandleError.HandleErrorWithStatusCode(HttpStatusCode.BadRequest);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Removing selected scheme id {selectedSchemeId} failed", selectedSchemeId);
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("{organisationId:guid}/schemes/{complianceSchemeId:guid}/export-subsidiaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportComplianceSchemeSubsidiaries(
        Guid organisationId,
        Guid complianceSchemeId
        )
    {
        var userId = User.UserId();

        var complianceSchemeSubsidiaries = await _complianceSchemeService.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        if (complianceSchemeSubsidiaries != null)
        {
            return Ok(complianceSchemeSubsidiaries);
        }
        else
        {
            return NoContent();
        }
    }
}
