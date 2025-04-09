
namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/v1/reprocessor-exporter-accounts")]
public class ReprocessorExporterAccountsController(IAccountService accountService)
    : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAccount(ReprocessorExporterAccountModel account)
    {
        var accountWithUser = new ReprocessorExporterAccountWithUserModel(account, new UserModel
        {
            UserId = User.UserId(),
            Email = User.Email()
        });

        await accountService.AddReprocessorExporterAccountAsync(accountWithUser);

        return Ok();
    }
}
