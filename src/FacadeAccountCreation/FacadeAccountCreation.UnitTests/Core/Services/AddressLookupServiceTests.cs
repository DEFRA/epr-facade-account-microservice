using FacadeAccountCreation.Core.Models.AddressLookup;
using FacadeAccountCreation.Core.Services.AddressLookup;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class AddressLookupServiceTests
{
    private const string PostcodeEndpoint = "postcodes";
    private const string PostcodeQueryStringKey = "postcode";
    private const string PostcodeQueryStringValue = "DummyPostcode";
    private const string BaseAddress = "http://localhost";
    private const string ExpectedUrl = $"{BaseAddress}/{PostcodeEndpoint}?{PostcodeQueryStringKey}={PostcodeQueryStringValue}";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

    [TestMethod]
    public async Task Should_return_correct_address_lookup_response()
    {
        // Arrange
        var apiResponse = _fixture.Create<AddressLookupResponseDto>();

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
        
        var sut = new AddressLookupService(httpClient);
        
        // Act
        var result = await sut.GetAddressLookupResponseAsync(PostcodeQueryStringValue);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
        result.Should().BeOfType<AddressLookupResponseDto>();
        apiResponse.Should().BeEquivalentTo(result);
    }

    [TestMethod]
    public async Task Should_return_empty_response_when_no_addresses_found()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<AddressLookupResponseDto>()
            .Without(x => x.Results)
            .Create();

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
        
        var sut = new AddressLookupService(httpClient);
        
        // Act
        var result = await sut.GetAddressLookupResponseAsync(PostcodeQueryStringValue);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
        result.Should().BeOfType<AddressLookupResponseDto>();
        apiResponse.Should().BeEquivalentTo(result);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task Should_throw_exception_when_api_returns_error(HttpStatusCode returnedStatusCode)
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = returnedStatusCode
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        
        var sut = new AddressLookupService(httpClient);
        
        // Act
        var exception = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => sut.GetAddressLookupResponseAsync(PostcodeQueryStringValue));

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
    [DataRow(HttpStatusCode.NoContent)]
    [DataRow(HttpStatusCode.BadRequest)]
    public async Task Should_return_null_When_NoContent_or_StatusCode_400(HttpStatusCode returnedStatusCode)
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = returnedStatusCode,
                Content = new StringContent(@"{""error"": { ""statuscode"": 400 }}")
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var sut = new AddressLookupService(httpClient);

        // Act
        var response = await sut.GetAddressLookupResponseAsync(PostcodeQueryStringValue);

        // Assert
        Assert.IsNull(response);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }
}
