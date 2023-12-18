using FacadeAccountCreation.Core.Enums;
using FacadeAccountCreation.Core.Helpers;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;
using FacadeAccountCreation.Core.Models.ServiceRolesLookup;
using FluentAssertions;

namespace FacadeAccountCreation.UnitTests.Core.Helpers;

[TestClass]
public class OrganisationUserHelperTests
{
    private readonly List<ServiceRolesLookupModel> _rolesLookupModels;
    
    public OrganisationUserHelperTests()
    {
        _rolesLookupModels = new List<ServiceRolesLookupModel>
        {
            new ServiceRolesLookupModel()
            {
                ServiceRoleId = 3,
                PersonRoleId = 1,
                Key = "Basic.Admin",
            },
            new ServiceRolesLookupModel()
            {
                ServiceRoleId = 3,
                PersonRoleId = 2,
                Key = "Basic.Employee",
            },            
        };
    }
    
    [TestMethod]
    public void User_with_BasicAdmin_should_return_BasicAdmin()
    {
        // Arrange
        var organisationUser = new OrganisationUser
        {
            PersonRoleId = 1,
            Enrolments = new List<OrganisationUserEnrolment>()
            {
                new OrganisationUserEnrolment()
                {
                    ServiceRoleId = 3,
                    EnrolmentStatus = EnrolmentStatus.Approved
                },
            }
        };

        // Act
        var highestServiceRole = OrganisationUserHelper.GetHighestRole(organisationUser, _rolesLookupModels);

        // Assert
        highestServiceRole.ServiceRoleId.Should().Be(3);
        highestServiceRole.Key.Should().Be("Basic.Admin");
    }
    
    [TestMethod]
    public void User_with_BasicEmployee_should_return_BasicEmployee()
    {
        // Arrange
        var organisationUser = new OrganisationUser
        {
            PersonRoleId = 2,
            Enrolments = new List<OrganisationUserEnrolment>()
            {
                new OrganisationUserEnrolment()
                {
                    ServiceRoleId = 3,
                    EnrolmentStatus = EnrolmentStatus.Approved
                },
            }
        };

        // Act
        var highestServiceRole = OrganisationUserHelper.GetHighestRole(organisationUser, _rolesLookupModels);

        // Assert
        highestServiceRole.ServiceRoleId.Should().Be(3);
        highestServiceRole.Key.Should().Be("Basic.Employee");
    }
    
    [TestMethod]
    [DataRow(2,1)]
    [DataRow(2,2)]
    [DataRow(3,3)]
    [DataRow(5,5)]
    public void User_with_no_matching_config_should_return_Null(int serviceRoleId, int personRoleId)
    {
        // Arrange
        var organisationUser = new OrganisationUser
        {
            PersonRoleId = personRoleId,
            Enrolments = new List<OrganisationUserEnrolment>()
            {
                new()
                {
                    ServiceRoleId = serviceRoleId,
                    EnrolmentStatus = EnrolmentStatus.Approved
                },
            }
        };

        // Act
        var highestServiceRole = OrganisationUserHelper.GetHighestRole(organisationUser, _rolesLookupModels);

        // Assert
        highestServiceRole.Should().BeNull();
    }
}
