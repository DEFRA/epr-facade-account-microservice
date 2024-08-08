using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Person;
using FacadeAccountCreation.Core.Models.User;

namespace FacadeAccountCreation.Core.Services.Person
{
    public interface IPersonService
    {
        Task<PersonResponseModel?> GetPersonByUserIdAsync(Guid userId);
        
        Task<PersonResponseModel?> GetPersonByExternalIdAsync(Guid externalId);
        Task<InviteApprovedUserModel> GetPersonByInviteToken(string token);

        /// <summary>
        /// Updates the user details
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="userDetailsDto">The details that will be updated of the user</param>
        /// <returns>Async task indicating success</returns>
        Task UpdateUserDetailsByUserId(
            Guid userId,
            UserDetailsDto userDetailsDto);
    }
}