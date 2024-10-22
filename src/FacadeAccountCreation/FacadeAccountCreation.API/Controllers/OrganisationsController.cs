using Azure;
using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;
using FacadeAccountCreation.Core.Services.Organisation;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/organisations")]
public class OrganisationsController : Controller
{
    private readonly ILogger<OrganisationsController> _logger;
    private readonly IOrganisationService _organisationService;
    private readonly IServiceRolesLookupService _serviceRolesLookupService;

    public OrganisationsController(ILogger<OrganisationsController> logger, IOrganisationService organisationService,
        IServiceRolesLookupService serviceRolesLookupService)
    {
        _logger = logger;
        _organisationService = organisationService;
        _serviceRolesLookupService = serviceRolesLookupService;
    }

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
                _logger.LogError("UserId not available");
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await _organisationService.GetOrganisationUserList(userId, organisationId, serviceRoleId);
            if (response.IsSuccessStatusCode)
            {
                var rolesLookupModels = _serviceRolesLookupService.GetServiceRoles();

                var userListResponse = response.Content.ReadFromJsonAsync<List<OrganisationUser>>().Result;

                _logger.LogInformation("Fetched the users for organisation {OrganisationId}", organisationId);

                var userList =
                    OrganisationUsersMapper.ConvertToOrganisationUserModels(userListResponse, rolesLookupModels);

                _logger.LogInformation("Mapped the users for the response for organisation {OrganisationId}",
                    organisationId);

                return Ok(userList);
            }

            _logger.LogError("Failed to fetch the users for organisation {OrganisationId}", organisationId);
            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError("Error fetching the users for organisation {OrganisationId}", organisationId);
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
            var response = await _organisationService.GetNationIdByOrganisationId(organisationId);
            if (response.IsSuccessStatusCode)
            {
                var nationIdList = await response.Content.ReadFromJsonAsync<List<int>>();
                return Ok(nationIdList);
            }

            _logger.LogError("Failed to fetch the nation Id for organisation {OrganisationId}", organisationId);
            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError("Error fetching the nation Id for organisation {OrganisationId}", organisationId);
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
            var response = await _organisationService.GetOrganisationNationByExternalIdAsync(organisationId);
            return response == null ? NotFound() : Ok(response);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error fetching the nation for organisation {OrganisationId}", organisationId);
            return HandleError.Handle(exception);
        }
    }

    [HttpGet]
    [Route("organisation-name")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNationIdByOrganisationId([FromQuery] string token)
    {
        var response = await _organisationService.GetOrganisationNameByInviteToken(token);

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
        var response = await _organisationService.GetOrganisationByReferenceNumber(referenceNumber);

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
        var response = await _organisationService.CreateAndAddSubsidiaryAsync(linkOrganisationModel);

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

        var response = await _organisationService.AddSubsidiaryAsync(subsidiaryAddModel);

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
    public async Task<ActionResult> TerminateSubsidiary(SubsidiaryTerminateModel subsidiaryTerminateModel)
    {
        try
        {
            subsidiaryTerminateModel.UserId = User.UserId();
            await _organisationService.TerminateSubsidiaryAsync(subsidiaryTerminateModel);
            return Ok();
        }
        catch (ProblemResponseException e)
        {
            _logger.LogError(e, "Error terminating subsidiary {ChildOrganisationId} for organisation {ParentOrganisationId}", subsidiaryTerminateModel.ChildOrganisationId, subsidiaryTerminateModel.ParentOrganisationId);
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
        var organisationRelationships = await _organisationService.GetOrganisationRelationshipsByOrganisationId(organisationId);

        if (organisationRelationships != null)
        {
            return Ok(organisationRelationships);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Route("{organisationId:guid}/export-subsidiaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExportOrganisationSubsidiariesAsync(Guid organisationId)
    {
        var organisationSubsidiaries = await _organisationService.ExportOrganisationSubsidiaries(organisationId);

        if (organisationSubsidiaries != null)
        {
            return Ok(organisationSubsidiaries);
        }
        else
        {
            return NoContent();
        }
    }
    /// <summary>
    /// Updates the details of an organisation
    /// </summary>
    /// <param name="organisationId">Id of the organisation to update</param>
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

            await _organisationService.UpdateOrganisationDetails(
                userId,
                id,
                organisationDetails);

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating the nation Id for organisation {id}");
            return HandleError.Handle(e);
        }
    }
}
