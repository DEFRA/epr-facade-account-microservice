using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

namespace FacadeAccountCreation.Core.Helpers;

public static class OrganisationUserHelper
{
    public static ServiceRolesLookupModel? GetHighestRole(OrganisationUser organisationUser, List<ServiceRolesLookupModel> rolesLookupModels)
    {
        foreach (var rolesLookupModel in rolesLookupModels.OrderBy(x => x.ServiceRoleId).ThenBy(x => x.PersonRoleId))
        {
            foreach (var enrolment in organisationUser.Enrolments.OrderBy(x => x.ServiceRoleId))
            {
                if (organisationUser.PersonRoleId == rolesLookupModel.PersonRoleId &&
                    enrolment.ServiceRoleId == rolesLookupModel.ServiceRoleId)
                {
                    rolesLookupModel.EnrolmentStatus = enrolment.EnrolmentStatus;
                    return rolesLookupModel;
                }
            }
        }
        
        return null;
    }
}