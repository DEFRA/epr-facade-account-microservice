using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Services.CreateAccount;
using System.Collections.ObjectModel;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class AccountServiceTests
{
    private const string OrganisationEndpoint = "api/organisations";
    private const string AccountsEndpoint = "api/producer-accounts";
    private const string ReprocessorExporterAccountsEndpoint = "api/v1/reprocessor-exporter-user-accounts";
    private const string CompaniesHouseNumber = "88888888";
    private const string BaseAddress = "http://localhost";
    private const string ExpectedUrl = $"{BaseAddress}/{OrganisationEndpoint}?companiesHouseNumber={CompaniesHouseNumber}";
    private const string EnrolInvitedUserEndpoint = "api/accounts-management/enrol-invited-user";
    private const string AddAccountPostUrl = $"{BaseAddress}/{AccountsEndpoint}";
    private const string AddReprocessorExporterAccountPostUrl = $"{BaseAddress}/{ReprocessorExporterAccountsEndpoint}";
    private const string EnrolInvitedUserUri = $"{BaseAddress}/{EnrolInvitedUserEndpoint}";
    private const string AddApprovedUserUri = $"{BaseAddress}/api/producer-accounts/ApprovedUser";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    
    [TestMethod]
    public async Task Should_return_correct_organisations_response()
    {
        // Arrange
        var apiResponse = _fixture.Create<List<OrganisationResponseModel>?>();

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.OK,
                 Content = new StringContent(JsonSerializer.Serialize(apiResponse))
             }).Verifiable();

        var sut = GetAccountService();

        // Act
        var result = await sut.GetOrganisationsByCompanyHouseNumberAsync(CompaniesHouseNumber);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
        result.Should().BeOfType<ReadOnlyCollection<OrganisationResponseModel>?>();
        apiResponse.Should().BeEquivalentTo(result);
        var resultDetail = result as ReadOnlyCollection<OrganisationResponseModel>;
        resultDetail?.Count.Should().NotBe(0);
    }

    [TestMethod]
    public async Task Should_return_empty_response_when_no_organisations_found()
    {
        // Arrange
        var apiResponse = new List<OrganisationResponseModel>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var sut = GetAccountService();

        // Act
        var result = await sut.GetOrganisationsByCompanyHouseNumberAsync(CompaniesHouseNumber);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
        result.Should().BeOfType<ReadOnlyCollection<OrganisationResponseModel>?>();
        apiResponse.Should().BeEquivalentTo(result);
        var resultDetail = result as ReadOnlyCollection<OrganisationResponseModel>;
        resultDetail?.Count.Should().Be(0);
    }

    [TestMethod]
    public async Task Should_successfully_post_to_AddAccount_endpoint()
    {
        // Arrange
        var apiRequest = _fixture.Create<AccountWithUserModel>();
        var apiResponse = _fixture.Create<CreateAccountResponse>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == AddAccountPostUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            { 
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var sut = GetAccountService();

        // Act
        await sut.AddAccountAsync(apiRequest);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == AddAccountPostUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task AddReprocessorExporterAccountAsync_HappyPath_ReturnsSuccessByNotThrowing()
    {
        // Arrange
        var apiRequest = _fixture.Create<ReprocessorExporterAccountWithUserModel>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == AddReprocessorExporterAccountPostUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            }).Verifiable();

        var sut = GetAccountService();

        // Act
        await sut.AddReprocessorExporterAccountAsync(apiRequest);

        // Assert

        //todo: this fails sonar
        // the test passes if no exception is thrown (MSTest doesn't have explicit support for checking no exception thrown)
    }

    [TestMethod]
    public async Task AddReprocessorExporterAccountAsync_HappyPath_CreateReprocessorExporterAccountEndpointCalled()
    {
        // Arrange
        var apiRequest = _fixture.Create<ReprocessorExporterAccountWithUserModel>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == AddReprocessorExporterAccountPostUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            }).Verifiable();

        var sut = GetAccountService();

        // Act
        await sut.AddReprocessorExporterAccountAsync(apiRequest);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == AddReprocessorExporterAccountPostUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task AddReprocessorExporterAccountAsync_CreateReprocessorExporterAccountReturnsConflict_ProblemExceptionThrown()
    {
        // Arrange
        var apiRequest = _fixture.Create<ReprocessorExporterAccountWithUserModel>();
        var apiResponse = _fixture.Create<ProblemDetails>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == AddReprocessorExporterAccountPostUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var sut = GetAccountService();

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ProblemResponseException>(async () => await sut.AddReprocessorExporterAccountAsync(apiRequest));
        Assert.IsNotNull(exception.ProblemDetails);
        Assert.AreEqual(apiResponse.Detail, exception.ProblemDetails.Detail);
        Assert.AreEqual(apiResponse.Type, exception.ProblemDetails.Type);
    }

    [TestMethod]
    public async Task AddReprocessorExporterAccountAsync_CreateReprocessorExporterAccountReturnsNonConflictError_500HttpRequestExceptionExceptionThrown()
    {
        // Arrange
        var apiRequest = _fixture.Create<ReprocessorExporterAccountWithUserModel>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == AddReprocessorExporterAccountPostUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("")
            }).Verifiable();

        var sut = GetAccountService();

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<HttpRequestException>(async () => await sut.AddReprocessorExporterAccountAsync(apiRequest));
        Assert.AreEqual(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [TestMethod]
    public async Task Should_successfully_post_to_enrol_invited_user_endpoint()
    {
        // Arrange
        var apiRequest = _fixture.Create<EnrolInvitedUserModel>();
        var apiResponse = _fixture.Create<HttpResponseMessage>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == EnrolInvitedUserUri),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            { 
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = GetAccountService();

        // Act
        await sut.EnrolInvitedUserAsync(apiRequest);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == EnrolInvitedUserUri),
            ItExpr.IsAny<CancellationToken>());
    }

    private AccountService GetAccountService()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var accountsEndpointsOptions = Options.Create(new AccountsEndpointsConfig
        {
            Accounts = "api/producer-accounts",
            Organisations = "api/organisations",
            InviteUser = "api/accounts-management/invite-user",
            EnrolInvitedUser = "api/accounts-management/enrol-invited-user",
            ApprovedUserAccounts = "/api/producer-accounts/ApprovedUser",
            ReprocessorExporterAccounts = "api/v1/reprocessor-exporter-user-accounts"
        });
        
        var sut = new AccountService(httpClient, accountsEndpointsOptions);

        return sut;
    }
    
    [TestMethod]
    public async Task Should_return_null_when_response_has_no_content()
    {
        // Arrange

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            }).Verifiable();

        var sut = GetAccountService();

        // Act
        var result = await sut.GetOrganisationsByCompanyHouseNumberAsync(CompaniesHouseNumber);

        // Assert
        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task AddApprovedUserAccountAsync_ShouldReturnPersonResponse()
    {
        // Arrange
        var apiResponse = _fixture.Create<CreateAccountResponse?>();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == AddApprovedUserUri),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            { 
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        var sut = GetAccountService();
        
        //Act
        await sut.AddApprovedUserAccountAsync(It.IsAny<AccountModel>());

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == AddApprovedUserUri),
            ItExpr.IsAny<CancellationToken>());
    }
}
