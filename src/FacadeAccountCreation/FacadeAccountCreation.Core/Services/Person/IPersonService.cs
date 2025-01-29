using FacadeAccountCreation.Core.Models.Person;

namespace FacadeAccountCreation.Core.Services.Person;

public interface IPersonService
{
    Task<PersonResponseModel?> GetPersonByUserIdAsync(Guid userId);
    Task<PersonResponseModel?> GetAllPersonByUserIdAsync(Guid userId);
    Task<PersonResponseModel?> GetPersonByExternalIdAsync(Guid externalId);
    Task<InviteApprovedUserModel> GetPersonByInviteToken(string token);
}