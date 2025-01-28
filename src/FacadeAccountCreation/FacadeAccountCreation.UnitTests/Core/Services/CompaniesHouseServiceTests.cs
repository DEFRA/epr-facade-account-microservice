using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Services.CompaniesHouse;
using Microsoft.FeatureManagement;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class CompaniesHouseServiceTests
{
    private const string CompaniesHouseEndpointForOAuth = "companies";
    private const string CompaniesHouseEndpointForCertificateAuth = "CompaniesHouse/companies";
    private const string CompaniesHouseNumber = "DummyCompaniesHouseNumber";
    private const string BaseAddress = "http://localhost";
    private const string ExpectedUrl = $"{BaseAddress}/{CompaniesHouseEndpointForOAuth}/{CompaniesHouseNumber}";
    private const string ExpectedUrlForCertificateAuth = $"{BaseAddress}/{CompaniesHouseEndpointForCertificateAuth}/{CompaniesHouseNumber}";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IFeatureManager> _featureManagerMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.UseBoomiOAuth))
            .ReturnsAsync(true);
    }

    [TestMethod]
    public async Task Should_return_correct_companies_house_lookup_response()
    {
        // Arrange
        var apiResponse = _fixture.Create<CompaniesHouseResponse>();

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.OK,
                 Content = new StringContent(JsonSerializer.Serialize(apiResponse))
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new CompaniesHouseLookupService(httpClient, _featureManagerMock.Object);

        // Act
        var result = await sut.GetCompaniesHouseResponseAsync(CompaniesHouseNumber);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeOfType<CompaniesHouseResponse>();
    }

    [TestMethod]
    public async Task Should_return_correct_companies_house_lookup_response_when_feature_flag_is_on()
    {
        // Arrange
        var apiResponse = _fixture.Create<CompaniesHouseResponse>();

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.UseBoomiOAuth))
            .ReturnsAsync(false);

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrlForCertificateAuth),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.OK,
                 Content = new StringContent(JsonSerializer.Serialize(apiResponse))
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new CompaniesHouseLookupService(httpClient, _featureManagerMock.Object);

        // Act
        var result = await sut.GetCompaniesHouseResponseAsync(CompaniesHouseNumber);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrlForCertificateAuth),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeOfType<CompaniesHouseResponse>();
    }

    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task Should_throw_exception_when_api_returns_error(HttpStatusCode returnedStatusCode)
    {
        // Arrange
        var errorResponse = new CompaniesHouseErrorResponse
        {
            InnerException = new InnerExceptionResponse
            {
                Code = ((int)returnedStatusCode).ToString()
            }
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = returnedStatusCode,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new CompaniesHouseLookupService(httpClient, _featureManagerMock.Object);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => sut.GetCompaniesHouseResponseAsync(CompaniesHouseNumber));

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
        exception.Should().BeOfType<HttpRequestException>();
    }
    
    [TestMethod]
    public async Task Should_return_null_when_respone_statuscode_notfound()
    {
        // Arrange
        var errorResponse = new CompaniesHouseErrorResponse
        {
            InnerException = new InnerExceptionResponse
            {
                Code = ((int)HttpStatusCode.NotFound).ToString()
            }
        };
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new CompaniesHouseLookupService(httpClient, _featureManagerMock.Object);

        // Act
        var result = await sut.GetCompaniesHouseResponseAsync(CompaniesHouseNumber);

        // Assert
        result.Should().BeNull();
    }
}
