using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Services.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class OrganisationServiceTests
{
    private const string GetOrganisationUsersListEndpoint = "api/organisations/users";
    private const string GetNationIdByOrganisationIdEndpoint = "api/regulator-organisation/organisation-nation";
    private const string GetOrganisationIdFromNationEndpoint = "api/regulator-organisation?nation=";
    private const string BaseAddress = "http://localhost";
    private const string GetOrganisationByExternalIdEndpoint = "api/organisations/organisation-by-externalId";
    private const string OrganisationNameUri = "api/organisations/organisation-by-invite-token";
    private const string OrganisationCreateAddSubsidiaryUri = "api/organisations/create-and-add-subsidiary";
    private const string OrganisationAddSubsidiaryUri = "api/organisations/add-subsidiary";
    private const string OrganisationGetRelationshipUri = "api/organisations";

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
            {"RegulatorOrganisationEndpoints:GetNationIdFromOrganisationId", GetNationIdByOrganisationIdEndpoint},
            {"RegulatorOrganisationEndpoints:GetOrganisationIdFromNation", GetOrganisationIdFromNationEndpoint}
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
    
    [TestMethod]
    public async Task GetOrganisationNameByInviteToken_ShouldReturnPersonResponse()
    {
        //Arrange
        var token = "inviteToken";
        var expectedUrl = $"{BaseAddress}/{OrganisationNameUri}?token={token}";
        var apiResponse = _fixture.Create<ApprovedPersonOrganisationModel>();

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
        var result = await sut.GetOrganisationNameByInviteToken(token);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeOfType<ApprovedPersonOrganisationModel>();
    }
    
    [TestMethod]
    public async Task GetPersonByInviteToken_ShouldReturnNull()
    {
        //Arrange
        var token = "inviteToken";
        var expectedUrl = $"{BaseAddress}/{OrganisationNameUri}?token={token}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            }).Verifiable();
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        var result = await sut.GetOrganisationNameByInviteToken(token);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeNull();
    }

    [TestMethod]
    [DataRow(1,"England")]
    [DataRow(2, "Northern Ireland")]
    [DataRow(3, "Scotland")]
    [DataRow(4, "Wales")]
    public async Task Get_GetRegulatorOrganisationByNationId_ShouldReturnSuccessfulResponse(int nationId, string nationName)
    {
        // Arrange
        var apiResponse = _fixture.Create<CheckRegulatorOrganisationExistResponseModel>();


        var expectedUrl =
            $"{BaseAddress}/{GetOrganisationIdFromNationEndpoint}{nationName}";

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
        var response = await sut.GetRegulatorOrganisationByNationId(nationId);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task Get_GetRegulatorOrganisationByNationId_ThrowExceptionWhenNoResponseReturned()
    {
        // Arrange
        var nationId = 1;
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetRegulatorOrganisationByNationId(nationId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task Get_GetRegulatorOrganisationByNationId_ShouldReturnEmptyResponse()
    {
        // Arrange
        int nationId = 1;
        string nationName = "England";

        var expectedUrl =
            $"{BaseAddress}/{GetOrganisationIdFromNationEndpoint}{nationName}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = null
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = async () => await sut.GetRegulatorOrganisationByNationId(nationId);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [TestMethod]
    public async Task Get_GetRegulatorOrganisationByNationId_ShouldReturnUnsuccessfulResponse()
    {
        // Arrange
        var apiResponse = _fixture.Create<ProblemDetails>();
        int nationId = 1;
        string nationName = "England";

        var expectedUrl =
            $"{BaseAddress}/{GetOrganisationIdFromNationEndpoint}{nationName}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = async () => await sut.GetRegulatorOrganisationByNationId(nationId);

        // Assert
        await act.Should().ThrowAsync<ProblemResponseException>();
    }

    [TestMethod]
    public async Task CreateAndAddSubsidiaryAsync_ShouldReturnOrganisationResponse()
    {
        // Arrange
        var apiResponse = _fixture.Create<string>();

        var expectedUrl =
            $"{BaseAddress}/{OrganisationCreateAddSubsidiaryUri}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        await sut.CreateAndAddSubsidiaryAsync(It.IsAny<LinkOrganisationModel>());

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task AddSubsidiaryAsync_ShouldReturnOrganisationResponse()
    {
        // Arrange
        var apiResponse = _fixture.Create<string>();

        var expectedUrl =
            $"{BaseAddress}/{OrganisationAddSubsidiaryUri}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        await sut.AddSubsidiaryAsync(It.IsAny<SubsidiaryAddModel>());

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationId_Returns_OrganisationRelationshipModel_OnSuccess()
    {
        // Arrange
        var organisationId = Guid.NewGuid();

        var expectedModel = _fixture.Create<OrganisationRelationshipModel>();

        var expectedUrl =
            $"{BaseAddress}/{OrganisationGetRelationshipUri}/{organisationId}/organisationRelationships";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedModel))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        var result = await sut.GetOrganisationRelationshipsByOrganisationId(organisationId);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeEquivalentTo(expectedModel);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationId_LogsError_AndThrowsException_OnFailure()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetOrganisationRelationshipsByOrganisationId(_organisationId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}