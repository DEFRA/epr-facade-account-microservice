﻿using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Models.Enrolments;
using FacadeAccountCreation.Core.Services.Enrolments;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class EnrolmentServiceTests
{
    private readonly EnrolmentService _sut;
    
    private const string BaseAddress = "http://localhost";
    private const string DeleteUserEndpoint = "api/enrolments/delete";
    private const string DeletePersonConnectionAndEnrolmentUserEndpoint = "api/enrolments/v1/delete";
    
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly DeleteUserModel _deleteUserModel;

    public EnrolmentServiceTests()
    {
        var options = Options.Create(new AccountsEndpointsConfig
        {
            DeleteUser = DeleteUserEndpoint,
            DeletePersonConnectionAndEnrolment = DeletePersonConnectionAndEnrolmentUserEndpoint,
        });

        _deleteUserModel = new DeleteUserModel
        {
            PersonExternalIdToDelete = Guid.NewGuid(),
            LoggedInUserId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            ServiceRoleId = 1,
            EnrolmentId = 1
        };
        
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
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == BuildExpectedUrl(false)),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception());
        
        // Act
        await _sut.DeleteUser(_deleteUserModel);
    }
    
    [TestMethod]
    public async Task DeletePersonConnectionAndEnrolment_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        SetupMessageHandlerMock(HttpStatusCode.NoContent, useEnrolmentEndpoint: true);

        // Act
        var response = await _sut.DeletePersonConnectionAndEnrolment(_deleteUserModel);

        // Assert
        VerifySendAsyncIsCalled(useEnrolmentEndpoint: true);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [TestMethod]
    public async Task DeletePersonConnectionAndEnrolment_ShouldReturnBadRequest_WhenInvalid()
    {
        // Arrange
        SetupMessageHandlerMock(HttpStatusCode.BadRequest, useEnrolmentEndpoint: true);

        // Act
        var response = await _sut.DeletePersonConnectionAndEnrolment(_deleteUserModel);

        // Assert
        VerifySendAsyncIsCalled(useEnrolmentEndpoint: true);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task DeletePersonConnectionAndEnrolment_ShouldThrowException_WhenBackendFails()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == BuildExpectedUrl(true)),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception());

        // Act
        await _sut.DeletePersonConnectionAndEnrolment(_deleteUserModel);
    }

    private void SetupMessageHandlerMock(HttpStatusCode httpStatusCode, bool useEnrolmentEndpoint = false)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == BuildExpectedUrl(useEnrolmentEndpoint)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode })
            .Verifiable();
    }

    private void VerifySendAsyncIsCalled(bool useEnrolmentEndpoint = false)
    {
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Delete &&
                req.RequestUri != null &&
                req.RequestUri.ToString() == BuildExpectedUrl(useEnrolmentEndpoint)),
            ItExpr.IsAny<CancellationToken>());
    }

    private string BuildExpectedUrl(bool useEnrolmentEndpoint)
    {
        var endpoint = useEnrolmentEndpoint
            ? $"{DeletePersonConnectionAndEnrolmentUserEndpoint}/{_deleteUserModel.PersonExternalIdToDelete}?userId={_deleteUserModel.LoggedInUserId}&organisationId={_deleteUserModel.OrganisationId}&enrolmentId={_deleteUserModel.EnrolmentId}"
            : $"{DeleteUserEndpoint}/{_deleteUserModel.PersonExternalIdToDelete}?userId={_deleteUserModel.LoggedInUserId}&organisationId={_deleteUserModel.OrganisationId}&serviceRoleId={_deleteUserModel.ServiceRoleId}";

        return $"{BaseAddress}/{endpoint}";
    }
}
