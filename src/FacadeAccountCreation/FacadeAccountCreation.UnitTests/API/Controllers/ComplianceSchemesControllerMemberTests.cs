using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Helpers;
using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Services.ComplianceScheme;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ComplianceSchemesControllerMemberTests
{
    private static readonly Guid Oid = Guid.NewGuid();
    private readonly Mock<IComplianceSchemeService> _mockComplianceSchemeServiceMock = new();
    private readonly NullLogger<ComplianceSchemesController> _nullLogger = new();
    private readonly Mock<IMessagingService> _messageService = new();
    private readonly Mock<IFeatureManager> _featureManager = new();
    private ComplianceSchemesController _sut = null!;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Mock<HttpContext>? _httpContextMock;

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _sut = new ComplianceSchemesController(_mockComplianceSchemeServiceMock.Object, _nullLogger,_messageService.Object, _featureManager.Object);
        _sut.AddDefaultContextWithOid(Oid, "TestAuth");
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ShouldReturnOk_WhenComplianceSchemeExists()
    {
        var serviceResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
        .With(x => x.Content, new StringContent(_fixture.Create<string>()))
        .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeMembersAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(serviceResponse);

        var result = await _sut.GetComplianceSchemeMembers(Guid.NewGuid(), Guid.NewGuid(), 10, "", 1);

        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(serviceResponse.Content);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ShouldReturnNotFound_WhenComplianceSchemeDoesNotExists()
    {
        var serviceResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.NotFound).Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeMembersAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(serviceResponse);

        var result = await _sut.GetComplianceSchemeMembers(Guid.NewGuid(), Guid.NewGuid(), 10, "", 1);

        result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ShouldReturnInternalServerErrorStatus_WhenOidNotFound()
    {
        var sut = new ComplianceSchemesController(_mockComplianceSchemeServiceMock.Object, _nullLogger,_messageService.Object, _featureManager.Object);

        var result = await sut.GetComplianceSchemeMembers(Guid.NewGuid(), Guid.NewGuid(), 10, "", 1);

        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ShouldReturnInternalServerErrorStatus_WhenServiceArguementException()
    {
        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeMembersAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Throws<ArgumentException>();

        var result = await _sut.GetComplianceSchemeMembers(Guid.NewGuid(), Guid.NewGuid(), 10, "", 1);

        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ShouldReturnBadRequestStatus_WhenQueryTooLong()
    {
        string longString = new string('*', 300);
        var result = await _sut.GetComplianceSchemeMembers(Guid.NewGuid(), Guid.NewGuid(), 10, longString, 1);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ShouldReturnInternalServerErrorStatus_WhenUserIdEmpty()
    {
        _sut.AddDefaultContextWithOid(Guid.Empty, "TestAuth");

        var result = await _sut.GetComplianceSchemeMembers(Guid.NewGuid(), Guid.NewGuid(), 10, "", 1);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_ShouldReturnOk_WhenRequestValid()
    {
        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetInfoForSelectedSchemeRemoval(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(_fixture.Build<InfoForSelectedSchemeRemovalResponse>().Create());

        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceSchemeMember(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<RemoveComplianceSchemeMemberModel>()))
            .ReturnsAsync(_fixture.Build<RemoveComplianceSchemeMemberResponse>().Create());

        var result = await _sut.RemoveComplianceSchemeMember(Guid.NewGuid(), Guid.NewGuid(), It.IsAny<RemoveComplianceSchemeMemberModel>());

        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_ShouldReturnError_WhenResponseNull()
    {
        var serviceResponse = (RemoveComplianceSchemeMemberResponse?)null;

        _mockComplianceSchemeServiceMock
            .Setup(x => 
                x.RemoveComplianceSchemeMember(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<RemoveComplianceSchemeMemberModel>()))
            .ReturnsAsync(serviceResponse);

        var result = await _sut.RemoveComplianceSchemeMember(Guid.NewGuid(), Guid.NewGuid(), It.IsAny<RemoveComplianceSchemeMemberModel>());

        result.Should().BeOfType<BadRequestResult>();
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_ShouldReturnInternalServerErrorStatus_WhenOidNotFound()
    {
        var sut = new ComplianceSchemesController(_mockComplianceSchemeServiceMock.Object, _nullLogger, _messageService.Object, _featureManager.Object);

        var result = await sut.RemoveComplianceSchemeMember(Guid.NewGuid(), Guid.NewGuid(), It.IsAny<RemoveComplianceSchemeMemberModel>());

        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
    
    [TestMethod]
    [DataRow(true, 1)]
    [DataRow(false, 0)]
    public async Task RemoveComplianceSchemeMemberAsync_ShouldSendEmailNotification_WhenRequestFeatureFlagEnabled(Boolean flagEnabled, int timesCalled)
    {
        _featureManager.Setup(x => x.IsEnabledAsync("SendDissociationNotificationEmail"))
            .Returns(Task.FromResult(flagEnabled));
        
        var removeResponse =
            _fixture
                .Build<RemoveComplianceSchemeMemberResponse>()
                .Create();
        var removalInfoResponse =
            _fixture
                .Build<InfoForSelectedSchemeRemovalResponse>()
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceSchemeMember(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<RemoveComplianceSchemeMemberModel>()))
            .ReturnsAsync(removeResponse);
        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetInfoForSelectedSchemeRemoval(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(removalInfoResponse);

        var result = await _sut.RemoveComplianceSchemeMember(Guid.NewGuid(), Guid.NewGuid(), It.IsAny<RemoveComplianceSchemeMemberModel>());

        _messageService.Verify(x => x.SendMemberDissociationRegulatorsNotification(It.IsAny<MemberDissociationRegulatorsEmailInput>()), Times.Exactly(timesCalled));       
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [DataRow(true, 1)]
    [DataRow(false, 0)]
    public async Task RemoveComplianceSchemeMemberAsync_ShouldSendEmailNotification_WhenRequestFeatureFlagEnabledForProducerEmail(Boolean flagEnabled, int timesCalled)
    {
        _featureManager.Setup(x => x.IsEnabledAsync("SendDissociationNotificationEmail"))
            .Returns(Task.FromResult(flagEnabled));

        var removeResponse =
            _fixture
                .Build<RemoveComplianceSchemeMemberResponse>()
                .Create();
        var removalInfoResponse =
            _fixture
                .Build<InfoForSelectedSchemeRemovalResponse>()
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceSchemeMember(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<RemoveComplianceSchemeMemberModel>()))
            .ReturnsAsync(removeResponse);
        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetInfoForSelectedSchemeRemoval(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(removalInfoResponse);

        var result = await _sut.RemoveComplianceSchemeMember(Guid.NewGuid(), Guid.NewGuid(), It.IsAny<RemoveComplianceSchemeMemberModel>());        
        _messageService.Verify(x => x.SendMemberDissociationProducersNotification(It.IsAny<NotifyComplianceSchemeProducerEmailInput>()), Times.Exactly(timesCalled));
        result.Should().BeOfType<OkObjectResult>();
    }

}
