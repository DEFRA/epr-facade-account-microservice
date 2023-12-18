using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.ServiceRolesLookup;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ServiceRolesControllerTests
{
    private readonly Mock<IServiceRolesLookupService> _mockServiceRoleLookupService = new();
    private readonly NullLogger<RolesController> _nullLogger = new();
    private RolesController _sut = null!;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    
    [TestInitialize]
    public void Setup()
    {
        _sut = new RolesController(_nullLogger, _mockServiceRoleLookupService.Object);
    }
    
    [TestMethod]
    public void When_Service_Roles_Are_Requested_Then_Return_Service_Roles()
    {
        // Arrange
        var response = 
            _fixture.Create<List<ServiceRolesLookupModel>>();

        _mockServiceRoleLookupService
            .Setup(x => x.GetServiceRoles()).Returns(response);

        // Act
        var result = _sut.GetServiceRoles();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var obj = result.Result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public void When_Service_Roles_Are_Requested_And_No_Configuration_Exists_Then_Return_Service_Roles()
    {
        // Arrange
        _mockServiceRoleLookupService.Setup(x => x.GetServiceRoles()).Returns(new List<ServiceRolesLookupModel>());

        // Act
        var result = _sut.GetServiceRoles();

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult?.StatusCode.Should().Be(500);
    }
}
