using FacadeAccountCreation.Core.Models.ServiceRolesLookup;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController(ILogger<RolesController> logger, IServiceRolesLookupService serviceRolesLookup)
    : ControllerBase
{
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(List<ServiceRolesLookupModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<List<ServiceRolesLookupModel>> GetServiceRoles()
    {
        var response = serviceRolesLookup.GetServiceRoles();
        if (response == null || response.Count == 0)
        {
            logger.LogError("No service roles found");
            return Problem("No service roles found", statusCode: StatusCodes.Status500InternalServerError);
        }

        logger.LogInformation("Fetched the service roles successfully");
        return new OkObjectResult(response);
    }
}
