using FacadeAccountCreation.Core.Models.Organisations.Mappers;

namespace FacadeAccountCreation.UnitTests.Core.Mappers;

[TestClass]
public class ReExAddOrganisationMapperTests
{
    [TestMethod]
    public void MapReExOrganisationModelToReExAddOrganisation_UpdatedWtihCorrectValue()
    {
        var reExOrgModel = new ReExOrganisationModel
        {
            Company = new ReExCompanyModel
            {
                CompaniesHouseNumber = "12345678",
                CompanyName = "Test Ltd",
                CompanyRegisteredAddress = new AddressModel
                {
                    BuildingName = "XYZ",
                    BuildingNumber = "14",
                    Country = "England",
                    County = "West Midlands",
                    Postcode = "CV1 9HB",
                    Street = "High Street",
                    Town = "Coventry"
                },
                IsComplianceScheme = false,
                Nation = Nation.England,
                OrganisationId = "7ea62027-2bd9-4267-aeea-7f6dbc71824a",
                OrganisationType = OrganisationType.CompaniesHouseCompany,
                ValidatedWithCompaniesHouse = true
            },
            InvitedApprovedPersons =
            [
                new() { 
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john.smith@tester.com",
                    Role = "CompanySecretary",
                    InviteToken = "B786tgs12856=="
                }
            ],
            IsApprovedUser = true,
            ReExUser = new ReExUserModel
            {
                IsApprovedUser = true,
                UserEmail = "user01@user.com",
                UserFirstName = "Peter",
                UserLastName = "Welsh",
                UserId = Guid.Parse("fadc06db-ac47-4c3c-ad5a-b0c800288668")
            },
            UserRoleInOrganisation = "Director"
        };

        var res = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(reExOrgModel);

        res.Should().NotBeNull();

        res.User.JobTitle.Should().Be("Director");
        res.User.IsApprovedUser.Should().BeTrue();
        res.User.UserId.Should().Be("fadc06db-ac47-4c3c-ad5a-b0c800288668");

        res.InvitedApprovedUsers.Should().HaveCount(1);
        res.InvitedApprovedUsers[0].JobTitle.Should().Be("CompanySecretary");
        res.InvitedApprovedUsers[0].Person.FirstName.Should().Be("John");
        res.InvitedApprovedUsers[0].Person.LastName.Should().Be("Smith");

        res.Organisation.Name.Should().Be("Test Ltd");
        res.Organisation.CompaniesHouseNumber.Should().Be("12345678");
        res.Organisation.Address.BuildingName.Should().Be("XYZ");
        res.Organisation.Address.Postcode.Should().Be("CV1 9HB");
    }
}
