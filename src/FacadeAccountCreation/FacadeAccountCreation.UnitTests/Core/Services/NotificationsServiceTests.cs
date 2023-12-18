using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.Core.Models.Notifications;
using FacadeAccountCreation.Core.Services.Connection;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class NotificationsServiceTests
{
    private const string baseAddress = "http://localhost";
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly NullLogger<NotificationsService> _logger = new();


    [TestMethod]
    public async Task GetNotificationsForServiceAsync_WhenRecordFound_ShouldReturnPersonResponse()
    {
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = $"api/notifications?serviceKey={serviceKey}";
        var expectedUrl = $"{baseAddress}/{endpoint}";

        var apiResponse = _fixture.Create<NotificationsResponse>();

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.OK,
                 Content = new StringContent(JsonSerializer.Serialize(apiResponse))
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var sut = new NotificationsService(httpClient, _logger);

        var result = await sut.GetNotificationsForServiceAsync(userId, organsationId, serviceKey);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeOfType<NotificationsResponse>();
    }

    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task GetNotificationsForServiceAsync_FailedStatusCode_ThrowHttpRequestException(HttpStatusCode statusCode)
    {
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = $"api/notifications?serviceKey={serviceKey}";
        var expectedUrl = $"{baseAddress}/{endpoint}";

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = statusCode
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var sut = new NotificationsService(httpClient, _logger);

        await sut.GetNotificationsForServiceAsync(userId, organsationId, serviceKey);
    }
    
    [TestMethod]
    public async Task GetNotificationsForServiceAsync_WhenRecordNotFound_ShouldReturnNull()
    {
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = $"api/notifications?serviceKey={serviceKey}";
        var expectedUrl = $"{baseAddress}/{endpoint}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
            }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var sut = new NotificationsService(httpClient, _logger);

        var result = await sut.GetNotificationsForServiceAsync(userId, organsationId, serviceKey);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeNull();

    }
}
