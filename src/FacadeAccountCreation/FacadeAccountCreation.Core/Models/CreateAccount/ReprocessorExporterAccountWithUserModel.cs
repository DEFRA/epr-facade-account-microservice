namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterAccountWithUserModel : ReprocessorExporterAccountModel
{
    public ReprocessorExporterAccountWithUserModel(ReprocessorExporterAccountModel account, UserModel user)
    {
        Person = account.Person;
        User = user;
    }

    public UserModel User { get; set; }
}