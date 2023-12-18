using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Services.CompaniesHouse;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class CompaniesHouseServiceTests
{
    private const string CompaniesHouseEndpoint = "CompaniesHouse/companies";
    private const string CompaniesHouseNumber = "DummyCompaniesHouseNumber";
    private const string BaseAddress = "http://localhost";
    private const string ExpectedUrl = $"{BaseAddress}/{CompaniesHouseEndpoint}/{CompaniesHouseNumber}";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

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

        var sut = new CompaniesHouseLookupService(httpClient);

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

        var sut = new CompaniesHouseLookupService(httpClient);

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

        var sut = new CompaniesHouseLookupService(httpClient);

        // Act
        var result = await sut.GetCompaniesHouseResponseAsync(CompaniesHouseNumber);

        // Assert
        result.Should().BeNull();
    }
}
