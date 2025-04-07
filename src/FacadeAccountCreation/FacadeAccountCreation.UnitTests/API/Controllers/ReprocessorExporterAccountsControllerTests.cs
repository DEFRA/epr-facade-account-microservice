using FacadeAccountCreation.Core.Services.CreateAccount;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ReprocessorExporterAccountsControllerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IAccountService> _mockAccountServiceMock = new();
    private readonly Mock<IMessagingService> _mockMessagingService = new();
    private readonly Mock<HttpContext> _httpContextMock = new();
    private ReprocessorExporterAccountsController? _sut;

    [TestInitialize]
    public void Setup()
    {
        _sut = new ReprocessorExporterAccountsController(_mockAccountServiceMock.Object, _mockMessagingService.Object)
        {
            ControllerContext =
            {
                HttpContext = _httpContextMock.Object
            }
        };

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new ("emails", "abc@efg.com"),
            new (ClaimConstants.ObjectId, _fixture.Create<Guid>().ToString())
        }.AsEnumerable());
    }

    [TestMethod]
    public async Task CreateAccount_HappyPath_ReturnsOk()
    {
        // Arrange
        var account = _fixture.Create<ReprocessorExporterAccountModel>();

        // Act
        var result = await _sut!.CreateAccount(account);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkResult>();
    }


    [TestMethod]
    public async Task CreateAccount_HappyPath_CallsAddReprocessorExporterAccountAsync()
    {
        // Arrange
        var account = _fixture.Create<ReprocessorExporterAccountModel>();

        _mockAccountServiceMock
            .Setup(x => x.AddReprocessorExporterAccountAsync(It.IsAny<ReprocessorExporterAccountWithUserModel>()));

        // Act
        await _sut!.CreateAccount(account);

        // Assert
        //todo: check correct model passed
        _mockAccountServiceMock.Verify(x => x.AddReprocessorExporterAccountAsync(It.IsAny<ReprocessorExporterAccountWithUserModel>()), Times.Once);
    }
}
