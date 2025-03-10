using FacadeAccountCreation.Core.Services.CreateAccount;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class AccountsControllerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IAccountService> _mockAccountServiceMock = new();
    private readonly Mock<IMessagingService> _mockMessagingService = new();
    private readonly Mock<HttpContext> _httpContextMock = new();
    private AccountsController _sut = default!;

    [TestInitialize]
    public void Setup()
    {
        _sut = new AccountsController(_mockAccountServiceMock.Object, _mockMessagingService.Object)
        {
            ControllerContext =
            {
                HttpContext = _httpContextMock.Object
            }
        };
        
        _httpContextMock.Setup(x=> x.User.Claims).Returns(new List<Claim>
        {
            new ("emails", "abc@efg.com"),
            new (ClaimConstants.ObjectId, _fixture.Create<Guid>().ToString())
        }.AsEnumerable());
    }
    
    [TestMethod]
    public async Task ShouldAcceptAccountModel_ThenPostWith_AccountWithUserModel()
    {
        // Arrange
        var account = _fixture.Create<AccountModel>();

        _mockAccountServiceMock
            .Setup(x => x.AddAccountAsync(It.IsAny<AccountWithUserModel>()));

        // Act
        var result = await _sut.CreateAccount(account);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        _mockAccountServiceMock.Verify(x => x.AddAccountAsync(It.IsAny<AccountWithUserModel>()), Times.Once);
    }
    
    [TestMethod]
    public async Task ShouldAcceptAccountModel_ThenPostWith_CorrectModelValue()
    {
        // Arrange
        var account = _fixture.Create<AccountModel>();

        _mockAccountServiceMock
            .Setup(x => x.AddAccountAsync(It.IsAny<AccountWithUserModel>())).ReturnsAsync(new CreateAccountResponse());

        // Act
        var result = await _sut.CreateAccount(account);

        // Assert
        result.Should().BeOfType<OkResult>();
        _mockAccountServiceMock.Verify(x => x.AddAccountAsync(It.IsAny<AccountWithUserModel>()), Times.Once);
        _mockMessagingService.Verify(x => x.SendAccountCreationConfirmation(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(200);
    }
    
    [TestMethod]
    public async Task ShouldAcceptApprovedUserAccountModel_ThenPostWith_CorrectModelValue()
    {
        // Arrange
        var account = _fixture.Create<AccountModel>();

        _mockAccountServiceMock
            .Setup(x => x.AddApprovedUserAccountAsync(It.IsAny<AccountModel>())).ReturnsAsync(new CreateAccountResponse());

        // Act
        var result = await _sut.CreateApprovedUserAccount(account);

        // Assert
        result.Should().BeOfType<OkResult>();
        
        _mockAccountServiceMock.Verify(x => x.AddApprovedUserAccountAsync(It.IsAny<AccountModel>()), Times.Once);
        _mockMessagingService.Verify(x => x.SendApprovedUserAccountCreationConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var obj = result as ObjectResult;
        obj?.Value.Should().BeEquivalentTo(200);
    }
}
