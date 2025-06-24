using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.Subsidiary;
using FacadeAccountCreation.Core.Services.ComplianceScheme;
using Microsoft.FeatureManagement;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/compliance-schemes")]
public class ComplianceSchemesController(
    IComplianceSchemeService complianceSchemeService,
    ILogger<ComplianceSchemesController> logger,
    IMessagingService messagingService,
    IFeatureManager featureManager)
    : ControllerBase
{
    private const int MaxQueryLength = 160;


    [Route("{organisationId:guid}/schemes/{complianceSchemeId:guid}/scheme-members")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ComplianceSchemeMembershipResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetComplianceSchemeMembers(
        Guid organisationId,
        Guid complianceSchemeId,
        [BindRequired, FromQuery] int pageSize,
        string? query = "",
        int? page = 1,
        bool hideNoSubsidiaries = false)
    {
        query ??= string.Empty;

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
                logger.LogError("Unable to get the OId for the user when attempting to get compliance scheme members list");
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await complianceSchemeService.GetComplianceSchemeMembersAsync(userId,organisationId, complianceSchemeId, query, pageSize, page.Value, hideNoSubsidiaries);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Fetched compliance scheme members list successfully");
                return new OkObjectResult(response.Content.ReadFromJsonAsync<ComplianceSchemeMembershipResponse>().Result);
            }

            logger.LogError("Fetching compliance scheme members list failed");

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching compliance scheme members list");
            return HandleError.Handle(e);
        }
    }


    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(List<ComplianceSchemeModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllComplianceSchemes()
    {
        try
        {
            var response = await complianceSchemeService.GetAllComplianceSchemesAsync();

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Fetched compliance scheme list successfully");
                return new OkObjectResult(response.Content.ReadFromJsonAsync<List<ComplianceSchemeModel>>().Result);
            }

            logger.LogError("Fetching compliance scheme list failed");

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching compliance scheme list");
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("get-for-operator")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(List<ComplianceSchemeModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOperatorComplianceSchemes(Guid operatorOrganisationId)
    {
        try
        {
            var response = await complianceSchemeService.GetComplianceSchemesForOperatorAsync(operatorOrganisationId);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Fetched compliance scheme list for operatorOrganisationId {OperatorOrganisationId} successfully", operatorOrganisationId);
                return new OkObjectResult(response.Content.ReadFromJsonAsync<List<ComplianceSchemeModel>>().Result);
            }

            logger.LogError("Failed retrieving list of compliance Schemes for operatorOrganisationId {OperatorOrganisationId}",
                operatorOrganisationId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error fetching compliance scheme list for operatorOrganisationId {OperatorOrganisationId}",
                operatorOrganisationId);
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("get-for-producer")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProducerComplianceSchemeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComplianceSchemeForProducer(Guid producerOrganisationId)
    {
        try
        {
            var response = await complianceSchemeService.GetComplianceSchemeForProducerAsync(producerOrganisationId, User.UserId());

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("No current selected compliance scheme for producerOrganisationId {ProducerOrganisationId}", producerOrganisationId);
                return new NotFoundResult();
            }

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(response.Content.ReadFromJsonAsync<ProducerComplianceSchemeModel>().Result);
            }

            logger.LogError("Failed retrieving Compliance Scheme for producerOrganisationId {ProducerOrganisationId}", producerOrganisationId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed retrieving Compliance Scheme for producerOrganisationId {ProducerOrganisationId}", producerOrganisationId);
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
            
            var response = await complianceSchemeService.GetComplianceSchemesSummary(complianceSchemeId, organisationId, userId);

            return Ok(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get compliance scheme summaries for operator {OrganisationId}", complianceSchemeId);

            return HandleError.Handle(e);
        }         
    }

    [HttpGet]
    [Route("member-removal-reasons")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(List<ReasonsForRemovalModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllReasonsForRemoval()
    {
        try
        {
            var response = await complianceSchemeService.GetAllReasonsForRemovalsAsync();

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Fetched reasons for removal list successfully");
                return new OkObjectResult(response.Content.ReadFromJsonAsync<List<ReasonsForRemovalModel>>().Result);
            }

            logger.LogError("Fetching reasons for removal list failed");

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching reasons for removal list");
            return HandleError.Handle(e);
        }
    }

    [HttpPost]
    [Route("remove")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remove(RemoveComplianceSchemeModel request)
    {
        try
        {
            var oId = User.UserId();

            if (oId == Guid.Empty)
            {
                logger.LogError("Unable to get the OId for the user when attempting to remove the selected scheme id {SelectedSchemeId}", request.SelectedSchemeId);
                return Problem("UserId not  available", statusCode: StatusCodes.Status500InternalServerError);
            }

            request.UserOId = oId;
            var response = await complianceSchemeService.RemoveComplianceScheme(request);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Selected compliance scheme id {SelectedSchemeId} removed successfully", request.SelectedSchemeId);
                return Ok();
            }

            logger.LogError("Removing the selected scheme id {SelectedSchemeId} failed", request.SelectedSchemeId);
            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Removing selected scheme id {SelectedSchemeId} failed", request.SelectedSchemeId);
            return HandleError.Handle(e);
        }
    }

    [HttpPost]
    [Route("select")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SelectedSchemeIdModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SelectComplianceScheme(SelectSchemeModel model)
    {
        try
        {
            var oId = User.UserId();
            if (oId == Guid.Empty)
            {
                logger.LogError(
                    "Unable to get the OId for the user when attempting to select the compliance scheme with id {ModelComplianceSchemeId}",
                    model.ComplianceSchemeId);
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await complianceSchemeService.SelectComplianceSchemeAsync(
                new SelectSchemeWithUserModel(model, oId));

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Compliance scheme id {ModelComplianceSchemeId} selected successfully",
                    model.ComplianceSchemeId);
                return new OkObjectResult(response.Content.ReadFromJsonAsync<SelectedSchemeIdModel>().Result);
            }

            logger.LogError("$Selecting the selected scheme id {ComplianceSchemeId} failed",
                model.ComplianceSchemeId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Selecting compliance scheme failed");
            return HandleError.Handle(e);
        }
    }

    [HttpPost]
    [Route("update")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SelectedSchemeIdModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateComplianceScheme(UpdateSchemeModel model)
    {
        try
        {
            var oId = User.UserId();
            if (oId == Guid.Empty)
            {
                logger.LogError(
                    "Unable to get the OId for the user when attempting to update the selected scheme with id {ModelComplianceSchemeId} and selected compliance scheme with id {RequestedComplianceSchemeId}",
                    model.ComplianceSchemeId,
                    model.ComplianceSchemeId);
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await complianceSchemeService.UpdateComplianceSchemeAsync(
                new UpdateSchemeWithUserModel(model, oId));

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Compliance scheme id {ModelComplianceSchemeId} selected successfully",
                    model.ComplianceSchemeId);
                return new OkObjectResult(response.Content.ReadFromJsonAsync<SelectedSchemeIdModel>().Result);
            }

            logger.LogError("Updating the compliance scheme to scheme id {ModelComplianceSchemeId} failed",
                model.ComplianceSchemeId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Updating compliance scheme failed");
            return HandleError.Handle(e);
        }
    }
    
    [HttpGet]
    [Route("{organisationId:guid}/scheme-members/{selectedSchemeId:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MemberDetailsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetComplianceSchemeMemberDetails(Guid organisationId, Guid selectedSchemeId)
    {
        try
        {
            var uId = User.UserId();
            if (uId == Guid.Empty)
            {
                logger.LogError(
                    "Unable to get the OId for the user when attempting to member details for the organisation id {OrganisationId} and selected scheme id {SelectedSchemeId}",organisationId, selectedSchemeId);
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await complianceSchemeService.GetComplianceSchemeMemberDetailsAsync(uId, organisationId, selectedSchemeId);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Members fetched successfully for the compliance scheme organisation id {OrganisationId} and selected scheme id {SelectedSchemeId}", organisationId, selectedSchemeId);
                return new OkObjectResult(response.Content.ReadFromJsonWithEnumsAsync<MemberDetailsModel>().Result);
            }

            logger.LogError("Members fetched successfully for the compliance scheme organisation id {OrganisationId} and selected scheme id {SelectedSchemeId}", organisationId, selectedSchemeId);

            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Fetching member details failed");
            return HandleError.Handle(e);
        }
    }

    [HttpPost]
    [Route("{organisationId:guid}/scheme-members/{selectedSchemeId:guid}/removed")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RemoveComplianceSchemeMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                logger.LogError("Unable to get the OId for the user when attempting to remove selected scheme id {SelectedSchemeId}", selectedSchemeId);
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var removalInfoResponse = await complianceSchemeService.GetInfoForSelectedSchemeRemoval(organisationId, selectedSchemeId, userId);
            if (removalInfoResponse != null)
            {
                var removeResponse = await complianceSchemeService.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, model);

                logger.LogInformation("Selected compliance scheme id {SelectedSchemeId} removed successfully", selectedSchemeId);

                if (await featureManager.IsEnabledAsync(nameof(FeatureFlags.SendDissociationNotificationEmail)))
                {
                    messagingService.SendMemberDissociationRegulatorsNotification(new MemberDissociationRegulatorsEmailInput
                    {
                        UserId = userId,
                        ComplianceSchemeName = removalInfoResponse.ComplianceSchemeName,
                        ComplianceSchemeNation = removalInfoResponse.ComplianceSchemeNation,
                        OrganisationName = removalInfoResponse.OrganisationName,
                        OrganisationNation = removalInfoResponse.OrganisationNation,
                        OrganisationNumber = removalInfoResponse.OrganisationNumber.ToReferenceNumberFormat()
                    });
                    messagingService.SendMemberDissociationProducersNotification(new NotifyComplianceSchemeProducerEmailInput
                    {
                        Recipients = removalInfoResponse.RemovalNotificationRecipients,
                        OrganisationId = removalInfoResponse.OrganisationNumber.ToReferenceNumberFormat(),
                        ComplianceScheme = removalInfoResponse.ComplianceSchemeName,
                        OrganisationName = removalInfoResponse.OrganisationName,
                    });
                }

                return Ok(removeResponse);
            }

            logger.LogError("Removing the selected scheme id {SelectedSchemeId} failed", selectedSchemeId);
            return HandleError.HandleErrorWithStatusCode(HttpStatusCode.BadRequest);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Removing selected scheme id {SelectedSchemeId} failed", selectedSchemeId);
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("{organisationId:guid}/schemes/{complianceSchemeId:guid}/export-subsidiaries")]
    [ProducesResponseType(typeof(List<ExportOrganisationSubsidiariesResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportComplianceSchemeSubsidiaries(
        Guid organisationId,
        Guid complianceSchemeId
        )
    {
        var userId = User.UserId();

        var complianceSchemeSubsidiaries = await complianceSchemeService.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        if (complianceSchemeSubsidiaries != null)
        {
            return Ok(complianceSchemeSubsidiaries);
        }

        return NoContent();
    }
}
