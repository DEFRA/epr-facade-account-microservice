using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.User
{
    /// <summary>
    /// User details that can be updated
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record UserDetailsUpdateModel
    {
        /// <summary>
        /// User FirstName
        /// </summary>
        public string FirstName { get; init; }

        /// <summary>
        /// User LastName
        /// </summary>
        public string LastName { get; init; }

        /// <summary>
        /// User Jobtitle in an organisation
        /// </summary>
        public string JobTitle { get; init; }

        /// <summary>
        /// User Telephone
        /// </summary>
        public string Telephone { get; init; }
    }
}
