namespace FacadeAccountCreation.Core.Models.Organisations.Mappers;

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
            Organisation = GetOrganisationModel(organisationModel),
            Partners = [],
            DeclarationTimeStamp = DateTime.UtcNow
        };
    }

    private static OrganisationModel GetOrganisationModel(ReExOrganisationModel organisationModel)
    {
        return organisationModel switch
        {
            { Company: not null } => new OrganisationModel
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
            { ManualInput: not null } => new OrganisationModel
            {
                Address = organisationModel.ManualInput.BusinessAddress,
                IsComplianceScheme = false,
                Name = organisationModel.ManualInput.TradingName,
                Nation = organisationModel.ManualInput.Nation ?? Nation.NotSet,
                OrganisationId = null,
                OrganisationType = organisationModel.ManualInput.OrganisationType ?? OrganisationType.NotSet,
                ProducerType = organisationModel.ManualInput.ProducerType ?? ProducerType.NotSet,
                ValidatedWithCompaniesHouse = false
            },
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
