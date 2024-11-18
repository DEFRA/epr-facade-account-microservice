using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Models.DelegatedPerson;
using FacadeAccountCreation.Core.Services.RoleManagement;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class DelegatedPersonEnrolmentServiceTests
{
    private const string BaseAddress = "http://localhost";
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly NullLogger<RoleManagementService> _logger = new();
    private IOptions<ConnectionsEndpointsConfig> connectionsEndpointsConfig;
    
    [TestInitialize]
    public void Setup()
    {
        connectionsEndpointsConfig = Options.Create(new ConnectionsEndpointsConfig
        {
            Enrolments = "api/connections/{0}/roles?serviceKey={1}",
            Person = "api/connections/{0}/person?serviceKey={1}"
        });
    }

    [TestMethod]
    public async Task GetDelegatedPersonNominator_WhenRecordFound_ShouldReturnDelegatedPersonNominatorModelResponse()
    {
        var enrolmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var serviceKey = "Packaging";

        var apiResponse = _fixture.Create<DelegatedPersonNominatorModel>();
        
        var expectedUrl = $"{BaseAddress}/api/enrolments/{enrolmentId}/delegated-person-nominator?serviceKey={serviceKey}";

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.OK,
                 Content = new StringContent(JsonSerializer.Serialize(apiResponse))
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        var result = await sut.GetDelegatedPersonNominator(enrolmentId, userId, organisationId, serviceKey);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeOfType<DelegatedPersonNominatorModel>();
    }
    
    [TestMethod]
    public async Task GetDelegatedPersonNominator_WhenRecordNotFound_ShouldReturnNoContent()
    {
        var enrolmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var serviceKey = "Packaging";
        
        var expectedUrl = $"{BaseAddress}/api/enrolments/{enrolmentId}/delegated-person-nominator?serviceKey={serviceKey}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = null
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        var result = await sut.GetDelegatedPersonNominator(enrolmentId, userId, organisationId, serviceKey);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeNull();
    }
}
