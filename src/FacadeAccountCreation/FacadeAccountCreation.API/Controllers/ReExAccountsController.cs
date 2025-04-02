namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/reprocessors-exporter-accounts")]
public class ReExAccountsController(IAccountService accountService) //, IMessagingService messagingService)
    : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateReExAccount(AccountModel account)
    {
        var accountWithUser = new AccountWithUserModel(account, new UserModel
        {
            UserId = User.UserId(),
            Email = User.Email()
        });

        var createAccountResponse = await accountService.AddAccountAsync(accountWithUser);

        if (createAccountResponse == null)
        {
            //todo: pass root issue, so consumers don't have to dig through logs?
            return Problem("Failed to create reprocessor/exporter account", statusCode: StatusCodes.Status500InternalServerError);
        }

        //todo: should we send a confirmation?
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
