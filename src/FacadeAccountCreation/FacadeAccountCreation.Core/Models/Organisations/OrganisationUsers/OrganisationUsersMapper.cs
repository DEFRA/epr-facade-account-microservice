using FacadeAccountCreation.Core.Helpers;
using FacadeAccountCreation.Core.Models.ServiceRolesLookup;

namespace FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

public static class OrganisationUsersMapper
{
    public static List<OrganisationUserModel> ConvertToOrganisationUserModels(List<OrganisationUser> organisationUsers, List<ServiceRolesLookupModel> rolesLookupModels)
    {
        var userList = new List<OrganisationUserModel>();
        
        foreach (OrganisationUser organisationUser in organisationUsers)
        {
            var highestRole = OrganisationUserHelper.GetHighestRole(organisationUser, rolesLookupModels);

            if (highestRole == null)
                continue;
            
            var user = new OrganisationUserModel
            {
                FirstName = organisationUser.FirstName,
                LastName = organisationUser.LastName,
                Email = organisationUser.Email,
                PersonId = organisationUser.PersonId,
                PersonRoleId = organisationUser.PersonRoleId,
                ServiceRoleId = highestRole.ServiceRoleId,
                ServiceRoleKey = highestRole.Key,
                ConnectionId = organisationUser.ConnectionId,
                EnrolmentStatus = highestRole.EnrolmentStatus
            };
            
            userList.Add(user);
        }

        return userList;
    }
}
