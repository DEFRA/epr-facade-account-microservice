namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterAccountModel
{
    [Required]
    public PersonModel Person { get; set; } = null!;

    //todo: why nullable / not required? (currently matching AccountModel)
    //public Guid? UserId { get; set; }
}
