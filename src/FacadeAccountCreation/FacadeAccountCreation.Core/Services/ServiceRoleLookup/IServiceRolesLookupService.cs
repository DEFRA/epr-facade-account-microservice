using FacadeAccountCreation.Core.Models.ServiceRolesLookup;

namespace FacadeAccountCreation.Core.Services.ServiceRoleLookup;

public interface IServiceRolesLookupService
{
    List<ServiceRolesLookupModel> GetServiceRoles();

     bool IsProducer(int serviceRoleId);
}