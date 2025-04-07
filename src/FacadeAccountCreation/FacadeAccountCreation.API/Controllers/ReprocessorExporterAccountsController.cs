namespace FacadeAccountCreation.API.Controllers;

[ApiController]
//todo: singular, singular - spec has plural for re
//todo: v1?
[Route("api/reprocessor-exporter-accounts")]
public class ReprocessorExporterAccountsController(IAccountService accountService, IMessagingService messagingService)
    : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAccount(ReprocessorExporterAccountModel account)
    {
        // we could just send down the user id guid, rather than an user, as the email is currently always the same as the person email and the other fields are not populated
        // but we follow the existing account creation pattern, which sends down a User
        var accountWithUser = new ReprocessorExporterAccountWithUserModel(account, new UserModel
        {
            UserId = User.UserId(),
            Email = User.Email()
        });

        //todo: for testing only
        //var accountWithUser = new ReprocessorExporterAccountWithUserModel(account, new UserModel
        //{
        //    UserId = Guid.NewGuid(),
        //    Email = account.Person.ContactEmail
        //});

        //todo: we could assert User.Email() == person.email

        //todo: if issue, what happens and do we need to return a Problem?
        await accountService.AddReprocessorExporterAccountAsync(accountWithUser);

        //if (createAccountResponse == null)
        //{
        //    //todo: pass root issue, so consumers don't have to dig through logs?
        //    return Problem("Failed to create reprocessor/exporter account", statusCode: StatusCodes.Status500InternalServerError);
        //}

        //todo: there will be a future story to send a notification to the user
        //messagingService.SendReprocessorExporterAccountCreationConfirmation(
        //    accountWithUser.User.UserId.GetValueOrDefault(),
        //    account.Person.FirstName,
        //    account.Person.LastName,
        //    account.Person.ContactEmail);

        return Ok();
    }
}
