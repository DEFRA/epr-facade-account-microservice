using System.Net;
using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;
using FacadeAccountCreation.Core.Models.ServiceRolesLookup;
using FacadeAccountCreation.Core.Services.Organisation;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using FacadeAccountCreation.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class OrganisationsControllerTests
{
    private readonly Guid _oid = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly int _serviceRoleId = 1;
    private readonly NullLogger<OrganisationsController> _nullLogger = new();
    private readonly Mock<IOrganisationService> _mockOrganisationService = new();
    private readonly Mock<IServiceRolesLookupService> _serviceRolesLookupServiceMock = new();
    private OrganisationsController _sut = null!;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Mock<HttpContext>? _httpContextMock;
    
    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _sut = new OrganisationsController(_nullLogger, _mockOrganisationService.Object, _serviceRolesLookupServiceMock.Object);
        _sut.AddDefaultContextWithOid(_oid, "TestAuth");
    }

    [TestMethod]
    public async Task Should_return_statuscode_404_when_GetOrganisationUsersList_throws_notfound()
    {
        // Arrange
        _mockOrganisationService.Setup(x => 
                x.GetOrganisationUserList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.NotFound));

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, _serviceRoleId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    
    [TestMethod]
    public async Task Should_return_statuscode_500_when_GetOrganisationUsersList_throws_500()
    {
        // Arrange
        _mockOrganisationService.Setup(x => 
            x.GetOrganisationUserList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.InternalServerError));

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, _serviceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as BadRequestResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task Should_return_organisation_user_list_if_no_exceptions()
    {
        // Arrange
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();
        
        _mockOrganisationService.Setup(x =>
            x.GetOrganisationUserList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, _serviceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }
    
    [TestMethod]
    public async Task Should_return_statuscode_200_when_Success()
    {
        // Arrange
        var apiResponse = _fixture.Create<OrganisationUser>();
        _mockOrganisationService.Setup(x => 
                x.GetOrganisationUserList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(new HttpResponseMessage{
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
        });

        _serviceRolesLookupServiceMock.Setup(x => x.GetServiceRoles()).Returns(new List<ServiceRolesLookupModel>());

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, _serviceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as OkObjectResult;
        statusCodeResult?.StatusCode.Should().Be(200);
    }
    
    [TestMethod]
    public async Task Should_return_500_statuscode_when_empty_user()
    {
        // Arrange
        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("emails", "_userEmail"),
            new(ClaimConstants.ObjectId, Guid.Empty.ToString()),
        }.AsEnumerable());
        _sut.ControllerContext.HttpContext = _httpContextMock.Object;

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, _serviceRoleId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var statusCodeResult = result as ObjectResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_Return_StatusCode_200_When_GetNationIdByOrganisationId_Succeeds()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expectedContent = "[1,2]";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };
        _mockOrganisationService.Setup(x => x.GetNationIdByOrganisationId(organisationId)).ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.GetNationIdByOrganisationId(organisationId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var resultValue = (result as OkObjectResult).Value as List<int>;
        resultValue.Should().Contain(1);
        resultValue.Should().Contain(2);
    }
    
    [TestMethod]
    public async Task Should_Return_StatusCode_400_When_GetNationIdByOrganisationId_Returns_BadRequest()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        _mockOrganisationService.Setup(x => x.GetNationIdByOrganisationId(organisationId)).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        // Act
        var result = await _sut.GetNationIdByOrganisationId(organisationId);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }
    
    [TestMethod]
    public async Task Should_Return_StatusCode_500_When_GetNationIdByOrganisationId_Returns_InternalServerError()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        _mockOrganisationService.Setup(x => x.GetNationIdByOrganisationId(organisationId)).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        var result = await _sut.GetNationIdByOrganisationId(organisationId);

        // Assert
        var statusCodeResult = result as StatusCodeResult;
        result.Should().BeOfType<StatusCodeResult>();
        statusCodeResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }
    
}
