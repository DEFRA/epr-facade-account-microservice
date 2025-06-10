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

    [TestMethod]
    public void MapReExOrganisationModelToReExAddOrganisation_MapsCompanyDetails_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var company = new ReExCompanyModel
        {
            OrganisationId = "org-1",
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            CompaniesHouseNumber = "12345678",
            CompanyName = "Test Company",
            CompanyRegisteredAddress = new AddressModel
            {
                BuildingName = "Building",
                Street = "Street",
                Town = "Town",
                Postcode = "AB12 3CD",
                Country = "UK"
            },
            ValidatedWithCompaniesHouse = true,
            Nation = Nation.England,
            IsComplianceScheme = false
        };

        var invitedPerson = new ReExInvitedApprovedPerson
        {
            Role = "Director",
            FirstName = "Alice",
            LastName = "Smith",
            TelephoneNumber = "0123456789",
            Email = "alice.smith@example.com"
        };

        var organisationModel = new ReExOrganisationModel
        {
            ReExUser = new ReExUserModel
            {
                UserId = userId,
                IsApprovedUser = true
            },
            UserRoleInOrganisation = "Director",
            IsApprovedUser = true,
            Company = company,
            ManualInput = null,
            InvitedApprovedPersons = new List<ReExInvitedApprovedPerson> { invitedPerson }
        };

        // Act
        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(organisationModel);

        // Assert

        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.UserId.Should().Be(userId);
        result.User.JobTitle.Should().Be("Director");
        result.User.IsApprovedUser.Should().BeTrue();

        result.Organisation.Should().NotBeNull();
        result.Organisation.OrganisationId.Should().Be("org-1");
        result.Organisation.Nation.Should().Be(Nation.England);
        result.Organisation.OrganisationType.Should().Be(OrganisationType.CompaniesHouseCompany);
        result.Organisation.Name.Should().Be("Test Company");

        result.ManualInput.Should().BeNull();

        result.InvitedApprovedUsers.Should().NotBeNull();
        result.InvitedApprovedUsers.Should().HaveCount(1);  
        result.InvitedApprovedUsers[0].Person.FirstName.Should().Be("Alice");
        result.InvitedApprovedUsers[0].Person.LastName.Should().Be("Smith");
        result.InvitedApprovedUsers[0].Person.ContactEmail.Should().Be("alice.smith@example.com");

        result.Partners.Should().NotBeNull();
        result.Partners.Should().BeEmpty();
    }

    [TestMethod]
    public void MapReExOrganisationModelToReExAddOrganisation_MapsManualInputCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var manualInput = new ReExManualInputModel
        {
            TradingName = "Manual Org",
            ProducerType = ProducerType.SoleTrader,
            BusinessAddress = new AddressModel
            {
                BuildingName = "Manual Building",
                Street = "Manual Street",
                Town = "Manual Town",
                Postcode = "ZZ99 9ZZ",
                Country = "UK"
            }
        };

        var organisationModel = new ReExOrganisationModel
        {
            ReExUser = new ReExUserModel
            {
                UserId = userId,
                IsApprovedUser = false
            },
            UserRoleInOrganisation = "Member",
            IsApprovedUser = false,
            Company = null,
            ManualInput = manualInput,
            InvitedApprovedPersons = new List<ReExInvitedApprovedPerson>()
        };

        // Act
        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(organisationModel);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();   
        result.User.UserId.Should().Be(userId);
        result.User.JobTitle.Should().Be("Member");
        result.User.IsApprovedUser.Should().BeFalse();

        result.Organisation.Should().BeNull();

        result.ManualInput.Should().NotBeNull();
        result.ManualInput.TradingName.Should().Be("Manual Org");
        result.ManualInput.ProducerType.Should().Be(ProducerType.SoleTrader);
        result.ManualInput.BusinessAddress.Should().NotBeNull();
        result.ManualInput.BusinessAddress.BuildingName.Should().Be("Manual Building");
        result.ManualInput.BusinessAddress.Street.Should().Be("Manual Street");
        result.ManualInput.BusinessAddress.Town.Should().Be("Manual Town");
        result.ManualInput.BusinessAddress.Postcode.Should().Be("ZZ99 9ZZ");
        result.ManualInput.BusinessAddress.Country.Should().Be("UK");
        
        result.InvitedApprovedUsers.Should().NotBeNull();
        result.InvitedApprovedUsers.Should().BeEmpty();

        result.Partners.Should().NotBeNull();
        result.Partners.Should().BeEmpty();

        result.DeclarationTimeStamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void MapReExOrganisationModelToReExAddOrganisation_HandlesNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<NullReferenceException>(() =>
            ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(null));
    }

}