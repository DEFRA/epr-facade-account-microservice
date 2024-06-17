using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Models.Enrolments;
using FacadeAccountCreation.Core.Services.Enrolments;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class EnrolmentServiceTests
{
    private readonly EnrolmentService _sut;
    
    private const string BaseAddress = "http://localhost";
    private const string DeleteUserEndpoint = "api/enrolments/delete";
    
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly DeleteUserModel _deleteUserModel;
    private readonly string _expectedUrl;

    public EnrolmentServiceTests()
    {
        var options = Options.Create(new AccountsEndpointsConfig()
        {
            DeleteUser = DeleteUserEndpoint
        });

        _deleteUserModel = new DeleteUserModel()
        {
            PersonExternalIdToDelete = Guid.NewGuid(),
            LoggedInUserId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            ServiceRoleId = 1
        };
        
        _expectedUrl = $"{BaseAddress}/{DeleteUserEndpoint}/{_deleteUserModel.PersonExternalIdToDelete}?userId={_deleteUserModel.LoggedInUserId}&organisationId={_deleteUserModel.OrganisationId}&serviceRoleId={_deleteUserModel.ServiceRoleId}";
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);
        
        _sut = new EnrolmentService(httpClient, options);
    }

    [TestMethod]
    public async Task Should_return_NoContent_when_deletion_is_successful()
    {
        // Arrange
        SetupMessageHandlerMock(HttpStatusCode.NoContent);
        
        // Act
        var response = await _sut.DeleteUser(_deleteUserModel);

        // Assert
        VerifySendAsyncIsCalled();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [TestMethod]
    public async Task Should_return_BadRequest_when_request_parameters_are_invalid()
    {
        // Arrange
        SetupMessageHandlerMock(HttpStatusCode.BadRequest);
        
        // Act
        var response = await _sut.DeleteUser(_deleteUserModel);
        
        // Assert
        VerifySendAsyncIsCalled();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task Should_expect_exception_when_backend_fails()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == _expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception());
        
        // Act
        await _sut.DeleteUser(_deleteUserModel);
    }

    private void SetupMessageHandlerMock(HttpStatusCode httpStatusCode)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == _expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                }
            ).Verifiable();
    }

    private void VerifySendAsyncIsCalled()
    {
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Delete &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == _expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }
}
