using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Models.Connections;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using FacadeAccountCreation.Core.Services.Connection;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class ConnectionsServiceTests
{
    private const string baseAddress = "http://localhost";
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly NullLogger<RoleManagementService> _logger = new();
    private IOptions<ConnectionsEndpointsConfig> connectionsEndpointsConfig;

    [TestInitialize]
    public void Setup()
    {
        connectionsEndpointsConfig = Options.Create(new ConnectionsEndpointsConfig
        {
            Enrolments = "api/connections/{0}/roles?serviceKey={1}",
            Person = "api/connections/{0}/person?serviceKey={1}"
        });
    }

    [TestMethod]
    public async Task GetPerson_WhenRecordFound_ShouldReturnPersonResponse()
    {
        var connectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = string.Format(connectionsEndpointsConfig.Value.Person, connectionId.ToString(), serviceKey);
        var expectedUrl = $"{baseAddress}/{endpoint}";

        var apiResponse = _fixture.Create<ConnectionPersonModel>();

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

        var sut = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        var result = await sut.GetPerson(connectionId, serviceKey, userId, organsationId);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeOfType<ConnectionPersonModel>();
    }

    [TestMethod]
    public async Task GetPerson_WhenRecordNotFound_ShouldReturnNull()
    {
        var connectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = string.Format(connectionsEndpointsConfig.Value.Person, connectionId.ToString(), serviceKey);
        var expectedUrl = $"{baseAddress}/{endpoint}";

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.NotFound
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var sut = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        var result = await sut.GetPerson(connectionId, serviceKey, userId, organsationId);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeNull();
    }

    [ExpectedException(typeof(HttpRequestException))]
    [TestMethod]
    public async Task GetPerson_WhenBadRequest_ShouldError()
    {
        var connectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = string.Format(connectionsEndpointsConfig.Value.Person, connectionId.ToString(), serviceKey);
        var expectedUrl = $"{baseAddress}/{endpoint}";

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.BadRequest
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var sut = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        _= await sut.GetPerson(connectionId, serviceKey, userId, organsationId);
    }

    [TestMethod]
    public async Task GetEnrolments_WhenRecordFound_ShouldReturnPersonResponse()
    {
        var connectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = string.Format(connectionsEndpointsConfig.Value.Enrolments, connectionId.ToString(), serviceKey);
        var expectedUrl = $"{baseAddress}/{endpoint}";

        var apiResponse = _fixture.Create<ConnectionWithEnrolmentsModel>();

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

        var sut = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        var result = await sut.GetEnrolments(connectionId, serviceKey, userId, organsationId);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeOfType<ConnectionWithEnrolmentsModel>();
    }

    [TestMethod]
    public async Task GetEnrolments_WhenRecordNotFound_ShouldReturnNull()
    {
        var connectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = string.Format(connectionsEndpointsConfig.Value.Enrolments, connectionId.ToString(), serviceKey);
        var expectedUrl = $"{baseAddress}/{endpoint}";

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.NotFound
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var sut = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        var result = await sut.GetEnrolments(connectionId, serviceKey, userId, organsationId);

        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());

        result.Should().BeNull();
    }

    [ExpectedException(typeof(HttpRequestException))]
    [TestMethod]
    public async Task GetEnrolments_WhenBadRequest_ShouldError()
    {
        var connectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organsationId = Guid.NewGuid();
        var serviceKey = "packaging";

        var endpoint = string.Format(connectionsEndpointsConfig.Value.Enrolments, connectionId.ToString(), serviceKey);
        var expectedUrl = $"{baseAddress}/{endpoint}";

        _httpMessageHandlerMock.Protected()
             .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
                 ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.BadRequest
             }).Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        httpClient.BaseAddress = new Uri(baseAddress);

        var sut = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        _ = await sut.GetEnrolments(connectionId, serviceKey, userId, organsationId);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    [TestCategory("AcceptNominationToDelegatedPerson")]
    public async Task AcceptNominationToDelegatedPerson_WhenServiceReturnsBadRequest_ThenThrow()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var service = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(
                new HttpResponseMessage 
                {
                    StatusCode = HttpStatusCode.BadRequest
                }
            ).Verifiable();

        await service.AcceptNominationToDelegatedPerson(
            enrolmentId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            organisationId: Guid.NewGuid(),
            serviceKey: "Packaging",
            acceptNominationRequest: new AcceptNominationRequest
            {
                NomineeDeclaration = "Nominee Declaration",
                Telephone = "01234000000"
            });
    }

    [TestMethod]
    [TestCategory("AcceptNominationToDelegatedPerson")]
    public async Task AcceptNominationToDelegatedPerson_WhenAcceptingNomination_ThenPutCallIsCalledOnceOnCorrectEndpoint()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var service = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                }
            ).Verifiable();

        var enrolmentId = Guid.NewGuid();

        var result = await service.AcceptNominationToDelegatedPerson(
            enrolmentId: enrolmentId,
            userId: Guid.NewGuid(),
            organisationId: Guid.NewGuid(),
            serviceKey: "Packaging",
            acceptNominationRequest: new AcceptNominationRequest
            {
                NomineeDeclaration = "Nominee Declaration",
                Telephone = "01234000000"
            });

        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var expectedUri = new Uri(new Uri(baseAddress), $"api/enrolments/{enrolmentId}/delegated-person-acceptance?serviceKey=Packaging");

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Put &&
                request.RequestUri == expectedUri),
            ItExpr.IsAny<CancellationToken>()
            );
    }

    [TestMethod]
    public async Task UpdatePersonRole_WhenUpdatedFine_ThenRetunrsOk()
    {
        //Arrange
        var connectionId = Guid.NewGuid();
        var serviceKey = "packaging";
        var apiResponse = _fixture.Create<UpdatePersonRoleResponse>();
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var service = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(apiResponse))
                }
            ).Verifiable();
        
        //Act
        var result = await service.UpdatePersonRole(
            connectionId: connectionId,
            userId: Guid.NewGuid(),
            organisationId: Guid.NewGuid(),
            serviceKey: "Packaging",
            updateRequest: new UpdatePersonRoleRequest
            {
                PersonRole = PersonRole.Admin
            });

        //Assert
        result.Should().NotBeNull();
        Assert.IsTrue(result.RemovedServiceRoles.Count >= 1);

        var expectedUri = new Uri(new Uri(baseAddress), $"api/connections/{connectionId}/roles?serviceKey=Packaging");
        
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Put &&
                request.RequestUri == expectedUri),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    
    [TestMethod]
    public async Task NominateToDelegatedPerson_WhenUpdatedFine_ThenRetunrsOk()
    {
        //Arrange
        var connectionId = Guid.NewGuid();
        var serviceKey = "packaging";
        var apiResponse = _fixture.Create<UpdatePersonRoleResponse>();
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress)
        };

        var service = new RoleManagementService(httpClient, _logger, connectionsEndpointsConfig);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(apiResponse))
                }
            ).Verifiable();
        
        //Act
        var result = await service.NominateToDelegatedPerson(
            connectionId: connectionId,
            userId: Guid.NewGuid(),
            organisationId: Guid.NewGuid(),
            serviceKey: "Packaging",
            nominationRequest: new DelegatedPersonNominationRequest()
            {
                RelationshipType = RelationshipType.Employment
            });

        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var expectedUri = new Uri(new Uri(baseAddress), $"api/connections/{connectionId}/delegated-person-nomination?serviceKey=Packaging");
        
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Put &&
                request.RequestUri == expectedUri),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
