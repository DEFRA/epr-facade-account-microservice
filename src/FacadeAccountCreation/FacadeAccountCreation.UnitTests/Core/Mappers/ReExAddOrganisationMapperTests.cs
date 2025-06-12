using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Models.Organisations.Mappers;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
                    Town = "Coventry",
                },
                ProducerType = ProducerType.LimitedLiabilityPartnership,
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
            UserRoleInOrganisation = "Director",
            Partners = 
            [
                new()
                {
                    Name = "PartnerTest",
                    PartnerRole = "PartnerRoleTest"
                }
            ]
        };

        var res = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(reExOrgModel);

        res.Should().NotBeNull();

        res.User.JobTitle.Should().Be("Director");
        res.User.IsApprovedUser.Should().BeTrue();
        res.User.UserId.Should().Be("fadc06db-ac47-4c3c-ad5a-b0c800288668");

        res.InvitedApprovedUsers.Should().HaveCount(0);
        res.InvitedApprovedUsers.Should().BeEmpty();

        res.Organisation.Name.Should().Be("Test Ltd");
        res.Organisation.CompaniesHouseNumber.Should().Be("12345678");
        res.Organisation.Address.BuildingName.Should().Be("XYZ");
        res.Organisation.Address.Postcode.Should().Be("CV1 9HB");
        res.Organisation.ProducerType.Should().Be(ProducerType.LimitedLiabilityPartnership);
        res.Partners[0].Name.Should().Be("PartnerTest");
        res.Partners[0].PartnerRole.Should().Be("PartnerRoleTest");
    }

    [TestMethod]
    [DataRow(OrganisationType.CompaniesHouseCompany, "CompaniesHouseCompany", Nation.Wales)]
    [DataRow(OrganisationType.NonCompaniesHouseCompany, "NonCompaniesHouseCompany", Nation.NorthernIreland)]
    [DataRow(OrganisationType.CompaniesHouseCompany, "CompaniesHouseCompany", Nation.Scotland)]
    [DataRow(OrganisationType.NonCompaniesHouseCompany, "NonCompaniesHouseCompany", Nation.England)]
    [DataRow(OrganisationType.NotSet, "NotSet", Nation.NotSet)]
    public void MapReExOrganisationModelToReExAddOrganisation_MapsCompanyDetails_Correctly(OrganisationType organisationType, string expectedResult, Nation nation)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var company = new ReExCompanyModel
        {
            OrganisationId = "org-1",
            OrganisationType = organisationType,
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
            Nation = nation,
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
            InvitedApprovedPersons = [invitedPerson]
        };

        // Act
        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(organisationModel);

        // Assert

        result.Organisation.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.UserId.Should().Be(userId);
        result.User.JobTitle.Should().Be("Director");
        result.User.IsApprovedUser.Should().BeTrue();

        result.Organisation.Should().NotBeNull();
        result.Organisation.OrganisationId.Should().Be("org-1");
        result.Organisation.Nation.Should().Be(nation);
        result.Organisation.OrganisationType.ToString().Should().Be(expectedResult);
        result.Organisation.Name.Should().Be("Test Company");

        result.InvitedApprovedUsers.Should().NotBeNull();
        result.InvitedApprovedUsers.Should().HaveCount(1);
        result.InvitedApprovedUsers[0].Person.FirstName.Should().Be("Alice");
        result.InvitedApprovedUsers[0].Person.LastName.Should().Be("Smith");
        result.InvitedApprovedUsers[0].Person.ContactEmail.Should().Be("alice.smith@example.com");

        result.Partners.Should().NotBeNull();
        result.Partners.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(OrganisationType.CompaniesHouseCompany, "CompaniesHouseCompany", Nation.Wales)]
    [DataRow(OrganisationType.NonCompaniesHouseCompany, "NonCompaniesHouseCompany", Nation.NorthernIreland)]
    [DataRow(OrganisationType.CompaniesHouseCompany, "CompaniesHouseCompany", Nation.Scotland)]
    [DataRow(OrganisationType.NonCompaniesHouseCompany, "NonCompaniesHouseCompany", Nation.England)]
    [DataRow(OrganisationType.NotSet, "NotSet", Nation.NotSet)]
    public void MapReExOrganisationModelToReExAddOrganisation_MapsManualInputCorrectly(OrganisationType organisationType, string expectedOrgType, Nation nation)
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
            },
            OrganisationType = organisationType,
            Nation = nation
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
            InvitedApprovedPersons = []
        };

        // Act
        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(organisationModel);

        // Assert
        result.Organisation.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.UserId.Should().Be(userId);
        result.User.JobTitle.Should().Be("Member");
        result.User.IsApprovedUser.Should().BeFalse();

        result.Organisation.Should().NotBeNull();

        result.Organisation.OrganisationType.ToString().Should().Be(expectedOrgType);

        result.Organisation.Name.Should().Be("Manual Org");
        result.Organisation.ProducerType.Should().Be(ProducerType.SoleTrader);
        result.Organisation.Address.Should().NotBeNull();
        result.Organisation.Address.BuildingName.Should().Be("Manual Building");
        result.Organisation.Address.Street.Should().Be("Manual Street");
        result.Organisation.Address.Town.Should().Be("Manual Town");
        result.Organisation.Address.Postcode.Should().Be("ZZ99 9ZZ");
        result.Organisation.Address.Country.Should().Be("UK");

        result.Organisation.Nation.Should().Be(nation);

        result.InvitedApprovedUsers.Should().NotBeNull();
        result.InvitedApprovedUsers.Should().BeEmpty();

        result.Partners.Should().NotBeNull();
        result.Partners.Should().BeEmpty();

        result.DeclarationTimeStamp.Should().NotBeOnOrAfter(DateTime.UtcNow);
    }

    [TestMethod]
    public void MapReExOrganisationModelToReExAddOrganisation_MapsManualInput_WithNullValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var manualInput = new ReExManualInputModel
        {
            TradingName = null,
            ProducerType = null,
            BusinessAddress = null
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
            InvitedApprovedPersons = []
        };

        // Act
        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(organisationModel);

        // Assert
        result.Organisation.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.UserId.Should().Be(userId);
        result.User.JobTitle.Should().Be("Member");
        result.User.IsApprovedUser.Should().BeFalse();

        result.Organisation.Should().NotBeNull();
        result.Organisation.Name.Should().BeNull();
        result.Organisation.ProducerType.Should().Be(ProducerType.NotSet);
        result.Organisation.Address.Should().BeNull();

        result.InvitedApprovedUsers.Should().NotBeNull();
        result.InvitedApprovedUsers.Should().BeEmpty();

        result.Partners.Should().NotBeNull();
        result.Partners.Should().BeEmpty();
    }

    [TestMethod]
    public void MapReExOrganisationModelToReExAddOrganisation_HandlesNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<NullReferenceException>(() =>
            ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(null));
    }

    [TestMethod]
    public void GetOrganisationModel_MapsCompanyCorrectly()
    {
        var userId = Guid.NewGuid();
        var company = new ReExCompanyModel
        {
            OrganisationId = "org-123",
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            CompaniesHouseNumber = "CH123456",
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
            Nation = Nation.Scotland,
            IsComplianceScheme = true
        };

        var orgModel = new ReExOrganisationModel
        {
            ReExUser = new ReExUserModel
            {
                UserId = userId,
                IsApprovedUser = false
            },
            Company = company,
            ManualInput = null
        };

        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(orgModel);

        result.Organisation.Should().NotBeNull();
        result.Organisation.OrganisationId.Should().Be("org-123");
        result.Organisation.OrganisationType.Should().Be(OrganisationType.CompaniesHouseCompany);
        result.Organisation.CompaniesHouseNumber.Should().Be("CH123456");
        result.Organisation.Name.Should().Be("Test Company");
        result.Organisation.Address.Should().NotBeNull();
        result.Organisation.Address.BuildingName.Should().Be("Building");
        result.Organisation.Nation.Should().Be(Nation.Scotland);
        result.Organisation.IsComplianceScheme.Should().BeTrue();
        result.Organisation.ValidatedWithCompaniesHouse.Should().BeTrue();
        result.Organisation.ProducerType.Should().BeNull();
    }

    [TestMethod]
    public void GetOrganisationModel_MapsManualInputCorrectly()
    {
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
            },
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            Nation = Nation.Wales
        };

        var orgModel = new ReExOrganisationModel
        {
            ReExUser = new ReExUserModel
            {
                UserId = userId,
                IsApprovedUser = false
            },
            Company = null,
            ManualInput = manualInput
        };

        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(orgModel);

        result.Organisation.Should().NotBeNull();
        result.Organisation.OrganisationId.Should().BeNull();
        result.Organisation.OrganisationType.Should().Be(OrganisationType.NonCompaniesHouseCompany);
        result.Organisation.CompaniesHouseNumber.Should().BeNull();
        result.Organisation.Name.Should().Be("Manual Org");
        result.Organisation.Address.Should().NotBeNull();
        result.Organisation.Address.BuildingName.Should().Be("Manual Building");
        result.Organisation.Nation.Should().Be(Nation.Wales);
        result.Organisation.IsComplianceScheme.Should().BeFalse();
        result.Organisation.ValidatedWithCompaniesHouse.Should().BeFalse();
        result.Organisation.ProducerType.Should().Be(ProducerType.SoleTrader);
    }

    [TestMethod]
    public void GetOrganisationModel_ManualInput_NullProperties_DefaultsSet()
    {
        var manualInput = new ReExManualInputModel
        {
            TradingName = null,
            ProducerType = null,
            BusinessAddress = null,
            OrganisationType = null,
            Nation = null
        };

        var orgModel = new ReExOrganisationModel
        {
            ReExUser = new ReExUserModel
            {
                UserId = Guid.NewGuid(),
                IsApprovedUser = false
            },
            Company = null,
            ManualInput = manualInput
        };

        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(orgModel);

        result.Organisation.Should().NotBeNull();
        result.Organisation.Name.Should().BeNull();
        result.Organisation.ProducerType.Should().Be(ProducerType.NotSet);
        result.Organisation.Address.Should().BeNull();
        result.Organisation.OrganisationType.Should().Be(OrganisationType.NotSet);
        result.Organisation.Nation.Should().Be(Nation.NotSet);
    }

    [TestMethod]
    public void GetOrganisationModel_Company_NullOptionalProperties_DefaultsSet()
    {
        var company = new ReExCompanyModel
        {
            OrganisationId = null,
            OrganisationType = null,
            CompaniesHouseNumber = null,
            CompanyName = null,
            CompanyRegisteredAddress = null,
            ValidatedWithCompaniesHouse = false,
            Nation = null,
            IsComplianceScheme = false
        };

        var orgModel = new ReExOrganisationModel
        {
            ReExUser = new ReExUserModel
            {
                UserId = Guid.NewGuid(),
                IsApprovedUser = false
            },
            Company = company,
            ManualInput = null
        };

        var result = ReExAddOrganisationMapper.MapReExOrganisationModelToReExAddOrganisation(orgModel);

        result.Organisation.Should().NotBeNull();
        result.Organisation.OrganisationId.Should().BeNull();
        result.Organisation.OrganisationType.Should().Be(OrganisationType.NotSet);
        result.Organisation.CompaniesHouseNumber.Should().BeNull();
        result.Organisation.Name.Should().BeNull();
        result.Organisation.Address.Should().BeNull();
        result.Organisation.Nation.Should().Be(Nation.NotSet);
        result.Organisation.IsComplianceScheme.Should().BeFalse();
        result.Organisation.ValidatedWithCompaniesHouse.Should().BeFalse();
        result.Organisation.ProducerType.Should().BeNull();
    }
}