namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[RequiredScope("account-creation")]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet(Name = "GetTestMessage")]
    public string Get()
    {
        var name = HttpContext.User.Identity!.Name;
        return $"Hello {name}, you successfully passed token to FACADE API and the request was authenticated";
    }
}
