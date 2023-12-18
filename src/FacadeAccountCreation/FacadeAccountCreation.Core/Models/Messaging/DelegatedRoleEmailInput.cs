namespace FacadeAccountCreation.Core.Models.Messaging
{
    public class DelegatedRoleEmailInput
    {
        public string Recipient { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NominatorFirstName { get; set; }
        public string NominatorLastName { get; set; }
        public string OrganisationNumber { get; set; }
        public string OrganisationName { get; set; }
        public Connections.PersonRole PersonRole { get; set; } = Connections.PersonRole.Employee;
        public Guid? OrganisationId { get; set; }
        public Guid? UserId { get; set; }
        public string? TemplateId { get; set; }

        public void EnsureInitialised()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                errors.Add("FirstName cannot be empty string.");
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                errors.Add("LastName cannot be empty string.");
            }

            if (string.IsNullOrWhiteSpace(Recipient))
            {
                errors.Add("Recipient cannot be empty string.");
            }

            if (string.IsNullOrWhiteSpace(OrganisationNumber))
            {
                errors.Add("OrganisationNumber cannot be empty string.");
            }

            if (string.IsNullOrWhiteSpace(OrganisationName))
            {
                errors.Add("OrganisationName cannot be empty string.");
            }

            if (string.IsNullOrWhiteSpace(NominatorFirstName))
            {
                errors.Add("NominatorFirstName cannot be empty string.");
            }

            if (string.IsNullOrWhiteSpace(NominatorFirstName))
            {
                errors.Add("NominatorFirstName cannot be empty string.");
            }

            if (string.IsNullOrWhiteSpace(TemplateId))
            {
                errors.Add("TemplateId cannot be empty string.");
            }

            if (!UserId.HasValue || UserId == Guid.Empty)
            {
                errors.Add("UserId is required.");
            }

            if (!OrganisationId.HasValue || OrganisationId == Guid.Empty)
            {
                errors.Add("OrganisationId is required.");
            }

            if (errors.Any())
            {
                throw new ArgumentException(string.Join(' ', errors));
            }
        }
    }
}