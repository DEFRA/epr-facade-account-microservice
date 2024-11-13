using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.Core.Models.PaymentCalculation;
using FacadeAccountCreation.Core.Services.PaymentCalculation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class PaymentCalculationServiceTests
{
    private const string BaseAddress = "http://localhost";
    private const string ProducerRegistrationFeesUri = "api/producer/registration-fee";
    private const string PaymentInitiationUrl = "api/organisations/nation-code";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Mock<ILogger<PaymentCalculationService>> _loggerMock = null!;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private HttpClient _httpClient = null!;

    private readonly PaymentCalculationRequest _paymentCalculationRequest = new PaymentCalculationRequest
    {
        ApplicationReferenceNumber = "Test",
        ProducerType = "Large",
        Regulator = "GB-ENG",
        NoOfSubsidiariesOnlineMarketplace = 0,
        IsProducerOnlineMarketplace = false,
        NumberOfSubsidiaries = 0,
        IsLateFeeApplicable = false,
        SubmissionDate = DateTime.UtcNow
    };

    private PaymentCalculationService _service = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _loggerMock = new Mock<ILogger<PaymentCalculationService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress)
        };
        _service = new PaymentCalculationService(_httpClient, _loggerMock.Object);
    }

    [TestMethod]
    public async Task ProducerRegistrationFees_ShouldReturnCorrectResponse()
    {
        // Arrange
        var apiResponse = _fixture.Create<PaymentCalculationResponse>();

        var expectedUrl = $"{BaseAddress}/{ProducerRegistrationFeesUri}";

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

        //Act
        await _service.ProducerRegistrationFees(_paymentCalculationRequest);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task ProducerRegistrationFees_ShouldReturnUnsuccessfulResponse()
    {
        // Arrange
        var apiResponse = _fixture.Create<HttpRequestException>();

        var expectedUrl = $"{BaseAddress}/{ProducerRegistrationFeesUri}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            }).Verifiable();

        //Act
        var act = async () => await _service.ProducerRegistrationFees(_paymentCalculationRequest);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldReturnRedirectUrl_WhenHtmlContainsRedirectUrl()
    {
        // Arrange
        var requestUrl = $"{BaseAddress}/{PaymentInitiationUrl}";
        var request = new PaymentInitiationRequest { Reference = "TestRef" };
        var expectedReturnUrl = "https://card.payments.service.gov.uk/secure/9defb517-66f8-45cd-8d9b-20e571b76fb5";
        var htmlContent = $@"<!DOCTYPE html><html lang=""en""><script>window.location.href = '{expectedReturnUrl}';</script>";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(htmlContent, Encoding.UTF8, "text/html")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == requestUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        var result = await _service.PaymentInitiation(request);

        // Assert
        result.Should().Be(expectedReturnUrl);
            
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Attempting to initialise Payment request for {request.Reference}")),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!),
            Times.Once);
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldReturnNull_WhenOkResponseHtmlContentIsEmpty()
    {
        // Arrange
        var requestUrl = $"{BaseAddress}/{PaymentInitiationUrl}";
        var request = new PaymentInitiationRequest { Reference = "TestRef" };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", Encoding.UTF8, "text/html")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == requestUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        var result = await _service.PaymentInitiation(request);

        // Assert
        result.Should().BeNull();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Redirect URL not found in the initialise Payment response.")),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!),
            Times.Once);
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldReturnNull_WhenNoContentResponseHtmlContentIsEmpty()
    {
        // Arrange
        var requestUrl = $"{BaseAddress}/{PaymentInitiationUrl}";
        var request = new PaymentInitiationRequest { Reference = "TestRef" };

        var response = new HttpResponseMessage(HttpStatusCode.NoContent)
        {
            Content = new StringContent("", Encoding.UTF8, "text/html")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == requestUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        var result = await _service.PaymentInitiation(request);

        // Assert
        result.Should().BeNull();
            
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Redirect URL not found in the initialise Payment response.")),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!),
            Times.Once);
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldLogErrorAndThrowException_WhenHttpRequestFails()
    {
        // Arrange
        var requestUrl = $"{BaseAddress}/{PaymentInitiationUrl}";
        var request = new PaymentInitiationRequest { Reference = "TestRef" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == requestUrl),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Request failed"))
            .Verifiable();

        // Act
        Func<Task> act = async () => await _service.PaymentInitiation(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Request failed");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Attempting to initialise Payment request for {request.Reference}")),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!),
            Times.Once);
    }

    [TestMethod]
    public void PaymentInitiation_Regex_ShouldCaptureUrlCorrectly()
    {
        // Arrange
        const string pattern = @"window\.location\.href\s*=\s*'(?<url>.*?)';";
        var htmlContent = "<script>window.location.href = 'https://example.com/redirect';</script>";
    
        // Act
        var match = Regex.Match(htmlContent, pattern);
    
        // Assert
        match.Success.Should().BeTrue();
        match.Groups["url"].Value.Should().Be("https://example.com/redirect");
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldClearHeadersInFinallyBlock()
    {
        // Arrange
        var requestUrl = $"{BaseAddress}/{PaymentInitiationUrl}";
        var request = new PaymentInitiationRequest { Reference = "TestRef" };
        var htmlContent = "<html><script>window.location.href = 'https://example.com/redirect';</script></html>";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(htmlContent, Encoding.UTF8, "text/html")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == requestUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        await _service.PaymentInitiation(request);

        // Assert
        _httpMessageHandlerMock.Invocations.Clear();
        _httpClient.DefaultRequestHeaders.Should().BeEmpty();
    }
}