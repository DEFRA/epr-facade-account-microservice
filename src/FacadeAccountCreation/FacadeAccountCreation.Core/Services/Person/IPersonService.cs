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
    }
}