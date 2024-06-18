using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Services.CreateAccount;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class InviteUserTests
{
    private const string BaseAddress = "http://localhost";
    private const string SendInvitationEndpoint = "api/accounts-management/invite-user";
    private const string ExpectedUrl = $"{BaseAddress}/{SendInvitationEndpoint}";
    
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly AccountInvitationModel _requestModel = new()
    {
        InvitedUser = new InvitedUserModel()
        {
            Email = "test-invited-user@test.com",
            PersonRoleId = 2,
            ServiceRoleId = 3,
            OrganisationId = Guid.NewGuid()
        },
        InvitingUser = new InvitingUserModel()
        {
            Email = "test-inviter@test.com",
            UserId = Guid.NewGuid()
        }
    };
    
    [TestMethod]
    public async Task Should_Return_Correct_Account_Invitation_Response()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.OK)
            .Create();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse).Verifiable();

        var sut = GetAccountService();

        // Act
        var result = await sut.SaveInviteAsync(_requestModel);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
        
        result.Should().BeEquivalentTo(apiResponse);
    }
    
    [TestMethod]
    public async Task Should_Return_Bad_Request_When_User_Already_Invited()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.BadRequest)
            .Create();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse).Verifiable();

        var sut = GetAccountService();

        // Act
        var result = await sut.SaveInviteAsync(_requestModel);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
        
        result.Should().BeEquivalentTo(apiResponse);
    }
    
    [TestMethod]
    public async Task Should_Return_Internal_Server_Error_When_Organisation_Is_Invalid()
    {
        // Arrange
        var apiResponse = _fixture
            .Build<HttpResponseMessage>()
            .With(x => x.StatusCode, HttpStatusCode.InternalServerError)
            .Create();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == ExpectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(apiResponse).Verifiable();

        var sut = GetAccountService();

        // Act
        var result = await sut.SaveInviteAsync(_requestModel);

        // Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Post &&
                       req.RequestUri != null &&
                       req.RequestUri.ToString() == ExpectedUrl),
            ItExpr.IsAny<CancellationToken>());
        
        result.Should().BeEquivalentTo(apiResponse);
    }
    
    private AccountService GetAccountService()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri(BaseAddress);

        var accountsEndpointsOptions = Options.Create(new AccountsEndpointsConfig()
        {
            Accounts = "api/producer-accounts",
            Organisations = "api/organisations",
            InviteUser = "api/accounts-management/invite-user"
        });
        
        var sut = new AccountService(httpClient, accountsEndpointsOptions);

        return sut;
    }
}