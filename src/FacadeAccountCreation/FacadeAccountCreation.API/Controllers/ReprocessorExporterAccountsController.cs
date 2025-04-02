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
        //todo: can person contact email and user email be different? don't think do, but check source (frontend) - we don't capture the person email during the journey
        // if they can be different, might need something like this..
        //var accountWithUser = new AccountWithUserModel(account, new UserModel
        //{
        //    UserId = User.UserId(),
        //    Email = User.Email()
        //});

        //var createAccountResponse = await accountService.AddReprocessorExporterAccountAsync(account);

        //if (createAccountResponse == null)
        //{
        //    //todo: pass root issue, so consumers don't have to dig through logs?
        //    return Problem("Failed to create reprocessor/exporter account", statusCode: StatusCodes.Status500InternalServerError);
        //}

        // we could just send down the user id guid, rather than an user, as the email is currently always the same as the person email and the other fields are not populated
        // but we follow the existing account creation pattern, which sends down a User
        var accountWithUser = new ReprocessorExporterAccountWithUserModel(account, new UserModel
        {
            UserId = User.UserId(),
            Email = User.Email()
        });

        //todo: if issue, what happens and do we need to return a Problem?
        await accountService.AddReprocessorExporterAccountAsync(accountWithUser);

        //todo: we need to send a confirmation, but we don't have an email template yet
        //messagingService.SendAccountCreationConfirmation(
        //    accountWithUser.User.UserId.GetValueOrDefault(),
        //    account.Person.FirstName,
        //    account.Person.LastName,
        //    account.Person.ContactEmail,
        //    createAccountResponse.ReferenceNumber.ToReferenceNumberFormat(),
        //    createAccountResponse.OrganisationId,
        //    account.Organisation.IsComplianceScheme);

        return Ok();
    }
}
