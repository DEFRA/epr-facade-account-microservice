using FacadeAccountCreation.Core.Models.ServiceRolesLookup;
using Microsoft.Extensions.Options;

namespace FacadeAccountCreation.Core.Services.ServiceRoleLookup;

public class ServiceRolesLookupService : IServiceRolesLookupService 
{
    private readonly ServiceRolesConfig _config;
    private const int ProducerApprovedPerson = 1;
    private const int ProducerBasicUser = 3;
    
    public ServiceRolesLookupService(IOptions<ServiceRolesConfig> config)
    {
        _config = config.Value;
    }
    
    public List<ServiceRolesLookupModel> GetServiceRoles()
    {
        return _config.Roles;
    }
    
    public bool IsProducer(int serviceRoleId)
    {
        return serviceRoleId is >= ProducerApprovedPerson and <= ProducerBasicUser;
    }
} 