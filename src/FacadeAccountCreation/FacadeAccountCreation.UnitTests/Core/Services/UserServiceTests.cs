using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.Core.Services.User;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class UserServiceTests
{
    private const string GetUserOrganisationsEndpoint = "GetUserOrganisations";
    private const string BaseAddress = "http://localhost";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly NullLogger<UserService> _logger = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly IConfiguration _configuration = GetConfig();
    private readonly Guid _userOid = Guid.NewGuid();

    private static IConfiguration GetConfig()
    {
        var config = new Dictionary<string, string?>
        {
            {"ApiConfig:AccountServiceBaseUrl", BaseAddress},
            {"ComplianceSchemeEndpoints:GetUserOrganisations", GetUserOrganisationsEndpoint},
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        return configuration;
    }

    [TestMethod]
    public async Task Get_user_organisations_should_return_successful_response()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl = $"{BaseAddress}/{GetUserOrganisationsEndpoint}?userId={_userOid}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new UserService(httpClient, _logger, _configuration);

        // Act
        var response = await sut.GetUserOrganisations(_userOid);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task Fetch_user_organisations_throw_exception_when_no_response_returned()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new UserService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetUserOrganisations(_userOid);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }


}
