using FacadeAccountCreation.Core.Models.ServiceRolesLookup;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class ServiceRolesLookupServiceTests
{
    private IOptions<ServiceRolesConfig> _serviceRolesConfigOptions = default!;
    private IOptions<ServiceRolesConfig> _serviceRolesEmptyConfigOptions = default!;

    [TestInitialize]
    public void Setup()
    {
        _serviceRolesEmptyConfigOptions = Options.Create(new ServiceRolesConfig());
        _serviceRolesConfigOptions = Options.Create(new ServiceRolesConfig()
        {
            Roles = new List<ServiceRolesLookupModel>
            {
                new ServiceRolesLookupModel
                {
                     Key = "Basic.Admin",
                     PersonRoleId = 1,
                     ServiceRoleId = 3,
                     DescriptionKey = "Basic.AdminDescriptionKey"
                },
                new ServiceRolesLookupModel
                {
                    Key = "Basic.Employee",
                    PersonRoleId = 2,
                    ServiceRoleId = 3
                }
            }
        });
    }

    [TestMethod]
    public void When_Get_Service_Roles_Called_Should_Return_Successful_Not_Null_Response()
    {
        // Arrange
        var expected = new List<ServiceRolesLookupModel>()
        {
            new()
            {
                Key = "Basic.Employee",
                PersonRoleId = 2,
                ServiceRoleId = 3
                
            },
            new()
            {
                Key = "Basic.Admin",
                PersonRoleId = 1,
                ServiceRoleId = 3,
                DescriptionKey = "Basic.AdminDescriptionKey"
            }
        };
        
        var sut = new ServiceRolesLookupService(_serviceRolesConfigOptions);
        
        // Act
        var response = sut.GetServiceRoles();

        // Assert
        response.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    public void When_Get_Service_Roles_Called_Should_Return_Null_Response()
    {
        // Arrange
        var sut = new ServiceRolesLookupService(_serviceRolesEmptyConfigOptions);
        
        // Act
        var response = sut.GetServiceRoles();

        // Assert
        response.Should().BeNull();
    }
    
    [TestMethod]
    public void Service_Roles_Includes_DescriptionKey()
    {
        // Arrange
        var sut = new ServiceRolesLookupService(_serviceRolesConfigOptions);
        
        // Act
        var response = sut.GetServiceRoles();

        // Assert
        response[0].DescriptionKey.Should().NotBeNull();
        response[1].DescriptionKey.Should().BeNull();
    }
    
    [TestMethod]
    public void When_Service_RoleId_Is_Passed_As_RegulatorAdmin_Is_Producer_Returns_False()
    {
        // Arrange
        var sut = new ServiceRolesLookupService(_serviceRolesConfigOptions);
        
        // Act
        var response = sut.IsProducer(4);

        // Assert
        response.Should().BeFalse();
    }
    
    [TestMethod]
    public void When_Service_RoleId_Is_Passed_As_ProducerApprovedPerson_Is_Producer_Returns_True()
    {
        // Arrange
        var sut = new ServiceRolesLookupService(_serviceRolesConfigOptions);
        
        // Act
        var response = sut.IsProducer(1);

        // Assert
        response.Should().BeTrue();
    }
}