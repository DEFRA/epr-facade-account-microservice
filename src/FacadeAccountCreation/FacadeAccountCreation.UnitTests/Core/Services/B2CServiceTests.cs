using FacadeAccountCreation.Core.Models.B2c;
using FacadeAccountCreation.Core.Services.B2C;
using Microsoft.Extensions.Configuration;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class B2CServiceTests
{
    private const string GetUserOrganisationIdsEndpoint = "GetUserOrganisationIds";
    private const string BaseAddress = "http://localhost";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly NullLogger<B2CService> _logger = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly IConfiguration _configuration = GetConfig();
    private readonly Guid _userOid = Guid.NewGuid();

    private static IConfiguration GetConfig()
    {
        var config = new Dictionary<string, string?>
        {
            {"ApiConfig:AccountServiceBaseUrl", BaseAddress},
            {"B2cEndpoints:GetUserOrganisationIds", GetUserOrganisationIdsEndpoint}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        return configuration;
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_Should_Return_Successful_Response()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest { ObjectId = _userOid };

        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl = $"{BaseAddress}/{GetUserOrganisationIdsEndpoint}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new B2CService(httpClient, _logger, _configuration);

        // Act
        var response = await sut.GetUserOrganisationIds(request);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_Throws_Exception_When_No_Response_Returned()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest { ObjectId = _userOid };
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new B2CService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetUserOrganisationIds(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
    
    [TestMethod]
    public async Task GetUserOrganisationIds_ShouldThrowException_WhenEndpointIsMissing()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest { ObjectId = _userOid };
        var badConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"ApiConfig:AccountServiceBaseUrl", BaseAddress},
            // No B2cEndpoints:GetUserOrganisationIds
        }).Build();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new B2CService(httpClient, _logger, badConfig);

        // Act
        Func<Task> act = async () => await sut.GetUserOrganisationIds(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The 'GetUserOrganisationIds' endpoint is not configured.");
    }
}
