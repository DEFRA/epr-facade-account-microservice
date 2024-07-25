using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;
using FacadeAccountCreation.Core.Services.Organisation;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using Microsoft.AspNetCore.Mvc;

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
            if (userId == default)
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
            return Problem("Failed to create and add organisation", statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(response);
    }
}
