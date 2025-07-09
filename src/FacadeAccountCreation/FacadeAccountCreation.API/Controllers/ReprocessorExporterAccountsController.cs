
namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/v1/reprocessor-exporter-user-accounts")]
public class ReprocessorExporterAccountsController(IAccountService accountService,
    IMessagingService messagingService)
    : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAccount(
        ReprocessorExporterAccountModel account,
        [BindRequired, FromQuery] string serviceKey)
    {
        var accountWithUser = new ReprocessorExporterAccountWithUserModel(account, new UserModel
        {
            UserId = User.UserId(),
            Email = User.Email()
        });

        await accountService.AddReprocessorExporterAccountAsync(accountWithUser, serviceKey);

        // send email confirmation for account creation
        var confirmationEmailId = messagingService.SendReExAccountCreationConfirmation(
            userId: User.UserId().ToString(), 
            firstName: account.Person.FirstName, 
            lastName: account.Person.LastName, 
            contactEmail: account.Person.ContactEmail);

        return Ok(confirmationEmailId);
    }
}
