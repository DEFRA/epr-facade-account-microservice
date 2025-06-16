using FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;
using FacadeAccountCreation.Core.Models.Organisations.Mappers;

namespace FacadeAccountCreation.UnitTests.Core.Mappers;

[TestClass]
public class ReExNotificationMapperTests
{

    [TestMethod]
    public void MapOrganisationModelToReExNotificationModel_MapsDataCorrectly_AndReturns_ReExNotificationModel()
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
                },
                new() {
                    FirstName = "Suzane",
                    LastName = "Jon",
                    Email = "s.jon@tester.com",
                    Role = "CompanySecretary"
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

        var addOrgResponse = new ReExAddOrganisationResponse()
        {
           OrganisationId = Guid.Parse("7ea62027-2bd9-4267-aeea-7f6dbc71824a"),
           ReferenceNumber = "122345678",
           InvitedApprovedUsers =
           [
               new() { Email = "john.smith@tester.com", InviteToken = "B786tgs12856==" }
           ],
           UserFirstName = "Peter",
           UserLastName = "Welsh"
        };


        var res = ReExNotificationMapper.MapOrganisationModelToReExNotificationModel(reExOrgModel, addOrgResponse, "www.someUrl.co.uk");

        res.Should().NotBeNull();
        res.CompanyName.Should().Be("Test Ltd");
        res.CompanyHouseNumber.Should().Be("12345678");

        res.OrganisationId.Should().Be("7ea62027-2bd9-4267-aeea-7f6dbc71824a");
        res.ReExInvitedApprovedPersons[0].FirstName.Should().Be("John");
        res.ReExInvitedApprovedPersons[0].InviteToken.Should().Be("www.someUrl.co.ukB786tgs12856%3d%3d");
    }
}
