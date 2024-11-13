using FacadeAccountCreation.Core.Models.User;
using FacadeAccountCreation.Core.Services.User;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class UsersControllerTests
{
    private readonly Guid _oid = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IMessagingService> _mockMessagingService = new();
    private readonly NullLogger<UsersController> _nullLogger = new();
    private UsersController _sut = null!;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Mock<HttpContext>? _httpContextMock;
    
    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _sut = new UsersController(
            _nullLogger, _mockUserService.Object, 
            _mockMessagingService.Object);
        _sut.AddDefaultContextWithOid(_oid, "TestAuth");
    }
    
    [TestMethod]
    public async Task Should_fetch_user_and_organisation_details_scheme_when_user_is_linked_to_organisation()
    {
        // Arrange
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockUserService
            .Setup(x => x.GetUserOrganisations(It.Is<Guid>(pc => pc == _userId)))
            .ReturnsAsync(handlerResponse);

        // Act

        var result = await _sut.GetOrganisation();

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }

    [TestMethod]
    public async Task Should_return_internal_server_error_when_fetching_organisations_for_users_throws_exception()
    {
        // Arrange
        _mockUserService
            .Setup(x => x.GetUserOrganisations(It.Is<Guid>(pc => pc == _userId)))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _sut.GetOrganisation();

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
     [TestMethod]
    public async Task Should_return_notfound_when_fetching_organisations_for_user_returns_notfound()
    {
        // Arrange
        _mockUserService
            .Setup(x => x.GetUserOrganisations(It.Is<Guid>(pc => pc == _userId)))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.NotFound));

        // Act
        var result = await _sut.GetOrganisation();

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as NotFoundResult;
        statusCodeResult?.StatusCode.Should().Be(404);
    }
    
    [TestMethod]
    public async Task Should_return_bad_request_when_fetching_organisations_for_user_returns_bad_request()
    {
        // Arrange
        _mockUserService
            .Setup(x => x.GetUserOrganisations(It.Is<Guid>(pc => pc == _userId)))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.BadRequest));

        // Act
        var result = await _sut.GetOrganisation();

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as BadRequestResult;
        statusCodeResult?.StatusCode.Should().Be(400);
    }
    
    [TestMethod]
    public async Task Should_return_statuscode_500_when_fetching_organisations_for_users_returns_500()
    {
        // Arrange
        _mockUserService
            .Setup(x => x.GetUserOrganisations(It.Is<Guid>(pc => pc == _userId)))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.InternalServerError));

        // Act
        var result = await _sut.GetOrganisation();

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_return_ok_when_sucess_statuscode()
    {
        // Arrange
        var apiResponse = _fixture.Create<UserOrganisationsListModel>();
        _mockUserService
            .Setup(x => x.GetUserOrganisations(It.IsAny<Guid>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            });

        // Act
        var result = await _sut.GetOrganisation();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
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
        var result = await _sut.GetOrganisation();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var statusCodeResult = result as ObjectResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_return_error_statuscode_when_error_response()
    {
        // Arrange
        var apiResponse = _fixture.Create<UserOrganisationsListModel>();
        _mockUserService
            .Setup(x => x.GetUserOrganisations(It.IsAny<Guid>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            });

        // Act
        var result = await _sut.GetOrganisation();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        var statusCodeResult = result as NotFoundResult;
        statusCodeResult?.StatusCode.Should().Be(404);
    }

    [TestMethod]
    [DataRow("Test Org", "OldJobTitle", "NewJobTitle")]
    [DataRow(null, "OldJobTitle", "NewJobTitle")]
    [DataRow("Test Org", null, "NewJobTitle")]
    [DataRow("Test Org", "OldJobTitle", null)]
    public async Task UpdatePersonalDetails_ReturnsOk_WhenUpdateIsSuccessfulAndEmailIsSent(
        string organisationName,
        string oldJobTitle,
        string newJobTitle)
    {
        // Arrange
        var updateUserDetailsRequest = new UpdateUserDetailsRequest();
        var serviceKey = "test-service-key";
        var organisationId = Guid.NewGuid();

        var responseContent = new UpdateUserDetailsResponse
        {
            HasTelephoneOnlyUpdated = false,
            HasBasicUserDetailsUpdated = true,
            HasApprovedOrDelegatedUserDetailsSentForApproval = true,
            ChangeHistory = new ChangeHistoryModel
            {
                Id = 1,
                PersonId = 123,
                OrganisationId = organisationId.GetHashCode(),
                OrganisationName = organisationName,
                Nation = "UK",
                CompaniesHouseNumber = "12345",
                OldValues = new UserDetailsChangeModel
                {
                    FirstName = "OldFirstName",
                    LastName = "OldLastName",
                    JobTitle = oldJobTitle
                },
                NewValues = new UserDetailsChangeModel
                {
                    FirstName = "NewFirstName",
                    LastName = "NewLastName",
                    JobTitle = newJobTitle
                },
                IsActive = true,
                ApproverComments = "Approved",
                ApprovedById = 456,
                DecisionDate = DateTimeOffset.UtcNow,
                DeclarationDate = DateTimeOffset.UtcNow.AddDays(-1),
                ExternalId = Guid.NewGuid(),
                CreatedOn = DateTimeOffset.UtcNow.AddMonths(-1),
                LastUpdatedOn = DateTimeOffset.UtcNow,
                Telephone = "1234567890",
                EmailAddress = "test@example.com"
            }
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent))
        };

        _mockUserService.Setup(s => s.UpdatePersonalDetailsAsync(It.IsAny<Guid>(), organisationId, serviceKey, updateUserDetailsRequest))
            .ReturnsAsync(httpResponse);

        _mockMessagingService.Setup(m => m.SendUserDetailChangeRequestEmailToRegulator(It.IsAny<UserDetailsChangeNotificationEmailInput>()))
            .Returns("notificationId");

        // Act
        var result = await _sut.UpdatePersonalDetails(updateUserDetailsRequest, serviceKey, organisationId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [TestMethod]
    public async Task UpdatePersonalDetails_ReturnsOk_WhenUpdateIsSuccessfulAndNoEmailIsSent()
    {
        // Arrange
        var updateUserDetailsRequest = new UpdateUserDetailsRequest();
        var serviceKey = "test-service-key";
        var organisationId = Guid.NewGuid();

        var responseContent = new UpdateUserDetailsResponse
        {
            HasTelephoneOnlyUpdated = false,
            HasBasicUserDetailsUpdated = true,
            HasApprovedOrDelegatedUserDetailsSentForApproval = false,
            ChangeHistory = null
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent))
        };

        _mockUserService.Setup(s => s.UpdatePersonalDetailsAsync(It.IsAny<Guid>(), organisationId, serviceKey, updateUserDetailsRequest))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _sut.UpdatePersonalDetails(updateUserDetailsRequest, serviceKey, organisationId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [TestMethod]
    public async Task UpdatePersonalDetails_ReturnsBadRequest_WhenUserServiceFails()
    {
        // Arrange
        var updateUserDetailsRequest = new UpdateUserDetailsRequest();
        var serviceKey = "test-service-key";
        var organisationId = Guid.NewGuid();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockUserService.Setup(s => s.UpdatePersonalDetailsAsync(It.IsAny<Guid>(), organisationId, serviceKey, updateUserDetailsRequest))
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _sut.UpdatePersonalDetails(updateUserDetailsRequest, serviceKey, organisationId);

        // Assert
        var badRequestResult = result as StatusCodeResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task UpdatePersonalDetails_HandlesExceptionDuringUpdate()
    {
        // Arrange
        var updateUserDetailsRequest = new UpdateUserDetailsRequest();
        var serviceKey = "test-service-key";
        var organisationId = Guid.NewGuid();

        _mockUserService.Setup(s => s.UpdatePersonalDetailsAsync(It.IsAny<Guid>(), organisationId, serviceKey, updateUserDetailsRequest))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _sut.UpdatePersonalDetails(updateUserDetailsRequest, serviceKey, organisationId);

        // Assert
        var statusResult = result as StatusCodeResult;
        Assert.IsNotNull(statusResult);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [TestMethod]
    public async Task UpdatePersonalDetails_HandlesExceptionDuringEmailNotification()
    {
        // Arrange
        var updateUserDetailsRequest = new UpdateUserDetailsRequest();
        var serviceKey = "test-service-key";
        var organisationId = Guid.NewGuid();
        var responseContent = _fixture.Create<UpdateUserDetailsResponse>();
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent))
        };

        _mockUserService.Setup(s => s.UpdatePersonalDetailsAsync(It.IsAny<Guid>(), organisationId, serviceKey, updateUserDetailsRequest))
            .ReturnsAsync(httpResponse);

        _mockMessagingService.Setup(m => m.SendUserDetailChangeRequestEmailToRegulator(It.IsAny<UserDetailsChangeNotificationEmailInput>()))
            .Throws(new Exception("Email sending failed"));

        // Act
        var result = await _sut.UpdatePersonalDetails(updateUserDetailsRequest, serviceKey, organisationId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
    }
}
