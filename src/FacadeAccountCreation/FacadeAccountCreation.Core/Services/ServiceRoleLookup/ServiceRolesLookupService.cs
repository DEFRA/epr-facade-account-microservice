namespace FacadeAccountCreation.Core.Services.ServiceRoleLookup;

public class ServiceRolesLookupService(IOptions<ServiceRolesConfig> config) : IServiceRolesLookupService
{
    private readonly ServiceRolesConfig _config = config.Value;
    private const int ProducerApprovedPerson = 1;
    private const int ProducerBasicUser = 3;

    public List<ServiceRolesLookupModel> GetServiceRoles()
    {
        return _config.Roles;
    }
    
    public bool IsProducer(int serviceRoleId)
    {
        return serviceRoleId is >= ProducerApprovedPerson and <= ProducerBasicUser;
    }
} 