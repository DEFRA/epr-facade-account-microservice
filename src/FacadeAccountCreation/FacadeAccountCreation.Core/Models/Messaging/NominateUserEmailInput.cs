namespace FacadeAccountCreation.Core.Models.Messaging;

[ExcludeFromCodeCoverage]
public class NominateUserEmailInput
{
    public Guid UserId { get; set; }
    public Guid OrganisationId { get; set; }
    public string TemplateId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Recipient { get; set; } = default!;
    public string OrganisationName { get; set; } = default!;
    public string OrganisationNumber { get; set; } = default!;
    public string NominatorFirstName { get; set; } = default!;
    public string NominatorLastName { get; set; } = default!;
    public string AccountLoginUrl { get; set; } = default!;
}