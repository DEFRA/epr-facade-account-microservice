using FacadeAccountCreation.Core.Models.ServiceRolesLookup;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using Microsoft.AspNetCore.Mvc;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : Controller
{
    private readonly ILogger<RolesController> _logger;
    private readonly IServiceRolesLookupService _serviceRolesLookup;

    public RolesController(ILogger<RolesController> logger, IServiceRolesLookupService serviceRolesLookup)
    {
        _logger = logger;
        _serviceRolesLookup = serviceRolesLookup;
    }

    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<List<ServiceRolesLookupModel>> GetServiceRoles()
    {
        var response = _serviceRolesLookup.GetServiceRoles();
        if (response == null || response.Count == 0)
        {
            _logger.LogError("No service roles found");
            return Problem("No service roles found", statusCode: StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation($"Fetched the service roles successfully");
        return new OkObjectResult(response);
    }
}
