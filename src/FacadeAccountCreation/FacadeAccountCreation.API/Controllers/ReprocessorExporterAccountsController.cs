
namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/v1/reprocessor-exporter-accounts")]
public class ReprocessorExporterAccountsController(IAccountService accountService, IMessagingService messagingService)
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

        //todo: there will be a future story to send a notification to the user
        //messagingService.SendReprocessorExporterAccountCreationConfirmation(
        //    accountWithUser.User.UserId.GetValueOrDefault(),
        //    account.Person.FirstName,
        //    account.Person.LastName,
        //    account.Person.ContactEmail);

        return Ok();
    }
}
