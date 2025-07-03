using FacadeAccountCreation.Core.Services.CreateAccount;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ReprocessorExporterAccountsControllerTests
{
    private const string ServiceKey = "ServiceKey";
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Guid _userId;
    private string _userEmail;
    private readonly Mock<IAccountService> _mockAccountServiceMock = new();
    private readonly Mock<HttpContext> _httpContextMock = new();
    private Mock<IMessagingService> _messagingServiceMock;
    private ReprocessorExporterAccountsController? _sut;

    [TestInitialize]
    public void Setup()
    {
        _messagingServiceMock = new Mock<IMessagingService>();

        _sut = new ReprocessorExporterAccountsController(_mockAccountServiceMock.Object, _messagingServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = _httpContextMock.Object
            }
        };

        _userId = _fixture.Create<Guid>();
        _userEmail = "email@example.com";

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new ("emails", _userEmail),
            new (ClaimConstants.ObjectId, _userId.ToString())
        }.AsEnumerable());
    }

    [TestMethod]
    public async Task CreateAccount_HappyPath_ReturnsOk()
    {
        // Arrange
        var account = _fixture.Create<ReprocessorExporterAccountModel>();
        account.Person.ContactEmail = _userEmail;

        // Act
        var result = await _sut!.CreateAccount(account, ServiceKey);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }


    [TestMethod]
    public async Task CreateAccount_HappyPath_CallsAddReprocessorExporterAccountAsync()
    {
        // Arrange
        var account = _fixture.Create<ReprocessorExporterAccountModel>();

        _mockAccountServiceMock
            .Setup(x => x.AddReprocessorExporterAccountAsync(It.IsAny<ReprocessorExporterAccountWithUserModel>(), ServiceKey));

        // Act
        await _sut!.CreateAccount(account, ServiceKey);

        // Assert
        _mockAccountServiceMock.Verify(x =>
                x.AddReprocessorExporterAccountAsync(It.Is<ReprocessorExporterAccountWithUserModel>(a =>
                    a.User.UserId == _userId
                    && a.User.Email == _userEmail
                    && a.User.ExternalIdpUserId == null
                    && a.User.ExternalIdpId == null
                    && a.Person.FirstName == account.Person.FirstName
                    && a.Person.LastName == account.Person.LastName
                    && a.Person.TelephoneNumber == account.Person.TelephoneNumber
                    && a.Person.ContactEmail == account.Person.ContactEmail),
                    ServiceKey),
            Times.Once);
    }
}
