using System.Net;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Services.Organisation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class OrganisationServiceTests
{
    private const string GetOrganisationUsersListEndpoint = "api/organisations/users";
    private const string GetNationIdByOrganisationIdEndpoint = "api/regulator-organisation/organisation-nation";
    private const string BaseAddress = "http://localhost";
    private const string GetOrganisationByExternalIdEndpoint = "api/organisations/organisation-by-externalId";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly NullLogger<OrganisationService> _logger = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly IConfiguration _configuration = GetConfig();
    private readonly Guid _userOid = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _organisationExternalId = Guid.NewGuid();
    
    private readonly int _serviceRoleId = 1;
    
    private static IConfiguration GetConfig()
    {
        var config = new Dictionary<string, string?>
        {
            {"ApiConfig:AccountServiceBaseUrl", BaseAddress},
            {"ComplianceSchemeEndpoints:GetOrganisationUsers", GetOrganisationUsersListEndpoint},
            {"RegulatorOrganisationEndpoints:GetNationIdFromOrganisationId", GetNationIdByOrganisationIdEndpoint}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        return configuration;
    }

    
    [TestMethod]
    public async Task Get_organisations_users_should_return_successful_response()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl =
            $"{BaseAddress}/{GetOrganisationUsersListEndpoint}?userId={_userOid}&organisationId={_organisationId}&serviceRoleId={_serviceRoleId}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        var response = await sut.GetOrganisationUserList(_userOid, _organisationId, _serviceRoleId);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }
    
    [TestMethod]
    public async Task Get_organisations_users_throw_exception_when_no_response_returned()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetOrganisationUserList(_userOid, _organisationId, _serviceRoleId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
    
    [TestMethod]
    public async Task Get_Nation_Id_By_Organisation_Id_should_return_successful_response()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl =
            $"{BaseAddress}/{GetNationIdByOrganisationIdEndpoint}?organisationId={_organisationId}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        var response = await sut.GetNationIdByOrganisationId(_organisationId);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }
    
    [TestMethod]
    public async Task Get_Nation_Id_By_Organisation_Id_throw_exception_when_no_response_returned()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetNationIdByOrganisationId(_organisationId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
    
    [TestMethod]
    public async Task Get_organisationsByExternalId_users_should_return_successful_response()
    {
        var apiResponse = _fixture.Create<RemovedUserOrganisationModel>();

        var expectedUrl =
            $"{BaseAddress}/{GetOrganisationByExternalIdEndpoint}?externalId={_organisationExternalId}";
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);
    
        // Act
        var response = await sut.GetOrganisationByExternalId(_organisationExternalId);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }
    
    [TestMethod]
    public async Task Get_organisationsByExternalId_users_throw_exception_when_no_response_returned()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetOrganisationByExternalId(_organisationExternalId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}