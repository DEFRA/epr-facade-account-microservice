namespace FacadeAccountCreation.Core.Services.User;

public interface IUserService
{
    Task<HttpResponseMessage> GetUserOrganisations(Guid userId);
}
