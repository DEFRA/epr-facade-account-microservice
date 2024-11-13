namespace FacadeAccountCreation.Core.Models.Person;

[ExcludeFromCodeCoverage]
public class PersonResponseModel
{
    public DateTimeOffset CreatedOn { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;

    public string TelephoneNumber { get; set; } = null!;
}