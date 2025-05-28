namespace FacadeAccountCreation.Core.Services.User;

public interface IUserService
{
    Task<HttpResponseMessage> GetUserOrganisations(Guid userId);
    Task<HttpResponseMessage> GetUserOrganisations(Guid userId, string serviceKey);
    Task<HttpResponseMessage> UpdatePersonalDetailsAsync(
        Guid userId, 
        Guid organisationId, 
        string serviceKey, 
        UpdateUserDetailsRequest userDetailsUpdateModelRequest);
}
