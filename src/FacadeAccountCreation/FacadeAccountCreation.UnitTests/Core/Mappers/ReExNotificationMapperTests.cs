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

    [TestMethod]
    public void MapOrganisationModelToReExNotificationModel_CompanyJourney_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var company = new ReExCompanyModel
        {
            CompanyName = "Test Company",
            CompaniesHouseNumber = "12345678"
        };
        var reExUser = new ReExUserModel
        {
            UserId = userId,
            UserEmail = "user@test.com"
        };
        var invitedPerson = new ReExInvitedApprovedPerson
        {
            Email = "ap1@test.com",
            FirstName = "AP1",
            LastName = "Person",
            TelephoneNumber = "123456789"
        };
        var organisationModel = new ReExOrganisationModel
        {
            ReExUser = reExUser,
            Company = company,
            InvitedApprovedPersons = new List<ReExInvitedApprovedPerson> { invitedPerson }
        };
        var response = new ReExAddOrganisationResponse
        {
            UserFirstName = "First",
            UserLastName = "Last",
            OrganisationId = organisationId,
            ReferenceNumber = "REF123",
            InvitedApprovedUsers =
            [
                new InvitedApprovedUserResponse
                {
                    Email = "ap1@test.com",
                    InviteToken = "token123"
                }
            ]
        };
        var accountCreationUrl = "https://test.com/invite?token=";

        // Act
        var result = ReExNotificationMapper.MapOrganisationModelToReExNotificationModel(organisationModel, response, accountCreationUrl);

        // Assert
        result.UserId.Should().Be(userId.ToString());
        result.UserFirstName.Should().Be("First");
        result.UserLastName.Should().Be("Last");
        result.UserEmail.Should().Be("user@test.com");
        result.OrganisationId.Should().Be(organisationId.ToString());
        result.OrganisationExternalId.Should().Be(string.Empty);
        result.CompanyName.Should().Be("Test Company");
        result.CompanyHouseNumber.Should().Be("12345678");
        result.ReExInvitedApprovedPersons.Should().HaveCount(1);
        result.ReExInvitedApprovedPersons[0].FirstName.Should().Be("AP1");
        result.ReExInvitedApprovedPersons[0].InviteToken.Should().Contain("token123");
        result.ReExInvitedApprovedPersons[0].InviteToken.Should().StartWith(accountCreationUrl);
    }

    [TestMethod]
    public void MapOrganisationModelToReExNotificationModel_ManualInputJourney_MapsTradingNameAndReferenceNumber()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var manualInput = new ReExManualInputModel();
        var reExUser = new ReExUserModel
        {
            UserId = userId,
            UserEmail = "manual@test.com"
        };
        var invitedPerson = new ReExInvitedApprovedPerson
        {
            Email = "ap2@test.com",
            FirstName = "AP2",
            LastName = "Person",
            TelephoneNumber = "987654321"
        };
        var organisationModel = new ReExOrganisationModel
        {
            TradingName = "Manual Trader",
            ReExUser = reExUser,
            ManualInput = manualInput,
            InvitedApprovedPersons = new List<ReExInvitedApprovedPerson> { invitedPerson }
        };
        var response = new ReExAddOrganisationResponse
        {
            UserFirstName = "ManualFirst",
            UserLastName = "ManualLast",
            OrganisationId = organisationId,
            ReferenceNumber = "REF123",
            InvitedApprovedUsers = new List<InvitedApprovedUserResponse>
                {
                    new() {
                        Email = "ap2@test.com",
                        InviteToken = "token123"
                    }
                }
        };
        var accountCreationUrl = "https://test.com/invite?token=";

        // Act
        var result = ReExNotificationMapper.MapOrganisationModelToReExNotificationModel(organisationModel, response, accountCreationUrl);

        // Assert
        result.CompanyName.Should().Be("Manual Trader");
        result.CompanyHouseNumber.Should().Be("REF123");
        result.ReExInvitedApprovedPersons.Should().HaveCount(1);
        result.ReExInvitedApprovedPersons[0].FirstName.Should().Be("AP2");
        result.ReExInvitedApprovedPersons[0].InviteToken.Should().Contain("token123");
    }

    [TestMethod]
    public void MapOrganisationModelToReExNotificationModel_NoCompanyOrManualInput_CompanyNameAndNumberAreEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var reExUser = new ReExUserModel
        {
            UserId = userId,
            UserEmail = "empty@test.com"
        };
        var organisationModel = new ReExOrganisationModel
        {
            ReExUser = reExUser,
            InvitedApprovedPersons = new List<ReExInvitedApprovedPerson>()
        };
        var response = new ReExAddOrganisationResponse
        {
            UserFirstName = "firstName",
            UserLastName = "lastName",
            OrganisationId = organisationId,
            ReferenceNumber = "empty",
            InvitedApprovedUsers = []
        };
        var accountCreationUrl = "https://test.com/invite?token=";

        // Act
        var result = ReExNotificationMapper.MapOrganisationModelToReExNotificationModel(organisationModel, response, accountCreationUrl);

        // Assert
        result.CompanyName.Should().Be(string.Empty);
        result.CompanyHouseNumber.Should().Be(string.Empty);
        result.ReExInvitedApprovedPersons.Should().BeEmpty();
    }

    [TestMethod]
    public void MapOrganisationModelToReExNotificationModel_ApprovedPersonsWithNoMatchingInviteToken_ExcludedFromResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var reExUser = new ReExUserModel
        {
            UserId = userId,
            UserEmail = "user@test.com"
        };
        var invitedPerson = new ReExInvitedApprovedPerson
        {
            Email = "ap3@test.com",
            FirstName = "AP3",
            LastName = "Person",
            TelephoneNumber = "555555555"
        };
        var organisationModel = new ReExOrganisationModel
        {
            ReExUser = reExUser,
            Company = new ReExCompanyModel { CompanyName = "Test", CompaniesHouseNumber = "111" },
            InvitedApprovedPersons = new List<ReExInvitedApprovedPerson> { invitedPerson }
        };
        var response = new ReExAddOrganisationResponse
        {
            UserFirstName = "First",
            UserLastName = "Last",
            OrganisationId = organisationId,
            ReferenceNumber = "REF123",
            InvitedApprovedUsers =
                [
                    // No matching email for ap3@test.com
                    new InvitedApprovedUserResponse
                    {
                        Email = "other@test.com",
                        InviteToken = "othertoken"
                    }
                ]
        };
        var accountCreationUrl = "https://test.com/invite?token=";

        // Act
        var result = ReExNotificationMapper.MapOrganisationModelToReExNotificationModel(organisationModel, response, accountCreationUrl);

        // Assert
        result.ReExInvitedApprovedPersons.Should().BeEmpty();
    }

    [TestMethod]
    public void MapOrganisationModelToReExNotificationModel_NullOrEmptyInviteToken_ExcludedFromResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var reExUser = new ReExUserModel
        {
            UserId = userId,
            UserEmail = "user@test.com"
        };
        var invitedPerson = new ReExInvitedApprovedPerson
        {
            Email = "ap4@test.com",
            FirstName = "AP4",
            LastName = "Person",
            TelephoneNumber = "444444444"
        };
        var organisationModel = new ReExOrganisationModel
        {
            ReExUser = reExUser,
            Company = new ReExCompanyModel { CompanyName = "Test", CompaniesHouseNumber = "222" },
            InvitedApprovedPersons = new List<ReExInvitedApprovedPerson> { invitedPerson }
        };
        var response = new ReExAddOrganisationResponse
        {
            UserFirstName = "First",
            UserLastName = "Last",
            OrganisationId = organisationId,
            ReferenceNumber = "REF222",
            InvitedApprovedUsers =
                [
                    new InvitedApprovedUserResponse
                    {
                        Email = "ap4@test.com",
                        InviteToken = "" // Empty token
                    }
                ]
        };
        var accountCreationUrl = "https://test.com/invite?token=";

        // Act
        var result = ReExNotificationMapper.MapOrganisationModelToReExNotificationModel(organisationModel, response, accountCreationUrl);

        // Assert
        result.ReExInvitedApprovedPersons.Should().BeEmpty();
    }
}
