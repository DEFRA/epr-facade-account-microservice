using FacadeAccountCreation.Core.Models.User;

namespace FacadeAccountCreation.Core.Services.User;

public interface IUserService
{
    Task<HttpResponseMessage> GetUserOrganisations(Guid userId);
    Task<HttpResponseMessage> UpdatePersonalDetailsAsync(Guid userId, Guid organisationId, string serviceKey, UserDetailsUpdateModel userDetailsUpdateModelRequest);
}
