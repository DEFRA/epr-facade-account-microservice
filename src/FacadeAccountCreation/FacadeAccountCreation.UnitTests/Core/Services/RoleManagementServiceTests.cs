using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Services.RoleManagement;
using RichardSzalay.MockHttp;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class RoleManagementServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _enrolmentId = Guid.NewGuid();
    private readonly string _serviceKey = "Packaging";
    private const string _baseAddress = "https://localhost";

    private MockHttpMessageHandler _mockHttpHandler;
    private NullLogger<RoleManagementService> _logger;
    private RoleManagementService _rms;
    private HttpClient _httpClient;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpHandler = new MockHttpMessageHandler();
        _httpClient = _mockHttpHandler.ToHttpClient();
        _httpClient.BaseAddress = new Uri(_baseAddress);
        _logger = new();

        var messagingConfig = new ConnectionsEndpointsConfig();

        var optionsMock = new Mock<IOptions<ConnectionsEndpointsConfig>>();
        optionsMock.Setup(ap => ap.Value).Returns(messagingConfig);

        _rms = new RoleManagementService(_httpClient, _logger, optionsMock.Object);
    }

    [TestMethod]
    public async Task AcceptNominationForApprovedPerson_SuccessfulResponse()
    {
        //  Arrange
        _mockHttpHandler.When($"/api/enrolments/{_enrolmentId}/approved-person-acceptance?serviceKey={_serviceKey}")
            .Respond(HttpStatusCode.OK);

        //  Act
        var result = await _rms.AcceptNominationForApprovedPerson(
            _enrolmentId, _userId, _organisationId, _serviceKey, GetAAPRequest());

        //  Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task AcceptNominationForApprovedPerson_InvalidServiceKey_ThrowsException()
    {
        _mockHttpHandler.When($"/api/enrolments/{_enrolmentId}/approved-person-acceptance?serviceKey=invalidServiceKey")
            .Respond(HttpStatusCode.Unauthorized);

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _rms.AcceptNominationForApprovedPerson(_enrolmentId, _userId, _organisationId, "invalidServiceKey", GetAAPRequest()));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    private static AcceptNominationApprovedPersonRequest GetAAPRequest()
    {
        return new AcceptNominationApprovedPersonRequest
        {
            ContactEmail = "test@test.com",
            DeclarationFullName = "First Last",
            DeclarationTimeStamp = DateTime.Now,
            JobTitle = "Worker",
            OrganisationName = "Org 1",
            OrganisationNumber = "1",
            PersonFirstName = "first",
            PersonLastName = "last",
            Telephone = "01274558822"
        };
    }
}