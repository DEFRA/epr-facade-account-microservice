using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Services.CreateAccount;
using FacadeAccountCreation.Core.Services.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[RequiredScope("account-creation")]
[Route("api/producer-accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IMessagingService _messagingService;

    public AccountsController(IAccountService accountService, IMessagingService messagingService)
    {
        _accountService = accountService;
        _messagingService = messagingService;
    }
    
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

        var createAccountResponse = await _accountService.AddAccountAsync(accountWithUser);

        if (createAccountResponse == null)
        {
            return Problem("Failed to create account", statusCode: StatusCodes.Status500InternalServerError);
        }

        _messagingService.SendAccountCreationConfirmation(
            accountWithUser.User.UserId.GetValueOrDefault(),
            account.Person!.FirstName!,
            account.Person!.LastName!,
            account.Person!.ContactEmail!,
            createAccountResponse.ReferenceNumber!.ToReferenceNumberFormat(),
            createAccountResponse.OrganisationId,
            account.Organisation!.IsComplianceScheme);
        
        return Ok();
    }
}
