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
            Organisation = organisationModel.Company != null ? GetCompanyModel(organisationModel.Company) : null,
            ManualInput = organisationModel.ManualInput != null ? GetManualInputModel(organisationModel) : null,
            Partners = [],
            DeclarationTimeStamp = DateTime.UtcNow
        };
    }

    private static ReExManualInputModel GetManualInputModel(ReExOrganisationModel organisationModel)
    {
        return new ReExManualInputModel
        {
            BusinessAddress = organisationModel.ManualInput?.BusinessAddress,
            ProducerType = organisationModel.ManualInput?.ProducerType,
            TradingName = organisationModel.ManualInput?.TradingName
        };
    }

    private static OrganisationModel GetCompanyModel(ReExCompanyModel companyModel)
    {
        return new OrganisationModel
        {
            Address = companyModel.CompanyRegisteredAddress,
            CompaniesHouseNumber = companyModel.CompaniesHouseNumber,
            IsComplianceScheme = companyModel.IsComplianceScheme,
            Name = companyModel.CompanyName,
            Nation = companyModel.Nation ?? Nation.NotSet,
            OrganisationId = companyModel.OrganisationId,
            OrganisationType = companyModel.OrganisationType ?? OrganisationType.NotSet,
            ProducerType = null,
            ValidatedWithCompaniesHouse = companyModel.ValidatedWithCompaniesHouse
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
