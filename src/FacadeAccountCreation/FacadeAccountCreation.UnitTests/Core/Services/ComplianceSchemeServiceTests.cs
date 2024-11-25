using System.Net.Http.Json;
using System.Text;
using EPR.Common.Logging.Constants;
using EPR.Common.Logging.Models;
using EPR.Common.Logging.Services;
using FacadeAccountCreation.Core.Helpers;
using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.Subsidiary;
using FacadeAccountCreation.Core.Services;
using FacadeAccountCreation.Core.Services.ComplianceScheme;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class ComplianceSchemeServiceTests
{
    private const string RemoveEndpoint = "remove";
    private const string SelectEndpoint = "select";
    private const string UpdateEndpoint = "update";
    private const string GetAllComplianceSchemesEndPoint = "get";
    private const string GetComplianceSchemeForProducerEndPoint = "get-for-producer";
    private const string GetComplianceSchemesForOperatorEndPoint = "get-for-operator";
    private const string GetComplianceSchemeMemberDetails = "{0}/scheme-members/{1}";
    private const string GetComplianceSchemeMembersEndPoint = "{0}/schemes/{1}/scheme-members?pageSize={2}&page={3}&query={4}";
    private const string ComplianceSchemeSummaryPath = "api/compliance-schemes/{0}/summary";
    private const string GetAllReasonsForRemovalsEndPoint = "member-removal-reasons";
    private const string RemoveComplianceSchemeMemberEndPoint = "{0}/scheme-members/{1}/removed";
    private const string GetInfoForSelectedSchemeRemoval = "{0}/scheme-members/{1}/removal";
    private const string ExportComplianceSchemeSubsidiaries = "api/compliance-schemes/{0}/scheme-members/{1}/export-subsidiaries";
    private const string BaseAddress = "http://localhost";
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly NullLogger<ComplianceSchemeService> _logger = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly IConfiguration _configuration = GetConfig();
    private Mock<ILoggingService> _loggingServiceMock;
    private Mock<ICorrelationIdProvider> _correlationIdProviderMock;


    [TestInitialize]
    public void TestInitialize()
    {
        _loggingServiceMock = new Mock<ILoggingService>();
        _correlationIdProviderMock = new Mock<ICorrelationIdProvider>();
    }

    private static IConfiguration GetConfig()
    {
        var config = new Dictionary<string, string?>
        {
            { "ApiConfig:AccountServiceBaseUrl", BaseAddress },
            { "ComplianceSchemeEndpoints:Select", SelectEndpoint },
            { "ComplianceSchemeEndpoints:Update", UpdateEndpoint },
            { "ComplianceSchemeEndpoints:Remove", RemoveEndpoint },
            { "ComplianceSchemeEndpoints:Get", GetAllComplianceSchemesEndPoint },
            { "ComplianceSchemeEndpoints:GetComplianceSchemeForProducer", GetComplianceSchemeForProducerEndPoint },
            { "ComplianceSchemeEndpoints:GetComplianceSchemesForOperator", GetComplianceSchemesForOperatorEndPoint },
            { "ComplianceSchemeEndpoints:GetComplianceSchemeMemberDetails", GetComplianceSchemeMemberDetails },
            { "ComplianceSchemeEndpoints:GetComplianceSchemeMembers", GetComplianceSchemeMembersEndPoint },
            { "ComplianceSchemeEndpoints:ComplianceSchemeSummaryPath", ComplianceSchemeSummaryPath },
            { "ComplianceSchemeEndpoints:GetAllReasonsForRemovals", GetAllReasonsForRemovalsEndPoint },
            { "ComplianceSchemeEndpoints:RemoveComplianceSchemeMember", RemoveComplianceSchemeMemberEndPoint },
            { "ComplianceSchemeEndpoints:GetInfoForSelectedSchemeRemoval", GetInfoForSelectedSchemeRemoval },
            { "ComplianceSchemeEndpoints:ExportComplianceSchemeSubsidiaries", ExportComplianceSchemeSubsidiaries },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        return configuration;
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_RecordsFound_ReturnsSuccessfulResult()
    {
        var organisationId = Guid.NewGuid();
        var selectedSchemeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = string.Empty;
        var pageSize = 10;
        var page = 1;

        var items = _fixture.Build<List<ComplianceSchemeMemberDto>>().Create();
        var pagedResult = _fixture
            .Build<PaginatedResponse<ComplianceSchemeMemberDto>>()
            .With(x => x.PageSize, pageSize)
            .With(x => x.CurrentPage, page)
            .With(x => x.Items, items)
            .With(x => x.TotalItems, items.Count)
            .Create();

        var complianceSchemeMembershipResponse = _fixture.Build<ComplianceSchemeMembershipResponse>()
            .With(x => x.PagedResult, pagedResult)
            .Create();

        var resultJsonString = JsonSerializer.Serialize(complianceSchemeMembershipResponse);

        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .With(c => c.Content, new StringContent(resultJsonString, Encoding.UTF8, "application/json"))
            .Create();

        var endpoint = string.Format(GetComplianceSchemeMembersEndPoint, organisationId, selectedSchemeId, pageSize, page, query);
        var expectedUrl = $"{BaseAddress}/{endpoint}";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        var response = await sut.GetComplianceSchemeMembersAsync(userId, organisationId, selectedSchemeId, query, pageSize, page, false);

        response.Should().BeEquivalentTo(apiResponse);

        var responseContent = response.Content.ReadFromJsonAsync<ComplianceSchemeMembershipResponse>().Result;
        responseContent.Should().NotBeNull();
        responseContent.PagedResult.CurrentPage.Should().Be(pagedResult.CurrentPage);
        responseContent.PagedResult.PageSize.Should().Be(pagedResult.PageSize);
        responseContent.PagedResult.TotalItems.Should().Be(pagedResult.Items.Count);
    }

    [TestMethod]
    public async Task GetAllComplianceSchemes_ReturnsSuccessfulResult()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl = $"{BaseAddress}/{GetAllComplianceSchemesEndPoint}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var response = await sut.GetAllComplianceSchemesAsync();

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task GetAllComplianceSchemes_ThrowsException_OnInternalServerError()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.InternalServerError)
            .Create();

        var expectedUrl = $"{BaseAddress}/{GetAllComplianceSchemesEndPoint}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var response = await sut.GetAllComplianceSchemesAsync();

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task GetComplianceSchemeForProducer_ReturnsSuccessfulResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userOid = Guid.NewGuid();
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();
        
        var expectedUrl = $"{BaseAddress}/{GetComplianceSchemeForProducerEndPoint}?organisationId={organisationId}&userOid={userOid}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var response = await sut.GetComplianceSchemeForProducerAsync(organisationId, userOid);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task GetAllComplianceScheme_ShouldThrowException_WhenNoResponseIsReturned()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        Func<Task> act = () => sut.GetAllComplianceSchemesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task Remove_selected_scheme_request_should_return_successful_response()
    {
        // Arrange
        var req = _fixture.Create<RemoveComplianceSchemeModel>();

        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl = $"{BaseAddress}/{RemoveEndpoint}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var response = await sut.RemoveComplianceScheme(req);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task remove_scheme_should_throw_exception_when_no_response_returned()
    {
        // Arrange
        var req = _fixture.Create<RemoveComplianceSchemeModel>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        Func<Task> act = () => sut.RemoveComplianceScheme(req);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task Select_scheme_should_form_request_from_config_and_return_successful_response()
    {
        // Arrange
        var req = _fixture.Create<SelectSchemeWithUserModel>();

        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl = $"{BaseAddress}/{SelectEndpoint}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var response = await sut.SelectComplianceSchemeAsync(req);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task Select_scheme_should_throw_exception_when_no_response_returned()
    {
        // Arrange
        var req = _fixture.Create<SelectSchemeWithUserModel>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        Func<Task> act = () => sut.SelectComplianceSchemeAsync(req);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task Update_scheme_should_form_request_from_config_and_return_successful_response()
    {
        // Arrange
        var req = _fixture.Create<UpdateSchemeWithUserModel>();

        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl = $"{BaseAddress}/{UpdateEndpoint}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var response = await sut.UpdateComplianceSchemeAsync(req);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task Update_scheme_should_throw_exception_when_no_response_returned()
    {
        // Arrange
        var req = _fixture.Create<UpdateSchemeWithUserModel>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        Func<Task> act = () => sut.UpdateComplianceSchemeAsync(req);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
    
    [TestMethod]
    public async Task GetComplianceSchemesForOperator_ReturnsSuccessfulResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();
        
        var expectedUrl = $"{BaseAddress}/{GetComplianceSchemesForOperatorEndPoint}?organisationId={organisationId}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        
        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);
        
        // Act
        var response = await sut.GetComplianceSchemesForOperatorAsync(organisationId);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }
    
    [TestMethod]
    public async Task GetComplianceSchemeMemberDetails_WhenComplianceSchemeMemberExists_ReturnsSuccessfulResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userOid = Guid.NewGuid();
        var selectedSchemeId = Guid.NewGuid();
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();
        
        var expectedUrl = string.Format($"{BaseAddress}/{GetComplianceSchemeMemberDetails}", organisationId, selectedSchemeId);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        
        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);
        
        // Act
        var response = await sut.GetComplianceSchemeMemberDetailsAsync(userOid, organisationId, selectedSchemeId);

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }
    
    [TestMethod]
    public async Task GetComplianceSchemeMemberDetails_WhenComplianceSchemeMemberDetailsReturnsNoResponse_ShouldThrowException()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userOid = Guid.NewGuid();
        var selectedSchemeId = Guid.NewGuid();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        
        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);
        
        // Act
        Func<Task> act = () => sut.GetComplianceSchemeMemberDetailsAsync(userOid, organisationId, selectedSchemeId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task ComplianceSchemesSummaries_WhenServiceReturnsSuccessfully_ThenControllerReturnsTheSameSummaryData()
    {
        // Arrange
        var complianceSchemeId = Guid.NewGuid();
        var apiEndpoint = string.Format($"{BaseAddress}/{ComplianceSchemeSummaryPath}", complianceSchemeId);

        var complianceSchemeSummary = new ComplianceSchemeSummary
        {
            Name = "Compliance Scheme Name",
            Nation = Nation.England,
            CreatedOn = DateTimeOffset.Now,
            MemberCount = 123,
            MembersLastUpdatedOn = DateTimeOffset.Now
        };

        var serviceResponseJson = JsonSerializer.Serialize(complianceSchemeSummary);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",

                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == apiEndpoint),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(serviceResponseJson)
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var service = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var result = await service.GetComplianceSchemesSummary(complianceSchemeId, organisationId: Guid.NewGuid(), userId: Guid.NewGuid());

        // Assert
        result.Should().BeEquivalentTo(complianceSchemeSummary);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))] // Assert
    public async Task ComplianceSchemesSummaries_WhenServiceReturnsError_ThenControllerThrows()
    {
        // Arrange
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var service = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                }
            ).Verifiable();

        // Act
        await service.GetComplianceSchemesSummary(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
    }

    [TestMethod]
    public async Task GetAllReasonsForRemoval_ReasonsFound_ReturnsSuccessfulResultWithList()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        var expectedUrl = $"{BaseAddress}/{GetAllReasonsForRemovalsEndPoint}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var response = await sut.GetAllReasonsForRemovalsAsync();

        // Assert
        response.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_WhenRequestIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var selectedSchemeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var req = _fixture.Create<RemoveComplianceSchemeMemberModel>();

        var removedComplianceSchemeMemberResponse = new RemoveComplianceSchemeMemberResponse
        {
            OrganisationName = "Compliance Scheme Member Name",
        };

        var serviceResponseJson = JsonSerializer.Serialize(removedComplianceSchemeMemberResponse);
        
        var expectedUrl = string.Format($"{BaseAddress}/{RemoveComplianceSchemeMemberEndPoint}", organisationId, selectedSchemeId);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(serviceResponseJson)
            })
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        
        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);
        
        // Act
        var response = await sut.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, req);

        // Assert
        response.Should().BeEquivalentTo(removedComplianceSchemeMemberResponse);
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_WhenRequestIsValid_ShouldCallLoggingServiceWithRightParameters()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var selectedSchemeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var req = _fixture.Create<RemoveComplianceSchemeMemberModel>();

        var removedComplianceSchemeMemberResponse = new RemoveComplianceSchemeMemberResponse
        {
            OrganisationName = "Compliance Scheme Member Name",
        };

        var serviceResponseJson = JsonSerializer.Serialize(removedComplianceSchemeMemberResponse);

        var expectedUrl = string.Format($"{BaseAddress}/{RemoveComplianceSchemeMemberEndPoint}", organisationId, selectedSchemeId);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(serviceResponseJson)
            })
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var response = await sut.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, req);

        // Assert
        response.Should().BeEquivalentTo(removedComplianceSchemeMemberResponse);

        _loggingServiceMock.Verify(x => x.SendEventAsync(userId, 
            It.Is<ProtectiveMonitoringEvent>(
                monitoringEvent => monitoringEvent.Component == "facade-account-microservice" && 
                                   monitoringEvent.PmcCode == PmcCodes.Code0706 &&
                                   monitoringEvent.Priority == Priorities.NormalEvent &&
                                   monitoringEvent.TransactionCode == TransactionCodes.SchemeMemberRemoved &&
                                   monitoringEvent.Message == $"Scheme membership removed for the organisation id: '{organisationId}' and selected scheme id: {selectedSchemeId}" &&
                                   monitoringEvent.AdditionalInfo == $"OrganisationId: '{organisationId}'"
                )), Times.Once);
        
        _correlationIdProviderMock.Verify(service => service.GetHttpRequestCorrelationIdOrNew(), Times.Once);
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_ProtectiveMonitoringFails_ShouldLogErrorAndNotThrowException()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var selectedSchemeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var req = _fixture.Create<RemoveComplianceSchemeMemberModel>();

        _loggingServiceMock.Setup(x => x.SendEventAsync(
                It.IsAny<Guid>(), It.IsAny<ProtectiveMonitoringEvent>()))
            .Throws(new Exception());

        var removedComplianceSchemeMemberResponse = new RemoveComplianceSchemeMemberResponse
        {
            OrganisationName = "Compliance Scheme Member Name",
        };

        var serviceResponseJson = JsonSerializer.Serialize(removedComplianceSchemeMemberResponse);

        var expectedUrl = string.Format($"{BaseAddress}/{RemoveComplianceSchemeMemberEndPoint}", organisationId, selectedSchemeId);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(serviceResponseJson)
            })
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        var loggerMock = new Mock<ILogger<ComplianceSchemeService>>();
        var sut = new ComplianceSchemeService(httpClient, loggerMock.Object, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var act = async() => await sut.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, req);

        // Assert
        await act.Should().NotThrowAsync();

        loggerMock.Verify( 
            x => x.Log( LogLevel.Error, 
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_WhenNoResponseReturned_ShouldThrowException()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var selectedSchemeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var req = _fixture.Create<RemoveComplianceSchemeMemberModel>();
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        
        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);
        
        // Act
        Func<Task> act = () => sut.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, req);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
    
    [TestMethod]
    public async Task GetInfoForSelectedSchemeRemovalAsync_WhenRequestIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var selectedSchemeId = Guid.NewGuid();
        
        var apiEndpoint = string.Format($"{BaseAddress}/{GetInfoForSelectedSchemeRemoval}", organisationId, selectedSchemeId);
        
        var infoForSelectedSchemeRemovalResponse = new InfoForSelectedSchemeRemovalResponse
        {
            ComplianceSchemeName = "Compliance Scheme Name",
            ComplianceSchemeNation = "England",
            OrganisationName = "Organisation Name",
            OrganisationNation = "England",
            OrganisationNumber = "100 001"
        };

        var serviceResponseJson = JsonSerializer.Serialize(infoForSelectedSchemeRemovalResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",

                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == apiEndpoint),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(serviceResponseJson)
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var service = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var result = await service.GetInfoForSelectedSchemeRemoval(organisationId, selectedSchemeId, userId: Guid.NewGuid());

        // Assert
        result.Should().BeEquivalentTo(infoForSelectedSchemeRemovalResponse);
    }

    [TestMethod]
    public async Task ExportComplianceSchemeSubsidiaries_Returns_List_ExportOrganisationSubsidiariesResponseModel_OnSuccess()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var complianceSchemeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var expectedModel = _fixture.Create<List<ExportOrganisationSubsidiariesResponseModel>>();
        var endpoint = string.Format(ExportComplianceSchemeSubsidiaries, organisationId, complianceSchemeId);
        var expectedUrl = $"{BaseAddress}/{endpoint}";

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

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        var result = await sut.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        // Assert
        Assert.IsNotNull(result);
       result.Should().BeEquivalentTo(expectedModel);
    }

    [TestMethod]
    public async Task ExportComplianceSchemeSubsidiaries_ThrowsException_OnFailure()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var complianceSchemeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new ComplianceSchemeService(httpClient, _logger, _loggingServiceMock.Object, _correlationIdProviderMock.Object, _configuration);

        // Act
        Func<Task> act = () => sut.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

}