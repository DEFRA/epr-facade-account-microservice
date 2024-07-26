using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.User
{
    [ExcludeFromCodeCoverage]
    public record UserDetailsDto
    {
        public string FirstName { get; init; }

        public string LastName { get; init; }

        public string JobTitle { get; init; }

        public string TelePhone { get; init; }
    }
}
