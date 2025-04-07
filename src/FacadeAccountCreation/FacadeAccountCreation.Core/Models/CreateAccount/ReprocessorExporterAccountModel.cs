namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterAccountModel
{
    [Required]
    public PersonModel Person { get; set; } = null!;
}
