using System.Security.Claims;

namespace FacadeAccountCreation.Core.Models.Organisations.Mappers;

[ExcludeFromCodeCoverage]
public static class ReExAddOrganisationMapper
{
    public static ReprocessorExporterAddOrganisation MapReExOrganisationModelToReExAddOrganisation(ReExOrganisationModel organisationModel)
    {
        return new ReprocessorExporterAddOrganisation
        {
            User = new ReprocessorExporterUserModel
            {
                IsApprovedUser = organisationModel.IsApprovedUser,
                JobTitle = organisationModel.UserRoleInOrganisation,
                UserId = organisationModel.ReExUser.UserId.Value
            },
            InvitedApprovedUsers = GetInvitedUsers(organisationModel.InvitedApprovedPersons),
            Organisation = new OrganisationModel
            {
                Address = organisationModel.Company.CompanyRegisteredAddress,
                CompaniesHouseNumber = organisationModel.Company.CompaniesHouseNumber,
                IsComplianceScheme = organisationModel.Company.IsComplianceScheme,
                Name = organisationModel.Company.CompanyName,
                Nation = organisationModel.Company.Nation ?? Nation.NotSet,
                OrganisationId = organisationModel.Company.OrganisationId,
                OrganisationType = organisationModel.Company.OrganisationType ?? OrganisationType.NotSet,
                ProducerType = null,
                ValidatedWithCompaniesHouse = organisationModel.Company.ValidatedWithCompaniesHouse                
            },
            Partners = [],
            ServiceRoleName = organisationModel.ServiceRole,
            DeclarationTimeStamp = DateTime.UtcNow
        };
    }

    private static List<InvitedApprovedUserModel> GetInvitedUsers(IEnumerable<ReExInvitedApprovedPerson> invitedApprovedList)
    {
        List<InvitedApprovedUserModel> invitedApprovedUserModels = [];

        foreach (var person in invitedApprovedList ?? [])
        {
            var userModel = new InvitedApprovedUserModel
            {
                JobTitle = person.Role,
                Person = new PersonModel
                {
                    ContactEmail = person.Email,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    TelephoneNumber = person.TelephoneNumber
                }
            };

            invitedApprovedUserModels.Add(userModel);
        }

        return invitedApprovedUserModels;
    }
}
