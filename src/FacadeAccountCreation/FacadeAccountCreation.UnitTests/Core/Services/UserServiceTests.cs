using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.Core.Models.User;
using FacadeAccountCreation.Core.Services.User;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class UserServiceTests
{
    private const string GetUserOrganisationsEndpoint = "GetUserOrganisations";
    private const string BaseAddress = "http://localhost";
    private const string UpdateUserDetailsEndpoint = "UpdateUserDetails";

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
            {"UserDetailsEndpoints:UpdateUserDetails", UpdateUserDetailsEndpoint},
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

    [TestMethod]
    public async Task UpdatePersonalDetailsAsync_ShouldReturnSuccessfulResponse_WhenRequestIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var serviceKey = "test-service-key";

        var userDetailsUpdateModelRequest = _fixture.Create<UpdateUserDetailsRequest>();
        var expectedUri = $"{BaseAddress}/{UpdateUserDetailsEndpoint}?serviceKey={serviceKey}";

        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x =>
                    x.Method == HttpMethod.Put &&
                    x.RequestUri != null &&
                    x.RequestUri.ToString() == expectedUri &&
                    x.Headers.Contains("X-EPR-User") &&
                    x.Headers.Contains("X-EPR-Organisation") &&
                    x.Headers.GetValues("X-EPR-User").First() == userId.ToString() &&
                    x.Headers.GetValues("X-EPR-Organisation").First() == organisationId.ToString()),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new UserService(httpClient, _logger, _configuration);

        // Act
        var response = await sut.UpdatePersonalDetailsAsync(userId, organisationId, serviceKey, userDetailsUpdateModelRequest);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task UpdatePersonalDetailsAsync_ShouldThrowException_WhenRequestFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var serviceKey = "test-service-key";

        var userDetailsUpdateModelRequest = _fixture.Create<UpdateUserDetailsRequest>();
        var expectedUri = $"{BaseAddress}/{UpdateUserDetailsEndpoint}?serviceKey={serviceKey}";

        var apiResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x =>
                    x.Method == HttpMethod.Put &&
                    x.RequestUri != null &&
                    x.RequestUri.ToString() == expectedUri),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new UserService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = async () => await sut.UpdatePersonalDetailsAsync(userId, organisationId, serviceKey, userDetailsUpdateModelRequest);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    public async Task UpdatePersonalDetailsAsync_ShouldThrowException_WhenHttpClientThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var serviceKey = "test-service-key";

        var userDetailsUpdateModelRequest = _fixture.Create<UpdateUserDetailsRequest>();
        var expectedUri = $"{BaseAddress}/{UpdateUserDetailsEndpoint}?serviceKey={serviceKey}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x =>
                    x.Method == HttpMethod.Put &&
                    x.RequestUri != null &&
                    x.RequestUri.ToString() == expectedUri),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"))
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new UserService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = async () => await sut.UpdatePersonalDetailsAsync(userId, organisationId, serviceKey, userDetailsUpdateModelRequest);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Network error");
    }

}
