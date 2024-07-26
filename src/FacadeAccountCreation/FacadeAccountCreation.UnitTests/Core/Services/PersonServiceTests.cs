//using AutoFixture;
//using AutoFixture.AutoMoq;
//using FacadeAccountCreation.Core.Exceptions;
//using FacadeAccountCreation.Core.Models.Organisations;
//using FacadeAccountCreation.Core.Models.Person;
//using FacadeAccountCreation.Core.Services.Person;
//using FluentAssertions;
//using Moq;
//using Moq.Protected;
//using System.Net;
//using System.Text.Json;

//namespace FacadeAccountCreation.UnitTests.Core.Services;

//[TestClass]
//public class PersonServiceTests
//{
//    private const string PersonEndpoint = "api/persons";
//    private const string BaseAddress = "http://localhost";
//    private const string ExternalIdEndpoint = "api/persons/person-by-externalId";
//    private const string PersonsByInviteToken = "api/persons/person-by-invite-token";
//    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
//    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

//    [TestMethod]
//    public async Task GetPersonByUserIdAsync_WhenValidUserId_ShouldReturnPersonResponse()
//    {
//        var userId = Guid.NewGuid();
//        var expectedUrl = $"{BaseAddress}/{PersonEndpoint}?userId={userId}";
//        var apiResponse = _fixture.Create<PersonResponseModel>();

//        _httpMessageHandlerMock.Protected()
//             .Setup<Task<HttpResponseMessage>>("SendAsync",
//                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
//                 ItExpr.IsAny<CancellationToken>())
//             .ReturnsAsync(new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.OK,
//                 Content = new StringContent(JsonSerializer.Serialize(apiResponse))
//             }).Verifiable();

//        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
//        httpClient.BaseAddress = new Uri(BaseAddress);

//        var sut = new PersonService(httpClient);

//        // Act
//        var result = await sut.GetPersonByUserIdAsync(userId);

//        // Assert
//        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
//            ItExpr.Is<HttpRequestMessage>(
//                req => req.Method == HttpMethod.Get &&
//                       req.RequestUri != null &&
//                       req.RequestUri.ToString() == expectedUrl),
//            ItExpr.IsAny<CancellationToken>());

//        result.Should().BeOfType<PersonResponseModel>();
//    }

//    [TestMethod]
//    public async Task GetPersonByUserIdAsync_WhenInvalidUserId_ShouldReturnNull()
//    {
//        var userId = Guid.NewGuid();
//        var expectedUrl = $"{BaseAddress}/{PersonEndpoint}?userId={userId}";

//        _httpMessageHandlerMock.Protected()
//             .Setup<Task<HttpResponseMessage>>("SendAsync",
//                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
//                 ItExpr.IsAny<CancellationToken>())
//             .ReturnsAsync(new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.NoContent
//             }).Verifiable();

//        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
//        httpClient.BaseAddress = new Uri(BaseAddress);

//        var sut = new PersonService(httpClient);

//        // Act
//        var result = await sut.GetPersonByUserIdAsync(userId);

//        // Assert
//        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
//            ItExpr.Is<HttpRequestMessage>(
//                req => req.Method == HttpMethod.Get &&
//                       req.RequestUri != null &&
//                       req.RequestUri.ToString() == expectedUrl),
//            ItExpr.IsAny<CancellationToken>());

//        result.Should().BeNull();
//    }
    
//    [TestMethod]
//    public async Task GetPersonByExternalIdAsync_WhenValidUserId_ShouldReturnPersonResponse()
//    {
//        var externalId = Guid.NewGuid();
//        var expectedUrl = $"{BaseAddress}/{ExternalIdEndpoint}?externalId={externalId}";
//        var apiResponse = _fixture.Create<PersonResponseModel>();

//        _httpMessageHandlerMock.Protected()
//            .Setup<Task<HttpResponseMessage>>("SendAsync",
//                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.OK,
//                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
//            }).Verifiable();

//        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
//        httpClient.BaseAddress = new Uri(BaseAddress);

//        var sut = new PersonService(httpClient);

//        // Act
//        var result = await sut.GetPersonByExternalIdAsync(externalId);

//        // Assert
//        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
//            ItExpr.Is<HttpRequestMessage>(
//                req => req.Method == HttpMethod.Get &&
//                       req.RequestUri != null &&
//                       req.RequestUri.ToString() == expectedUrl),
//            ItExpr.IsAny<CancellationToken>());

//        result.Should().BeOfType<PersonResponseModel>();
//    }

//    [TestMethod]
//    [ExpectedException(typeof(ProblemResponseException))]
//    public async Task GetPersonByExternalIdAsync_WhenInValidUserId_ShouldReturnProblemResponseExceptionResponse()
//    {
//        var externalId = Guid.NewGuid();
//        var expectedUrl = $"{BaseAddress}/{ExternalIdEndpoint}?externalId={externalId}";
//        var apiResponse = _fixture.Create<ProblemResponseException>();

//        _httpMessageHandlerMock.Protected()
//            .Setup<Task<HttpResponseMessage>>("SendAsync",
//                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.BadGateway,
//                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
//            }).Verifiable();

//        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
//        httpClient.BaseAddress = new Uri(BaseAddress);

//        var sut = new PersonService(httpClient);

//        // Act
//        var result = await sut.GetPersonByExternalIdAsync(externalId);

//        // Assert
//        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
//            ItExpr.Is<HttpRequestMessage>(
//                req => req.Method == HttpMethod.Get &&
//                       req.RequestUri != null &&
//                       req.RequestUri.ToString() == expectedUrl),
//            ItExpr.IsAny<CancellationToken>());

//        result.Should().BeOfType<ProblemResponseException>();
//    }

//    [TestMethod]
//    public async Task GetPersonByExternalIdAsync_WhenInvalidUserId_ShouldReturnNull()
//    {
//        var externalId = Guid.NewGuid();
//        var expectedUrl = $"{BaseAddress}/{ExternalIdEndpoint}?externalId={externalId}";

//        _httpMessageHandlerMock.Protected()
//            .Setup<Task<HttpResponseMessage>>("SendAsync",
//                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.NoContent
//            }).Verifiable();

//        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
//        httpClient.BaseAddress = new Uri(BaseAddress);

//        var sut = new PersonService(httpClient);

//        // Act
//        var result = await sut.GetPersonByExternalIdAsync(externalId);

//        // Assert
//        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
//            ItExpr.Is<HttpRequestMessage>(
//                req => req.Method == HttpMethod.Get &&
//                       req.RequestUri != null &&
//                       req.RequestUri.ToString() == expectedUrl),
//            ItExpr.IsAny<CancellationToken>());

//        result.Should().BeNull();
//    }

//    [TestMethod]
//    public async Task GetPersonByInviteToken_ShouldReturnPersonResponse()
//    {
//        //Arrange
//        var token = "inviteToken";
//        var expectedUrl = $"{BaseAddress}/{PersonsByInviteToken}?token={token}";
//        var apiResponse = _fixture.Create<InviteApprovedUserModel>();

//        _httpMessageHandlerMock.Protected()
//            .Setup<Task<HttpResponseMessage>>("SendAsync",
//                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.OK,
//                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
//            }).Verifiable();

//        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
//        httpClient.BaseAddress = new Uri(BaseAddress);

//        var sut = new PersonService(httpClient);

//        // Act
//        var result = await sut.GetPersonByInviteToken(token);

//        // Assert
//        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
//            ItExpr.Is<HttpRequestMessage>(
//                req => req.Method == HttpMethod.Get &&
//                       req.RequestUri != null &&
//                       req.RequestUri.ToString() == expectedUrl),
//            ItExpr.IsAny<CancellationToken>());

//        result.Should().BeOfType<InviteApprovedUserModel>();
//    }
    
//    [TestMethod]
//    public async Task GetPersonByInviteToken_ShouldReturnNull()
//    {
//        //Arrange
//        var token = "inviteToken";
//        var expectedUrl = $"{BaseAddress}/{PersonsByInviteToken}?token={token}";

//        _httpMessageHandlerMock.Protected()
//            .Setup<Task<HttpResponseMessage>>("SendAsync",
//                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.NoContent
//            }).Verifiable();
        
//        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
//        httpClient.BaseAddress = new Uri(BaseAddress);

//        var sut = new PersonService(httpClient);

//        // Act
//        var result = await sut.GetPersonByInviteToken(token);

//        // Assert
//        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
//            ItExpr.Is<HttpRequestMessage>(
//                req => req.Method == HttpMethod.Get &&
//                       req.RequestUri != null &&
//                       req.RequestUri.ToString() == expectedUrl),
//            ItExpr.IsAny<CancellationToken>());

//        result.Should().BeNull();
//    }    
//}
