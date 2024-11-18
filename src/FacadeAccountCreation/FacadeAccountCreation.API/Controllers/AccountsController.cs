namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/producer-accounts")]
public class AccountsController(IAccountService accountService, IMessagingService messagingService)
    : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAccount(AccountModel account)
    {
        var accountWithUser = new AccountWithUserModel(account, new UserModel
        {
            UserId = User.UserId(),
            Email = User.Email()
        });

        var createAccountResponse = await accountService.AddAccountAsync(accountWithUser);

        if (createAccountResponse == null)
        {
            return Problem("Failed to create account", statusCode: StatusCodes.Status500InternalServerError);
        }

        messagingService.SendAccountCreationConfirmation(
            accountWithUser.User.UserId.GetValueOrDefault(),
            account.Person.FirstName,
            account.Person.LastName,
            account.Person.ContactEmail,
            createAccountResponse.ReferenceNumber.ToReferenceNumberFormat(),
            createAccountResponse.OrganisationId,
            account.Organisation.IsComplianceScheme);
        
        return Ok();
    }
    
    [HttpPost]
    [Route("ApprovedUser")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateApprovedUserAccount(AccountModel approvedUser)
    {
        approvedUser.UserId = User.UserId();
        var createAccountResponse = await accountService.AddApprovedUserAccountAsync(approvedUser);

        if (createAccountResponse == null)
        {
            return Problem("Failed to create account", statusCode: StatusCodes.Status500InternalServerError);
        }
        
        messagingService.SendApprovedUserAccountCreationConfirmation(
            approvedUser.Organisation.Name,
            approvedUser.Person.FirstName,
            approvedUser.Person.LastName,
            approvedUser.Organisation.OrganisationId,
            approvedUser.Person.ContactEmail);
        
        return Ok();
    }
}
