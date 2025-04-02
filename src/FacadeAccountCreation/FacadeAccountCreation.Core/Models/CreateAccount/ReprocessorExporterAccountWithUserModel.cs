namespace FacadeAccountCreation.Core.Models.CreateAccount;

//todo: this is a little messy (copies existing pattern)
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