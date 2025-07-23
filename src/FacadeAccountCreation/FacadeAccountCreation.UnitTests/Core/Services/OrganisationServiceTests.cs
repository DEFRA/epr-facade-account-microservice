using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;
using FacadeAccountCreation.Core.Models.Subsidiary;
using FacadeAccountCreation.Core.Services.Organisation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class OrganisationServiceTests
{
    private const string GetOrganisationUsersListEndpoint = "api/organisations/users";
    private const string GetOrganisationAllUsersListEndpoint = "api/organisations/all-users";
    private const string GetNationIdByOrganisationIdEndpoint = "api/regulator-organisation/organisation-nation";
    private const string GetOrganisationIdFromNationEndpoint = "api/regulator-organisation?nation=";
    private const string UpdateOrganisationEndPoint = "api/organisations/organisation/";
    private const string BaseAddress = "http://localhost";
    private const string OrganisationNameUri = "api/organisations/organisation-by-invite-token";
    private const string OrganisationCreateAddSubsidiaryUri = "api/organisations/create-and-add-subsidiary";
    private const string OrganisationAddSubsidiaryUri = "api/organisations/add-subsidiary";
    private const string OrganisationTerminateSubsidiaryUri = "api/organisations/terminate-subsidiary";
    private const string OrganisationGetRelationshipUri = "api/organisations";
    private const string OrganisationByReferenceNumberUrl = "api/organisations/organisation-by-reference-number";
    private const string OrganisationNationUrl = "api/organisations/nation-code";
    private const string OrganisationByCompanyHouseNumberUrl = "api/organisations/organisation-by-companies-house-number";
    private const string ServiceKey = "re-ex";

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
            {"ComplianceSchemeEndpoints:GetAllOrganisationUsers", GetOrganisationAllUsersListEndpoint},
            {"RegulatorOrganisationEndpoints:GetNationIdFromOrganisationId", GetNationIdByOrganisationIdEndpoint},
            {"RegulatorOrganisationEndpoints:GetOrganisationIdFromNation", GetOrganisationIdFromNationEndpoint},
            {"OrganisationEndpoints:UpdateOrganisation", UpdateOrganisationEndPoint }
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
    public async Task GetOrganisationAllUsersList_should_return_successful_response()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl =
            $"{BaseAddress}/{GetOrganisationAllUsersListEndpoint}?userId={_userOid}&organisationId={_organisationId}&serviceRoleId={_serviceRoleId}";

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
        var response = await sut.GetOrganisationAllUsersList(_userOid, _organisationId, _serviceRoleId);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task GetOrganisationAllUsersList_should_throw_exception_when_no_response_returned()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetOrganisationAllUsersList(_userOid, _organisationId, _serviceRoleId);

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
    [DataRow(1, "England")]
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
        var nationId = 1;
        var nationName = "England";

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
        var nationId = 1;
        var nationName = "England";

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
    public async Task TerminateSubsidiaryAsync_ShouldReturnSuccess()
    {
        // Arrange
        const string expectedUrl = $"{BaseAddress}/{OrganisationTerminateSubsidiaryUri}";
        var apiResponse = StatusCodes.Status200OK.ToString();

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

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        await sut.TerminateSubsidiaryAsync(It.IsAny<SubsidiaryTerminateModel>());

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task TerminateSubsidiaryAsync_ShouldReturnUnsuccessfulResponse()
    {
        // Arrange
        const string expectedUrl = $"{BaseAddress}/{OrganisationTerminateSubsidiaryUri}";
        var apiResponse = _fixture.Create<ProblemDetails>();


        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        await sut.TerminateSubsidiaryAsync(It.IsAny<SubsidiaryTerminateModel>());

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task TerminateSubsidiaryAsync_WhenApiReturnsNoContent_ShouldReturnNull()
    {
        // Arrange
        const string expectedUrl = $"{BaseAddress}/{OrganisationTerminateSubsidiaryUri}";

        var organisationId = Guid.NewGuid();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = null
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        await sut.TerminateSubsidiaryAsync(It.IsAny<SubsidiaryTerminateModel>());

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    //[TestMethod]
    //public async Task GetPagedOrganisationRelationships_WithoutSearchParameter_Returns_OrganisationRelationshipModel_OnSuccess()
    //{
    //    // Arrange
    //    var page = 1;
    //    var showPerPage = 20;

    //    var expectedModel = _fixture.Create<PagedOrganisationRelationshipsModel>();

    //    var expectedUrl =
    //        $"{BaseAddress}/{OrganisationGetRelationshipUri}/organisationRelationships?page={page}&showPerPage={showPerPage}";

    //    _httpMessageHandlerMock.Protected()
    //        .Setup<Task<HttpResponseMessage>>("SendAsync",
    //            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
    //            ItExpr.IsAny<CancellationToken>())
    //        .ReturnsAsync(new HttpResponseMessage
    //        {
    //            StatusCode = HttpStatusCode.OK,
    //            Content = new StringContent(JsonSerializer.Serialize(expectedModel))
    //        }).Verifiable();

    //    var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
    //    httpClient.BaseAddress = new Uri(BaseAddress);

    //    var sut = new OrganisationService(httpClient, _logger, _configuration);

    //    // Act
    //    var result = await sut.GetPagedOrganisationRelationships(page, showPerPage);

    //    // Assert
    //    Assert.IsNotNull(result);
    //    result.Should().BeEquivalentTo(expectedModel);
    //}

    //[TestMethod]
    //public async Task GetPagedOrganisationRelationships_WithSearchParameter_Returns_OrganisationRelationshipModel_OnSuccess()
    //{
    //    // Arrange
    //    var page = 1;
    //    var showPerPage = 20;
    //    var search = "test";

    //    var expectedModel = _fixture.Create<PagedOrganisationRelationshipsModel>();

    //    var expectedUrl =
    //        $"{BaseAddress}/{OrganisationGetRelationshipUri}/organisationRelationships?page={page}&showPerPage={showPerPage}&search={search}";

    //    _httpMessageHandlerMock.Protected()
    //        .Setup<Task<HttpResponseMessage>>("SendAsync",
    //            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
    //            ItExpr.IsAny<CancellationToken>())
    //        .ReturnsAsync(new HttpResponseMessage
    //        {
    //            StatusCode = HttpStatusCode.OK,
    //            Content = new StringContent(JsonSerializer.Serialize(expectedModel))
    //        }).Verifiable();

    //    var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
    //    httpClient.BaseAddress = new Uri(BaseAddress);

    //    var sut = new OrganisationService(httpClient, _logger, _configuration);

    //    // Act
    //    var result = await sut.GetPagedOrganisationRelationships(page, showPerPage, search);

    //    // Assert
    //    Assert.IsNotNull(result);
    //    result.Should().BeEquivalentTo(expectedModel);
    //}

    [TestMethod]
    public async Task GetPagedOrganisationRelationships_ThrowsException_OnFailure()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetPagedOrganisationRelationships(1, 20);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task GetUnpagedOrganisationRelationships_Returns_OrganisationRelationshipModel_OnSuccess()
    {
        // Arrange

        var expectedModel = _fixture.Create<List<RelationshipResponseModel>>();

        var expectedUrl =
            $"{BaseAddress}/{OrganisationGetRelationshipUri}/organisationRelationshipsWithoutPaging";

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
        var result = await sut.GetUnpagedOrganisationRelationships();

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeEquivalentTo(expectedModel);
    }


    [TestMethod]
    public async Task GetUnpagedOrganisationRelationships_ThrowsException_OnFailure()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.GetUnpagedOrganisationRelationships();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
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
    public async Task GetOrganisationRelationshipsByOrganisationId_ThrowsException_OnFailure()
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

    [TestMethod]

    public async Task GetOrganisationRelationshipsByOrganisationId_LogsError_AndThrowsException_OnFailure()
    {
        // Arrange

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        httpClient.BaseAddress = new Uri(BaseAddress);

        var loggerMock = new Mock<ILogger<OrganisationService>>();

        var sut = new OrganisationService(httpClient, loggerMock.Object, _configuration);

        // Act

        Func<Task> act = async () => await sut.GetOrganisationRelationshipsByOrganisationId(_organisationId);

        // Assert

        await act.Should().ThrowAsync<InvalidOperationException>();

        Assert.IsNotNull(act);

        loggerMock.VerifyLog(logger => logger.LogError(It.IsAny<Exception>(), "Failed to get Organisation Relationships for Organisation Id: '{organisationId}'", _organisationId));
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationId_WhenApiReturnsNoContent_ShouldReturnNull()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        var result = await sut.GetOrganisationRelationshipsByOrganisationId(organisationId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task ExportOrganisationSubsidiaries_Returns_List_ExportOrganisationSubsidiariesResponseModel_OnSuccess()
    {
        // Arrange
        var organisationId = Guid.NewGuid();

        var expectedModel = _fixture.Create<List<ExportOrganisationSubsidiariesResponseModel>>();

        var expectedUrl =
            $"{BaseAddress}/{OrganisationGetRelationshipUri}/{organisationId}/export-subsidiaries";

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
        var result = await sut.ExportOrganisationSubsidiaries(organisationId);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeEquivalentTo(expectedModel);
    }

    [TestMethod]
    public async Task ExportOrganisationSubsidiaries_ThrowsException_OnFailure()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.ExportOrganisationSubsidiaries(_organisationId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task ExportOrganisationSubsidiaries_LogsError_AndThrowsException_OnFailure()
    {
        // Arrange

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        httpClient.BaseAddress = new Uri(BaseAddress);

        var loggerMock = new Mock<ILogger<OrganisationService>>();

        var sut = new OrganisationService(httpClient, loggerMock.Object, _configuration);

        // Act

        Func<Task> act = async () => await sut.ExportOrganisationSubsidiaries(_organisationId);

        // Assert

        await act.Should().ThrowAsync<InvalidOperationException>();

        Assert.IsNotNull(act);

        loggerMock.VerifyLog(logger => logger.LogError(It.IsAny<Exception>(), "Failed to Export Organisation Relationships for Organisation Id: '{OrganisationId}'", _organisationId));
    }
    [TestMethod]
    public async Task UpdateOrganisationDetails_ShouldReturnSuccesfulResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var organisation = new OrganisationUpdateDto();

        var expectedUrl =
            $"{BaseAddress}/{UpdateOrganisationEndPoint}/{organisationId}";

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        var sut = new OrganisationService(
            httpClient,
            _logger,
            _configuration);

        // Act
        await sut.UpdateOrganisationDetails(
            userId,
            organisationId,
            organisation);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [TestMethod]
    public async Task GetOrganisationByReferenceNumber_ShouldReturnSuccess()
    {
        // Arrange
        const string referenceNumber = "123456";
        const string expectedUrl = $"{BaseAddress}/{OrganisationByReferenceNumberUrl}?referenceNumber={referenceNumber}";
        var apiResponse = new OrganisationDto { RegistrationNumber = referenceNumber };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Get &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
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

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        await sut.GetOrganisationByReferenceNumber(referenceNumber);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetOrganisationByReferenceNumber_WhenApiReturnsNoContent_ReturnsNullResponse()
    {
        // Arrange
        const string referenceNumber = "123456";
        const string expectedUrl = $"{BaseAddress}/{OrganisationByReferenceNumberUrl}?referenceNumber={referenceNumber}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Get &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        var result = await sut.GetOrganisationByReferenceNumber(referenceNumber);

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
    public async Task GetOrganisationByCompanyNumber_WhenApiReturnsNoContent_ReturnsNullResponse()
    {
        // Arrange
        const string companiesHouseNumber = "12345678";
        const string expectedUrl = $"{BaseAddress}/{OrganisationByCompanyHouseNumberUrl}?companiesHouseNumber={companiesHouseNumber}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Get &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        var result = await sut.GetOrganisationByCompanyHouseNumber(companiesHouseNumber);

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
    public async Task GetOrganisationByReferenceNumber_WhenApiReturnsNonSuccess_ShouldReturnUnsuccessfulResponse()
    {
        // Arrange
        const string referenceNumber = "123456";
        const string expectedUrl = $"{BaseAddress}/{OrganisationByReferenceNumberUrl}?referenceNumber={referenceNumber}";
        var apiResponse = _fixture.Create<ProblemDetails>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Get &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        var act = async () => await sut.GetOrganisationByReferenceNumber(referenceNumber);

        // Assert
        await act.Should().ThrowAsync<ProblemResponseException>();
    }

    [TestMethod]
    public async Task GetOrganisationNationByExternalIdAsync_When_APIReturns_Return_200_Successful_Response()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expectedUrl = $"{BaseAddress}/{OrganisationNationUrl}?organisationId={organisationId}";

        var apiResponse = "GB-ENG";

        _httpMessageHandlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync",
               ItExpr.Is<HttpRequestMessage>(
                   req => req.RequestUri != null &&
                          req.RequestUri.ToString() == expectedUrl),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(JsonSerializer.Serialize(apiResponse))
           });

        _httpMessageHandlerMock.Verify();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        var result = await sut.GetOrganisationNationCodeByExternalIdAsync(organisationId);


        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetOrganisationNationByExternalIdAsync_When_APIReturns_404_NoFound_ReturnsNullResponse()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expectedUrl = $"{BaseAddress}/{OrganisationNationUrl}?organisationId={organisationId}";

        _httpMessageHandlerMock.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                       ItExpr.Is<HttpRequestMessage>(
                           req => req.Method == HttpMethod.Get &&
                                  req.RequestUri != null &&
                                  req.RequestUri.ToString() == expectedUrl),
                       ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.NotFound,
                       Content = null
                   }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        var result = await sut.GetOrganisationNationCodeByExternalIdAsync(organisationId);


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
    public async Task GetOrganisationNationByExternalIdAsync_When_APIReturns_500_InternalServerError_ReturnsExeption()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expectedUrl = $"{BaseAddress}/{OrganisationNationUrl}?organisationId={organisationId}";

        _httpMessageHandlerMock.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                       ItExpr.Is<HttpRequestMessage>(
                           req => req.Method == HttpMethod.Get &&
                                  req.RequestUri != null &&
                                  req.RequestUri.ToString() == expectedUrl),
                       ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.InternalServerError,
                       Content = null
                   }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        //Act
        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => sut.GetOrganisationNationCodeByExternalIdAsync(organisationId));

        // Assert
        ex.Should().NotBeNull();
        ex.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetOrganisationNationByExternalIdAsync_When_API_ThrowsException()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var loggerMock = new Mock<ILogger<OrganisationService>>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, loggerMock.Object, _configuration);

        // Act
        Func<Task> act = async () => await sut.GetOrganisationNationCodeByExternalIdAsync(organisationId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        Assert.IsNotNull(act);

        loggerMock.VerifyLog(logger => logger.LogError(It.IsAny<Exception>(), "Failed to get Organisation nation for Organisation Id: '{OrganisationExternalId}'", organisationId));
    }

    [TestMethod]
    [DataRow("790108e5-08a2-426a-8c3c-45336efd0a5b", "790108e5-08a2-426a-8c3c-45336efd0a5b")]
    public async Task CreateReExOrganisationAsync_Returns_OrganisationId_AsExpected(string organisationId, string expectedResult)
    {
        // Arrange    
        var reExOrgModel = _fixture.Create<ReprocessorExporterAddOrganisation>();
        reExOrgModel.Organisation.OrganisationId = organisationId;
        _fixture.Inject(reExOrgModel);

        var apiResponse = _fixture.Create<ReExAddOrganisationResponse>();
        apiResponse.OrganisationId = Guid.Parse(organisationId);

        var httpRequestMessage = _fixture.Build<HttpRequestMessage>().With(q => q.Content, JsonContent.Create(reExOrgModel)).Create();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req =>
                  req.Content.ReadAsStringAsync().Result == httpRequestMessage.Content.ReadAsStringAsync().Result), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        var result = await sut.CreateReExOrganisationAsync(reExOrgModel, ServiceKey);

        // Assert
        result.OrganisationId.ToString().Should().BeEquivalentTo(expectedResult);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req => 
            req.Content.ReadAsStringAsync().Result == httpRequestMessage.Content.ReadAsStringAsync().Result), 
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task CreateReExOrganisationAsync_Returns_No_Content()
    {
        // Arrange     
        var reExOrgModel = _fixture.Create<ReprocessorExporterAddOrganisation>();
        reExOrgModel.Organisation.OrganisationId = "790108e5-08a2-426a-8c3c-45336efd0a5b";
        _fixture.Inject(reExOrgModel);

        ReExAddOrganisationResponse? apiResponse = null;

        var httpRequestMessage = _fixture.Build<HttpRequestMessage>().With(q => q.Content, JsonContent.Create(reExOrgModel)).Create();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req =>
                  req.Content.ReadAsStringAsync().Result == httpRequestMessage.Content.ReadAsStringAsync().Result), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        var result = await sut.CreateReExOrganisationAsync(reExOrgModel, ServiceKey);

        // Assert
        result.Should().BeNull();

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req =>
            req.Content.ReadAsStringAsync().Result == httpRequestMessage.Content.ReadAsStringAsync().Result),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task CreateReExOrganisationAsync_ThrowsException_OnFailure()
    {
        // Arrange
        var apiResponse = new ProblemDetails
        {
            Detail = "detail",
            Type = "type"
        };

        var reExOrgModel = _fixture.Create<ReprocessorExporterAddOrganisation>();        
        var httpRequestMessage = _fixture.Build<HttpRequestMessage>().With(q => q.Content, JsonContent.Create(reExOrgModel)).Create();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req =>
                  req.Content.ReadAsStringAsync().Result == httpRequestMessage.Content.ReadAsStringAsync().Result), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new OrganisationService(httpClient, _logger, _configuration);

        // Act
        Func<Task> act = () => sut.CreateReExOrganisationAsync(reExOrgModel, ServiceKey);

        // Assert
        var exception = await act.Should().ThrowAsync<ProblemResponseException>();

        exception.Which.Should().NotBeNull();
        exception.Which.ProblemDetails.Detail.Should().Be(apiResponse.Detail);
        exception.Which.ProblemDetails.Type.Should().Be(apiResponse.Type);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
            req.Content.ReadAsStringAsync().Result == httpRequestMessage.Content.ReadAsStringAsync().Result),
            ItExpr.IsAny<CancellationToken>());
    }

	[TestMethod]
	public async Task GetOrganisationTeamMembers_ReturnsTeamMembers_WhenSuccessful()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var organisationId = Guid.NewGuid();
		var serviceRoleId = 1;

		var expectedMembers = new List<OrganisationTeamMemberModel>
		{
			new() { FirstName = "John", LastName = "Doe", Email = "john@example.com" }
		};

		var json = JsonSerializer.Serialize(expectedMembers);

		_httpMessageHandlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			});

		var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
		{
			BaseAddress = new Uri(BaseAddress)
		};

		var sut = new OrganisationService(httpClient, _logger, _configuration);

		// Act
		var result = await sut.GetOrganisationTeamMembers(userId, organisationId, serviceRoleId);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(1);
		result[0].FirstName.Should().Be("John");
		result[0].Email.Should().Be("john@example.com");
	}

	[TestMethod]
	public async Task GetOrganisationTeamMembers_ThrowsProblemResponseException_WhenResponseHasProblemDetails()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var organisationId = Guid.NewGuid();
		var serviceRoleId = 1;

		var problem = new ProblemDetails
		{
			Title = "Unauthorized",
			Status = 401,
			Detail = "You are not authorized"
		};

		var json = JsonSerializer.Serialize(problem);

		_httpMessageHandlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.Unauthorized,
				Content = new StringContent(json)
			});

		var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
		{
			BaseAddress = new Uri(BaseAddress)
		};

		var sut = new OrganisationService(httpClient, _logger, _configuration);

		// Act
		Func<Task> action = async () =>
			await sut.GetOrganisationTeamMembers(userId, organisationId, serviceRoleId);

		// Assert
		await action.Should().ThrowAsync<ProblemResponseException>()
			.Where(ex => ex.StatusCode == HttpStatusCode.Unauthorized &&
						 ex.ProblemDetails.Title == "Unauthorized");
	}

	[TestMethod]
	public async Task GetOrganisationTeamMembers_ThrowsException_WhenHttpCallFailsWithoutProblemDetails()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var organisationId = Guid.NewGuid();
		var serviceRoleId = 1;

		_httpMessageHandlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Content = new StringContent("Internal Error") // not JSON
			});

		var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
		{
			BaseAddress = new Uri(BaseAddress)
		};

		var sut = new OrganisationService(httpClient, _logger, _configuration);

		// Act
		Func<Task> act = async () => await sut.GetOrganisationTeamMembers(userId, organisationId, serviceRoleId);

		// Assert
		await act.Should().ThrowAsync<JsonException>();
	}
}