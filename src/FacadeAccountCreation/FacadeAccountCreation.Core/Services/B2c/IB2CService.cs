using FacadeAccountCreation.Core.Models.B2c;

namespace FacadeAccountCreation.Core.Services.B2c;

public interface IB2CService
{
    Task<HttpResponseMessage> GetUserOrganisationIds(UserOrganisationIdentifiersRequest request);
}
