namespace FacadeAccountCreation.Core.Models.Messaging;

[ExcludeFromCodeCoverage]
public class InviteUserEmailInput
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Recipient { get; set; } = default!;
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; } = default!;
    public string JoinTheTeamLink { get; set; } = default!;
    public string TemplateId { get; set; }
}