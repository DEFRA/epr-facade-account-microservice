using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;
using FacadeAccountCreation.Core.Services.Organisation;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ReprocessorExporterOrganisationControllerTests
{
    private IFixture _fixture;
    private Guid _userId;
    private string _userEmail;
    private const string ServiceKey = "ServiceKey";
    private Mock<IOrganisationService> _organisationServiceMock;
    private Mock<IMessagingService> _messagingServiceMock;
    private readonly Mock<HttpContext> _httpContextMock = new();
    private ReprocessorExporterOrganisationController? _sut;
    private readonly NullLogger<ReprocessorExporterOrganisationController> _logger = new();

    [TestInitialize]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _organisationServiceMock = new Mock<IOrganisationService>();
        _messagingServiceMock = new Mock<IMessagingService>();

        var messagingConfigOptions = Options.Create(new MessagingConfig
        {
            ReExAccountCreationUrl = "https://localhost:50012/re-ex-accountCreation"
        });

        _sut = new ReprocessorExporterOrganisationController(_organisationServiceMock.Object, _messagingServiceMock.Object, messagingConfigOptions, _logger)
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
    [DataRow("790108e5-08a2-426a-8c3c-45336efd0a5b", "790108e5-08a2-426a-8c3c-45336efd0a5b")]
    public async Task CreateAccount_HappyPath_ReturnsOk(string organisationId, string expectedResult)
    {
        // Arrange
        var reExOrgModel = _fixture.Create<ReExOrganisationModel>();
        reExOrgModel.Company.Nation = Nation.England;
        reExOrgModel.Company.OrganisationType = OrganisationType.CompaniesHouseCompany;
        reExOrgModel.InvitedApprovedPersons[0].Email = "testc@test.com";
        _fixture.Inject(reExOrgModel);

        var response = new ReExAddOrganisationResponse
        {
            UserFirstName = "Jon",
            UserLastName = "Doe",
            OrganisationId = Guid.Parse(organisationId),
            ReferenceNumber = "12555209",
            UserServiceRoles = [],
            InvitedApprovedUsers = 
            [
                new InvitedApprovedUserResponse
                {
                    Email = "testc@test.com", InviteToken = "xyz122334==" , ServiceRole = new ServiceRoleResponse()
                }
            ]
        };

        _organisationServiceMock.Setup(x => x.CreateReExOrganisationAsync(It.IsAny<ReprocessorExporterAddOrganisation>(), It.IsAny<string>()))
            .Returns(Task.FromResult(response));

        var notificationList = new List<(string email, string responseId)>()
        {
            ("test01@test.com", "123456")
        };

        _messagingServiceMock.Setup(x => x.SendReExInvitationToAcceptServiceRole(It.IsAny<ReExNotificationModel>()))
            .Returns(notificationList);
        _messagingServiceMock.Setup(x => x.SendReExInvitationConfirmationToInviter(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<(string email, string notificationResponseId)>>()))
            .Returns("9879");

        // Act
        var result = await _sut!.CreateReExAccount(reExOrgModel, ServiceKey);

        // Assert
        result.Should().NotBeNull();
        var okResult = (Microsoft.AspNetCore.Mvc.OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ToString().Should().Be(expectedResult);

        _organisationServiceMock.Verify(x => x.CreateReExOrganisationAsync(It.IsAny<ReprocessorExporterAddOrganisation>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAccount_Throws_Probelm_When_Response_IsNull()
    {
        // Arrange
        var reExOrgModel = _fixture.Create<ReExOrganisationModel>();
        reExOrgModel.Company.Nation = Nation.England;
        reExOrgModel.Company.OrganisationType = OrganisationType.CompaniesHouseCompany;
        reExOrgModel.Company.OrganisationId = null;
        _fixture.Inject(reExOrgModel);

        ReExAddOrganisationResponse? response = null;

        _organisationServiceMock.Setup(x => x.CreateReExOrganisationAsync(It.IsAny<ReprocessorExporterAddOrganisation>(), It.IsAny<string>()))
            .Returns(Task.FromResult(response));

        // Act
        var result = await _sut!.CreateReExAccount(reExOrgModel, ServiceKey);

        // Assert
        result.Should().NotBeNull();
        var objResult = result as Microsoft.AspNetCore.Mvc.ObjectResult;
        var value = objResult.Value as Microsoft.AspNetCore.Mvc.ProblemDetails;
        value.Detail.Should().Be("Response can not be null");
        value.Status.Should().Be(204);

        _organisationServiceMock.Verify(x => x.CreateReExOrganisationAsync(It.IsAny<ReprocessorExporterAddOrganisation>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAccount_Throws_Probelm_When_OrganisationId_IsEmptyGuid()
    {
        // Arrange
        var reExOrgModel = _fixture.Create<ReExOrganisationModel>();
        reExOrgModel.Company.Nation = Nation.England;
        reExOrgModel.Company.OrganisationType = OrganisationType.CompaniesHouseCompany;
        reExOrgModel.Company.OrganisationId = null;
        _fixture.Inject(reExOrgModel);

        var response = new ReExAddOrganisationResponse
        {
            UserFirstName = "Jon",
            UserLastName = "Doe",
            OrganisationId = Guid.Empty,
            ReferenceNumber = "12555209",
            UserServiceRoles = [],
            InvitedApprovedUsers = [new InvitedApprovedUserResponse { Email = "testc@test.com", InviteToken = "xyz122334==", ServiceRole = new ServiceRoleResponse() }]
        };

        _organisationServiceMock.Setup(x => x.CreateReExOrganisationAsync(It.IsAny<ReprocessorExporterAddOrganisation>(), It.IsAny<string>()))
            .Returns(Task.FromResult(response));

        // Act
        var result = await _sut!.CreateReExAccount(reExOrgModel, ServiceKey);

        // Assert
        result.Should().NotBeNull();
        var objResult = result as Microsoft.AspNetCore.Mvc.ObjectResult;
        var value = objResult.Value as Microsoft.AspNetCore.Mvc.ProblemDetails;
        value.Detail.Should().Be("Organisation id can not be empty");
        value.Status.Should().Be(204);

        _organisationServiceMock.Verify(x => x.CreateReExOrganisationAsync(It.IsAny<ReprocessorExporterAddOrganisation>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAccount_Throws_Exception()
    {
        // Arrange
        var apiResponse = new ProblemDetails
        {
            Detail = "detail",
            Type = "type"
        };

        var reExOrgModel = _fixture.Create<ReExOrganisationModel>();
        reExOrgModel.Company.Nation = Nation.England;
        reExOrgModel.Company.OrganisationType = OrganisationType.CompaniesHouseCompany;
        _fixture.Inject(reExOrgModel);

        _organisationServiceMock.Setup(x => x.CreateReExOrganisationAsync(It.IsAny<ReprocessorExporterAddOrganisation>(), It.IsAny<string>()))
            .ThrowsAsync(new ProblemResponseException(apiResponse, HttpStatusCode.Conflict));

        // Act
        Func<Task> act = () => _sut.CreateReExAccount(reExOrgModel, ServiceKey);

        // Assert
        var exception = await act.Should().ThrowAsync<ProblemResponseException>();
        exception.Which.Should().NotBeNull();
        exception.Which.ProblemDetails.Detail.Should().Be(apiResponse.Detail);
        exception.Which.ProblemDetails.Type.Should().Be(apiResponse.Type);

        _organisationServiceMock.Verify(x => x.CreateReExOrganisationAsync(It.IsAny<ReprocessorExporterAddOrganisation>(), It.IsAny<string>()), Times.Once);
    }
}
