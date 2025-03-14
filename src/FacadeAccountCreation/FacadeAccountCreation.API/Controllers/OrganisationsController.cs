using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/organisations")]
public class OrganisationsController(
    ILogger<OrganisationsController> logger,
    IOrganisationService organisationService,
    IServiceRolesLookupService serviceRolesLookupService)
    : ControllerBase
{
    [HttpGet]
    [Route("users")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganisationUsers(Guid organisationId, int serviceRoleId)
    {
        try
        {
            var userId = User.UserId();
            if (userId == Guid.Empty)
            {
                logger.LogError("UserId not available");
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await organisationService.GetOrganisationUserList(userId, organisationId, serviceRoleId);
            if (response.IsSuccessStatusCode)
            {
                var rolesLookupModels = serviceRolesLookupService.GetServiceRoles();

                var userListResponse = response.Content.ReadFromJsonAsync<List<OrganisationUser>>().Result;

                logger.LogInformation("Fetched the users for organisation {OrganisationId}", organisationId);

                var userList =
                    OrganisationUsersMapper.ConvertToOrganisationUserModels(userListResponse, rolesLookupModels);

                logger.LogInformation("Mapped the users for the response for organisation {OrganisationId}",
                    organisationId);

                return Ok(userList);
            }

            logger.LogError("Failed to fetch the users for organisation {OrganisationId}", organisationId);
            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching the users for organisation {OrganisationId}", organisationId);
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("organisation-nation")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNationIdByOrganisationId(Guid organisationId)
    {
        try
        {
            var response = await organisationService.GetNationIdByOrganisationId(organisationId);
            if (response.IsSuccessStatusCode)
            {
                var nationIdList = await response.Content.ReadFromJsonAsync<List<int>>();
                return Ok(nationIdList);
            }

            logger.LogError("Failed to fetch the nation Id for organisation {OrganisationId}", organisationId);
            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching the nation Id for organisation {OrganisationId}", organisationId);
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("regulator-nation")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRegulatorNation(Guid organisationId)
    {
        try
        {
            var response = await organisationService.GetOrganisationNationCodeByExternalIdAsync(organisationId);
            return response == null ? NotFound("Organisation not found") : Ok(response);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error fetching the nation for organisation {OrganisationId}", organisationId);
            return HandleError.Handle(exception);
        }
    }

    [HttpGet]
    [Route("organisation-name")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganisationNameByInviteToken([FromQuery] string token)
    {
        var response = await organisationService.GetOrganisationNameByInviteToken(token);

        return response == null ? NotFound() : Ok(response);
    }

    [HttpGet]
    [Route("organisation-by-reference-number/{referenceNumber}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganisationByReferenceNumber(string referenceNumber)
    {
        var response = await organisationService.GetOrganisationByReferenceNumber(referenceNumber);

        return response == null ? NotFound() : Ok(response);
    }

    [HttpPost]
    [Route("create-and-add-subsidiary")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAndAddSubsidiary(LinkOrganisationModel linkOrganisationModel)
    {
        linkOrganisationModel.UserId = User.UserId();
        var response = await organisationService.CreateAndAddSubsidiaryAsync(linkOrganisationModel);

        if (response == null)
        {
            return Problem("Failed to create and add organisation", statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("add-subsidiary")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddSubsidiary(SubsidiaryAddModel subsidiaryAddModel)
    {
        subsidiaryAddModel.UserId = User.UserId();

        var response = await organisationService.AddSubsidiaryAsync(subsidiaryAddModel);

        if (response == null)
        {
            return Problem("Failed to add subsidiary", statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("terminate-subsidiary")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> TerminateSubsidiary(SubsidiaryTerminateModel subsidiaryTerminateModel)
    {
        try
        {
            subsidiaryTerminateModel.UserId = User.UserId();
            await organisationService.TerminateSubsidiaryAsync(subsidiaryTerminateModel);
            return Ok();
        }
        catch (ProblemResponseException e)
        {
            logger.LogError(e, "Error terminating subsidiary {ChildOrganisationId} for organisation {ParentOrganisationId}", subsidiaryTerminateModel.ChildOrganisationId, subsidiaryTerminateModel.ParentOrganisationId);
            return HandleError.Handle(e);
        }
    }

    [HttpGet]
    [Route("{organisationId:guid}/organisationRelationships")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationRelationshipsByOrganisationIdAsync(Guid organisationId)
    {
        var organisationRelationships = await organisationService.GetOrganisationRelationshipsByOrganisationId(organisationId);

        if (organisationRelationships != null)
        {
            return Ok(organisationRelationships);
        }

        return NoContent();
    }

    [HttpGet]
    [Route("~/api/v2/organisations/{organisationId:guid}/organisationRelationships")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPagedOrganisationRelationshipsByOrganisationIdAsyncV2(
        Guid organisationId,
        Guid? complianceSchemeId,
        [Required] int? page,
        [Required] int? showPerPage)
    {
        var result = await organisationService.GetPagedOrganisationRelationshipsByOrganisationId(
            organisationId,
            complianceSchemeId,
            page.Value,
            showPerPage.Value);

        return Ok(result);
    }

    [HttpGet]
    [Route("{organisationId:guid}/export-subsidiaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExportOrganisationSubsidiariesAsync(Guid organisationId)
    {
        var organisationSubsidiaries = await organisationService.ExportOrganisationSubsidiaries(organisationId);

        if (organisationSubsidiaries != null)
        {
            return Ok(organisationSubsidiaries);
        }

        return NoContent();
    }
    
    /// <summary>
    /// Updates the details of an organisation
    /// </summary>
    /// <param name="id">Id of the organisation to update</param>
    /// <param name="organisationDetails">The updated details for the organisation</param>
    /// <returns>An async IActionResult</returns>
    [HttpPut]
    [Route("organisation/{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrganisationDetails(
        Guid id,
        [FromBody] OrganisationUpdateDto? organisationDetails)
    {
        if (organisationDetails == null)
        {
            return HandleError.HandleErrorWithStatusCode(HttpStatusCode.BadRequest);
        }

        try
        {
            var userId = User.UserId();

            await organisationService.UpdateOrganisationDetails(
                userId,
                id,
                organisationDetails);

            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating the nation Id for organisation {Id}", id);
            return HandleError.Handle(e);
        }
	}

	[HttpGet]
	[Route("v1/child-organisation-external-ids")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> GetChildOrganisationExternalIdsAsync([BindRequired, FromQuery] Guid organisationId, Guid? complianceSchemeId)
	{
        var externalIds = await organisationService.GetChildOrganisationExternalIdsAsync(organisationId, complianceSchemeId);
		return Ok(externalIds);
	}
}
