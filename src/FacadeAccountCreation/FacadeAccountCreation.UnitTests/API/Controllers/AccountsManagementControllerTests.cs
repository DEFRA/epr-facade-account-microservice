using FacadeAccountCreation.Core.Models.ServiceRolesLookup;
using FacadeAccountCreation.Core.Services.CreateAccount;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class AccountsManagementControllerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IAccountService> _mockAccountServiceMock = new();
    private readonly Mock<HttpContext> _httpContextMock = new();
    private readonly Mock<IMessagingService> _mockMessagingService = new();
    private readonly Mock<IServiceRolesLookupService> _serviceRolesLookupServiceMock = new();
    private readonly NullLogger<AccountsManagementController> _logger = new();
    private readonly Guid _oid = Guid.NewGuid();
    private readonly string _userEmail = "abc@efg.com";
    private AccountsManagementController _sut = default!;
    
    [TestInitialize]
    public void Setup()
    {
        _serviceRolesLookupServiceMock.Setup(x => x.GetServiceRoles())
            .Returns([
                new()
                {
                    Key = "Basic.Admin",
                    ServiceRoleId = 3,
                    PersonRoleId = 1,
                    InvitationTemplateId = Guid.NewGuid().ToString()
                },

                new()
                {
                    Key = "Basic.Employee",
                    ServiceRoleId = 3,
                    PersonRoleId = 2,
                    InvitationTemplateId = Guid.NewGuid().ToString()
                }
            ]);

        var messagingConfigOptions = Options.Create(new MessagingConfig
        {
            AccountCreationUrl = "https://localhost:5001/Account/ConfirmAccountCreation"
        });
        
        _sut = new AccountsManagementController(
            _mockAccountServiceMock.Object, 
            _mockMessagingService.Object, 
            messagingConfigOptions, 
            _serviceRolesLookupServiceMock.Object,
            _logger);
        
        _sut.ControllerContext.HttpContext = _httpContextMock.Object;

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("emails", _userEmail),
            new(ClaimConstants.ObjectId, _oid.ToString()),
        }.AsEnumerable());
    }
    
    [TestMethod]
    public async Task ShouldReturnBadRequest_When_AccountInvitationModelIsInvalid()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        _sut.ModelState.AddModelError("test", "test");

        // Act
        var result = await _sut.InviteUser(accountInvitationModel) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(400);
        _mockMessagingService.Verify(x => x.SendInviteToUser(It.IsAny<InviteUserEmailInput>()), Times.Never);
    }
    
    [TestMethod]
    public async Task ShouldReturnBadRequest_When_UserWasInvitedBefore()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        accountInvitationModel.InvitedUser.Rolekey = "Basic.Admin";
        var sendInviteResponse = _fixture.Create<Task<HttpResponseMessage>>();
        sendInviteResponse.Result.StatusCode = HttpStatusCode.BadRequest;
        sendInviteResponse.Result.Content = new StringContent($"User '{accountInvitationModel.InvitedUser.Email}' is already invited");

        _mockAccountServiceMock.Setup(x => x.SaveInviteAsync(It.IsAny<AccountInvitationModel>()))
            .Returns(sendInviteResponse);

        // Act
        var result = await _sut.InviteUser(accountInvitationModel) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(400);
        _mockMessagingService.Verify(x => x.SendInviteToUser(It.IsAny<InviteUserEmailInput>()), Times.Never);
    }
    
    [TestMethod]
    public async Task ShouldReturnBadRequest_When_UserWasEnrolledAlready()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        accountInvitationModel.InvitedUser.Rolekey = "Basic.Admin";
        var sendInviteResponse = _fixture.Create<Task<HttpResponseMessage>>();
        sendInviteResponse.Result.StatusCode = HttpStatusCode.BadRequest;
        sendInviteResponse.Result.Content = new StringContent($"Invited user '{accountInvitationModel.InvitedUser.Email}' is enrolled already");

        _mockAccountServiceMock.Setup(x => x.SaveInviteAsync(It.IsAny<AccountInvitationModel>()))
            .Returns(sendInviteResponse);

        // Act
        var result = await _sut.InviteUser(accountInvitationModel) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(400);
        _mockMessagingService.Verify(x => x.SendInviteToUser(It.IsAny<InviteUserEmailInput>()), Times.Never);
    }
    
    [TestMethod]
    public async Task ShouldReturnBadRequest_When_UserDoesNotBelongToSameOrganisation()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        accountInvitationModel.InvitedUser.Rolekey = "Basic.Admin";
        var sendInviteResponse = _fixture.Create<Task<HttpResponseMessage>>();
        sendInviteResponse.Result.StatusCode = HttpStatusCode.BadRequest;
        sendInviteResponse.Result.Content = new StringContent($"Invited user '{accountInvitationModel.InvitedUser.Email}' doesn't belong to the same organisation");

        _mockAccountServiceMock.Setup(x => x.SaveInviteAsync(It.IsAny<AccountInvitationModel>()))
            .Returns(sendInviteResponse);

        // Act
        var result = await _sut.InviteUser(accountInvitationModel) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(400);
        _mockMessagingService.Verify(x => x.SendInviteToUser(It.IsAny<InviteUserEmailInput>()), Times.Never);
    }

    [TestMethod]
    public async Task AccountsController_ShouldCallEmailService_WhenInvitationCreatedForAdmin()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        accountInvitationModel.InvitedUser.Rolekey = "Basic.Admin";

        SetupSuccessfulHttpResponseMessage();
        
        // Act
        var result = await _sut.InviteUser(accountInvitationModel) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(200);
        _mockMessagingService.Verify(x => x.SendInviteToUser(It.IsAny<InviteUserEmailInput>()), Times.Once);
    }
    
    [TestMethod]
    public async Task AccountsController_ShouldReturnInternalServerError_WhenRoleKeyIsNotFoundInConfig()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        accountInvitationModel.InvitedUser.Rolekey = "Basic.Admin1";

        SetupSuccessfulHttpResponseMessage();
        
        // Act
        var result = await _sut.InviteUser(accountInvitationModel) as ObjectResult;

        // Assert
        result.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task AccountsController_ShouldCallEmailService_WhenInvitationCreatedForEmployee()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        accountInvitationModel.InvitedUser.Rolekey = "Basic.Employee";
        
        SetupSuccessfulHttpResponseMessage();

        // Act
        var result = await _sut.InviteUser(accountInvitationModel);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockMessagingService.Verify(x => x.SendInviteToUser(It.IsAny<InviteUserEmailInput>()), Times.Once);
    }
    
    [TestMethod]
    public async Task EnrolInvitedUser_Should_Add_User_Data_And_Call_Backend()
    {
        // Arrange
        var enrolInvitedUserModel = _fixture.Build<EnrolInvitedUserModel>()
            .Without(x => x.UserId)
            .Without(x => x.Email)
            .Create();

        _mockAccountServiceMock
            .Setup(x => x.EnrolInvitedUserAsync(It.Is<EnrolInvitedUserModel>(x => 
                x.UserId == _oid && 
                x.Email == _userEmail &&
                x.FirstName == enrolInvitedUserModel.FirstName &&
                x.LastName == enrolInvitedUserModel.LastName &&
                x.InviteToken == enrolInvitedUserModel.InviteToken)))
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        
        // Act
        var result = await _sut.EnrolInvitedUser(enrolInvitedUserModel);
        
        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockAccountServiceMock.Verify(x => x.EnrolInvitedUserAsync(It.Is<EnrolInvitedUserModel>(x => 
            x.UserId == _oid && 
            x.Email == _userEmail &&
            x.FirstName == enrolInvitedUserModel.FirstName &&
            x.LastName == enrolInvitedUserModel.LastName &&
            x.InviteToken == enrolInvitedUserModel.InviteToken)), Times.Once);
    }

    [TestMethod]
    public async Task EnrolInvitedUser_Should_Return_Problem_When_Backend_Returns_BadRequest()
    {
        // Arrange
        var enrolInvitedUserModel = _fixture.Build<EnrolInvitedUserModel>()
            .Without(x => x.UserId)
            .Without(x => x.Email)
            .Create();

        _mockAccountServiceMock
            .Setup(x => x.EnrolInvitedUserAsync(It.Is<EnrolInvitedUserModel>(x => 
                x.UserId == _oid && 
                x.Email == _userEmail &&
                x.FirstName == enrolInvitedUserModel.FirstName &&
                x.LastName == enrolInvitedUserModel.LastName &&
                x.InviteToken == enrolInvitedUserModel.InviteToken)))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });
        
        // Act
        var result = await _sut.EnrolInvitedUser(enrolInvitedUserModel);
        
        // Assert
        _mockAccountServiceMock.Verify(x => x.EnrolInvitedUserAsync(It.Is<EnrolInvitedUserModel>(x => 
            x.UserId == _oid && 
            x.Email == _userEmail &&
            x.FirstName == enrolInvitedUserModel.FirstName &&
            x.LastName == enrolInvitedUserModel.LastName &&
            x.InviteToken == enrolInvitedUserModel.InviteToken)), Times.Once);
        result.Should().NotBeNull();
        result?.Should().BeOfType<ObjectResult>();
        (result as ObjectResult).StatusCode.Should().Be(400);
    }
    
    [TestMethod]
    public async Task EnrolInvitedUser_Should_Return_Problem_When_Backend_Returns_Error()
    {
        // Arrange
        var enrolInvitedUserModel = _fixture.Build<EnrolInvitedUserModel>()
            .Without(x => x.UserId)
            .Without(x => x.Email)
            .Create();

        _mockAccountServiceMock
            .Setup(x => x.EnrolInvitedUserAsync(It.Is<EnrolInvitedUserModel>(x => 
                x.UserId == _oid && 
                x.Email == _userEmail &&
                x.FirstName == enrolInvitedUserModel.FirstName &&
                x.LastName == enrolInvitedUserModel.LastName &&
                x.InviteToken == enrolInvitedUserModel.InviteToken)))
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.InternalServerError
                    });
        
        // Act
        var result = await _sut.EnrolInvitedUser(enrolInvitedUserModel);
        
        // Assert
        _mockAccountServiceMock.Verify(x => x.EnrolInvitedUserAsync(It.Is<EnrolInvitedUserModel>(x => 
            x.UserId == _oid && 
            x.Email == _userEmail &&
            x.FirstName == enrolInvitedUserModel.FirstName &&
            x.LastName == enrolInvitedUserModel.LastName &&
            x.InviteToken == enrolInvitedUserModel.InviteToken)), Times.Once);
        result.Should().NotBeNull();
        result?.Should().BeOfType<ObjectResult>();
        (result as ObjectResult).StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task AccountsController_ShouldReturnBadRequest_WhenSendInviteResponsesBadRequest()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        accountInvitationModel.InvitedUser.Rolekey = "Basic.Admin";

        var sendInviteResponse = _fixture.Create<Task<HttpResponseMessage>>();
        
            sendInviteResponse.Result.StatusCode = HttpStatusCode.BadRequest;
            sendInviteResponse.Result.Content = new StringContent("token");
            _mockAccountServiceMock.Setup(x => x.SaveInviteAsync(It.IsAny<AccountInvitationModel>())).Returns(sendInviteResponse);
       
        
        // Act
        var result = await _sut.InviteUser(accountInvitationModel) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    [TestMethod]
    public async Task AccountsController_ShouldReturnBadRequest_WhenSendInviteResponsesNotFound()
    {
        // Arrange
        var accountInvitationModel = _fixture.Create<AccountInvitationModel>();
        accountInvitationModel.InvitedUser.Rolekey = "Basic.Admin";

        var sendInviteResponse = _fixture.Create<Task<HttpResponseMessage>>();
        
        sendInviteResponse.Result.StatusCode = HttpStatusCode.NotFound;
        sendInviteResponse.Result.Content = new StringContent("token");
        _mockAccountServiceMock.Setup(x => x.SaveInviteAsync(It.IsAny<AccountInvitationModel>())).Returns(sendInviteResponse);
       
        
        // Act
        var result = await _sut.InviteUser(accountInvitationModel) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task ShouldReturnBadRequest_When_EnrolInvitedUserModelIsInvalid()
    {
        // Arrange
        var enrolInvitedUserModel = _fixture.Create<EnrolInvitedUserModel>();
        _sut.ModelState.AddModelError("test", "test");

        // Act
        var result = await _sut.EnrolInvitedUser(enrolInvitedUserModel) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(400);
        _mockAccountServiceMock.Verify(x => x.EnrolInvitedUserAsync(It.IsAny<EnrolInvitedUserModel>()), Times.Never);
    }

    private void SetupSuccessfulHttpResponseMessage()
    {
        var sendInviteResponse = _fixture.Create<Task<HttpResponseMessage>>();
        if(sendInviteResponse != null)
        {
            sendInviteResponse.Result.StatusCode = HttpStatusCode.OK;
            sendInviteResponse.Result.Content = new StringContent("token");
            _mockAccountServiceMock.Setup(x => x.SaveInviteAsync(It.IsAny<AccountInvitationModel>())).Returns(sendInviteResponse);
        }
    }
}