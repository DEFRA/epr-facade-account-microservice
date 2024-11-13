namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class AccountWithUserModel : AccountModel
{
    public AccountWithUserModel(AccountModel account, UserModel user)
    {
        Person = account.Person;
        Organisation = account.Organisation;
        Connection = account.Connection;
        User = user;
    }

    public UserModel User { get; set; }
}