using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Services.ComplianceScheme;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using Moq;
using System.Net;
using System.Security.Claims;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ComplianceSchemesControllerTests
{
    private static readonly Guid Oid = Guid.NewGuid();
    private RemoveComplianceSchemeModel _removeComplianceSchemeModel = null!;
    private readonly Guid _selectedSchemeId = Guid.NewGuid();
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
        _sut = new ComplianceSchemesController(_mockComplianceSchemeServiceMock.Object, _nullLogger, _messageService.Object, _featureManager.Object);
        _sut.AddDefaultContextWithOid(Oid, "TestAuth");
        _removeComplianceSchemeModel = new RemoveComplianceSchemeModel{ SelectedSchemeId = _selectedSchemeId, UserOId= Oid};
    }
    
    [TestMethod]
    public async Task ShouldReturn_Ok_WhenComplianceSchemesExist()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetAllComplianceSchemesAsync())
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetAllComplianceSchemes();

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }

    [TestMethod]
    public async Task ShouldReturn_InternalServiceError_WhenNoComplianceSchemesFound()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.InternalServerError)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetAllComplianceSchemesAsync())
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetAllComplianceSchemes() as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }

    private void RemoveOidSetup()
    {
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [TestMethod]
    public async Task ShouldReturn_Ok_WhenComplianceSchemeExistsForProducer()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeForProducerAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetComplianceSchemeForProducer(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }
    
    [TestMethod]
    public async Task ShouldReturn_NotFound_WhenComplianceSchemeDoesNotExistForProducer()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.NotFound)
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeForProducerAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetComplianceSchemeForProducer(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    
    [TestMethod]
    public async Task ShouldReturn_InternalServiceError_WhenNoComplianceSchemeExistsForProducer()
    {
        // Arrange
        RemoveOidSetup();

        // Act
        var result = await _sut.GetComplianceSchemeForProducer(Guid.NewGuid()) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_remove_selected_compliance_scheme_when_id_is_valid()
    {
        // Arrange
        var request = new RemoveComplianceSchemeModel();
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceScheme(It.Is<RemoveComplianceSchemeModel>(pc => pc == _removeComplianceSchemeModel)))
            .ReturnsAsync(handlerResponse);

        // Act

        var result = await _sut.Remove(request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }
    
    [TestMethod]
    public async Task Should_return_internal_server_error_when_removing_compliance_scheme_throws_exception()
    {
        // Arrange.
        var request = new RemoveComplianceSchemeModel();
        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceScheme(It.Is<RemoveComplianceSchemeModel>(pc => pc == _removeComplianceSchemeModel)))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _sut.Remove(request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_return_notfound_when_removing_compliance_scheme_returns_notfound()
    {
        // Arrange
        var request = new RemoveComplianceSchemeModel();
        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceScheme(It.Is<RemoveComplianceSchemeModel>(pc => pc == _removeComplianceSchemeModel)))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.NotFound));

        // Act
        var result = await _sut.Remove(request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as NotFoundResult;
        statusCodeResult?.StatusCode.Should().Be(404);
    }
    
    [TestMethod]
    public async Task Should_return_bad_request_when_removing_compliance_scheme_returns_bad_request()
    {
        // Arrange
        var request = new RemoveComplianceSchemeModel();
        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceScheme(It.Is<RemoveComplianceSchemeModel>(pc => pc == _removeComplianceSchemeModel)))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.BadRequest));

        // Act
        var result = await _sut.Remove(request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as BadRequestResult;
        statusCodeResult?.StatusCode.Should().Be(400);
    }
    
    [TestMethod]
    public async Task Should_return_statuscode_500_when_removing_compliance_scheme_returns_500()
    {
        // Arrange
        var request = new RemoveComplianceSchemeModel();
        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceScheme(It.Is<RemoveComplianceSchemeModel>(pc => pc == _removeComplianceSchemeModel)))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.InternalServerError));

        // Act
        var result = await _sut.Remove(request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task Should_return_ok_when_success_response_is_returned_from_select_compliance_scheme()
    {
        // Arrange
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();
        
        var model = _fixture.Create<SelectSchemeModel>();
        
        _mockComplianceSchemeServiceMock
            .Setup(x => x.SelectComplianceSchemeAsync(
                It.Is<SelectSchemeWithUserModel>(m => 
                    m.ComplianceSchemeId == model.ComplianceSchemeId && 
                    m.UserOId == Oid &&
                    m.ProducerOrganisationId == model.OrganisationId)))
            .ReturnsAsync(handlerResponse);
    
        // Act
        var result = await _sut.SelectComplianceScheme(model);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }

    [TestMethod]
    public async Task Should_return_internal_server_error_when_oid_not_found_in_select_scheme()
    {
        // Arrange
        RemoveOidSetup();
        
        var model = _fixture.Create<SelectSchemeModel>();
        
        // Act
        var result = await _sut.SelectComplianceScheme(model) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_return_internal_server_error_when_select_scheme_throws()
    {
        // Arrange
        var model = _fixture.Create<SelectSchemeModel>();
        
        _mockComplianceSchemeServiceMock
            .Setup(x => x.SelectComplianceSchemeAsync(It.IsAny<SelectSchemeWithUserModel>()))
            .ThrowsAsync(_fixture.Create<Exception>());
        
        // Act
        var result = await _sut.SelectComplianceScheme(model) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task Should_map_400_403_404_500_when_select_scheme_returns_codes(HttpStatusCode statusCode)
    {
        // Arrange
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, statusCode)
                .Create();
        
        _mockComplianceSchemeServiceMock
            .Setup(x => x.SelectComplianceSchemeAsync(It.IsAny<SelectSchemeWithUserModel>()))
            .ReturnsAsync(handlerResponse);
        
        var model = _fixture.Create<SelectSchemeModel>();
        
        // Act
        var result = await _sut.SelectComplianceScheme(model) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)statusCode);
    }
    
    [TestMethod]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.Ambiguous)]
    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.NotImplemented)]
    public async Task Should_return_internal_server_error_when_select_scheme_returns_other_non_successful_response(HttpStatusCode statusCode)
    {
        // Arrange
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, statusCode)
                .Create();
        
        _mockComplianceSchemeServiceMock
            .Setup(x => x.SelectComplianceSchemeAsync(It.IsAny<SelectSchemeWithUserModel>()))
            .ReturnsAsync(handlerResponse);
        
        var model = _fixture.Create<SelectSchemeModel>();
        
        // Act
        var result = await _sut.SelectComplianceScheme(model) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_return_ok_when_success_response_is_returned_from_update_compliance_scheme()
    {
        // Arrange
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();
        
        var model = _fixture.Create<UpdateSchemeModel>();
        
        _mockComplianceSchemeServiceMock
            .Setup(x => x.UpdateComplianceSchemeAsync(
                It.Is<UpdateSchemeWithUserModel>(m => 
                    m.ComplianceSchemeId == model.ComplianceSchemeId && 
                    m.UserOId == Oid &&
                    m.ProducerOrganisationId == model.OrganisationId)))
            .ReturnsAsync(handlerResponse);
    
        // Act
        var result = await _sut.UpdateComplianceScheme(model);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }

    [TestMethod]
    public async Task Should_return_internal_server_error_when_oid_not_found_in_update_scheme()
    {
        // Arrange
        RemoveOidSetup();
        
        var model = _fixture.Create<UpdateSchemeModel>();
        
        // Act
        var result = await _sut.UpdateComplianceScheme(model) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_return_internal_server_error_when_select_update_throws()
    {
        _mockComplianceSchemeServiceMock
            .Setup(x => x.UpdateComplianceSchemeAsync(It.IsAny<UpdateSchemeWithUserModel>()))
            .ThrowsAsync(_fixture.Create<Exception>());
        
        var model = _fixture.Create<UpdateSchemeModel>();
        
        // Act
        var result = await _sut.UpdateComplianceScheme(model) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task Should_map_400_403_404_500_when_update_scheme_returns_codes(HttpStatusCode statusCode)
    {
        // Arrange
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, statusCode)
                .Create();
        
        _mockComplianceSchemeServiceMock
            .Setup(x => x.UpdateComplianceSchemeAsync(It.IsAny<UpdateSchemeWithUserModel>()))
            .ReturnsAsync(handlerResponse);
        
        var model = _fixture.Create<UpdateSchemeModel>();
        
        // Act
        var result = await _sut.UpdateComplianceScheme(model) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)statusCode);
    }
    
    [TestMethod]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.Ambiguous)]
    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.NotImplemented)]
    public async Task Should_return_internal_server_error_when_update_scheme_returns_other_non_successful_response(HttpStatusCode statusCode)
    {
        // Arrange
        var handlerResponse = 
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, statusCode)
                .Create();
        
        _mockComplianceSchemeServiceMock
            .Setup(x => x.UpdateComplianceSchemeAsync(It.IsAny<UpdateSchemeWithUserModel>()))
            .ReturnsAsync(handlerResponse);
        
        var model = _fixture.Create<UpdateSchemeModel>();
        
        // Act
        var result = await _sut.UpdateComplianceScheme(model) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }

       
    [TestMethod]
    public async Task ShouldReturn_Ok_WhenComplianceSchemesExistForOperator()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemesForOperatorAsync(It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetOperatorComplianceSchemes(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }
    
    [TestMethod]
    public async Task ShouldReturn_InternalServiceError_WhenNoComplianceSchemeListExistsForOperator()
    {
        // Arrange
        RemoveOidSetup();

        // Act
        var result = await _sut.GetOperatorComplianceSchemes(Guid.NewGuid()) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task ShouldReturn_error_WhenComplianceSchemesThrowsException()
    {
        // Arrange
        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetAllComplianceSchemesAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _sut.GetAllComplianceSchemes();

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as StatusCodeResult;
        obj.StatusCode.Should().Be(500);

    }
    
    [TestMethod]
    public async Task ShouldReturn_Error_WhenComplianceSchemesDoesNotExistForOperator()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.NotFound)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemesForOperatorAsync(It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetOperatorComplianceSchemes(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        var obj = result as NotFoundResult;
        obj.StatusCode.Should().Be(404);
    }
    
    [TestMethod]
    public async Task ShouldReturn_BadRequest_WhenComplianceSchemeDoesNotExistsForProducer()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.BadRequest)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeForProducerAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetComplianceSchemeForProducer(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        var obj = result as BadRequestResult;
        obj.StatusCode.Should().Be(400);
    }
    
    [TestMethod]
    public async Task Should_throw_500_error_when_no_userId()
    {
        // Arrange
        var request = new RemoveComplianceSchemeModel();
        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("emails", "_userEmail"),
            new(ClaimConstants.ObjectId, Guid.Empty.ToString()),
        }.AsEnumerable());
        _sut.ControllerContext.HttpContext = _httpContextMock.Object;

        // Act

        var result = await _sut.Remove(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var obj = result as ObjectResult;
        obj?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task Should_return_ok_when_remove_selected_compliance_scheme_when_id_is_valid()
    {
        // Arrange
        var request = new RemoveComplianceSchemeModel();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceScheme(It.IsAny<RemoveComplianceSchemeModel>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act

        var result = await _sut.Remove(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(200);
    }
    
    [TestMethod]
    public async Task Should_return_not_when_remove_selected_compliance_scheme_when_id_is_not_found()
    {
        // Arrange
        var request = new RemoveComplianceSchemeModel();
        

        _mockComplianceSchemeServiceMock
            .Setup(x => x.RemoveComplianceScheme(It.IsAny<RemoveComplianceSchemeModel>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act

        var result = await _sut.Remove(request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        var obj = result as NotFoundResult;
        obj.StatusCode.Should().Be(404);
    }
    
    [TestMethod]
    public async Task SelectComplainScheme_Should_return_500_when_user_is_empty()
    {
        // Arrange
        var model = _fixture.Create<SelectSchemeModel>();
        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("emails", "_userEmail"),
            new(ClaimConstants.ObjectId, Guid.Empty.ToString()),
        }.AsEnumerable());
        _sut.ControllerContext.HttpContext = _httpContextMock.Object;
        
        // Act
        var result = await _sut.SelectComplianceScheme(model);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var obj = result as ObjectResult;
        obj?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task UpdateComplianceScheme_Should_return_500_when_user_is_empty()
    {
        // Arrange
        var model = _fixture.Create<UpdateSchemeModel>();
        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("emails", "_userEmail"),
            new(ClaimConstants.ObjectId, Guid.Empty.ToString()),
        }.AsEnumerable());
        _sut.ControllerContext.HttpContext = _httpContextMock.Object;
        
        // Act
        var result = await _sut.UpdateComplianceScheme(model);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var obj = result as ObjectResult;
        obj?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetailsAsync_WhenMemberDetailsExistsForUser_ReturnOkResult()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeMemberDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetComplianceSchemeMemberDetails(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetailsAsync_WhenMemberDetailsEndpointIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.BadRequest)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeMemberDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetComplianceSchemeMemberDetails(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        var obj = result as BadRequestResult;
        obj?.StatusCode.Should().Be(400);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetailsAsync_WhenMemberDetailsNotFoundForUser_ReturnsNotFound()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.NotFound)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeMemberDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetComplianceSchemeMemberDetails(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        var obj = result as NotFoundResult;
        obj?.StatusCode.Should().Be(404);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetailsAsync_WhenUserIsNotAuthorised_ReturnsForbidden()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.Forbidden)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemeMemberDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetComplianceSchemeMemberDetails(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        (result as StatusCodeResult).StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    public async Task GetComplianceSchemesSummaries_WhenServiceThrows_ThenControllerReturnsErrorStatusCode500()
    {
        // Arrange
        var complianceSchemeId = Guid.NewGuid();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemesSummary(complianceSchemeId, It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(_fixture.Create<Exception>());

        // Act
        var result = await _sut.GetComplianceSchemeSummary(complianceSchemeId, Guid.NewGuid()) as StatusCodeResult;

        // Assert
        result.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task GetComplianceSchemesSummaries_WhenServiceRequestIsSuccessful_ThenControllerReturnsData()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var complianceSchemeId = Guid.NewGuid();

        var complianceSchemeSummary = new ComplianceSchemeSummary
        {
            Name = "Compliance Scheme Name",
            Nation = Nation.England,
            CreatedOn = DateTimeOffset.Now,
            MemberCount = 123,
            MembersLastUpdatedOn = DateTimeOffset.Now
        };

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetComplianceSchemesSummary(complianceSchemeId, organisationId, It.IsAny<Guid>()))
            .ReturnsAsync(complianceSchemeSummary);

        // Act
        var result = await _sut.GetComplianceSchemeSummary(complianceSchemeId, organisationId) as OkObjectResult;

        // Assert
        result.StatusCode.Should().Be(200);

        var resultValue = result.Value as ComplianceSchemeSummary;

        resultValue.Should().BeEquivalentTo(complianceSchemeSummary);
    }
    [TestMethod]
    public async Task GetAllReasonsForRemoval_WhenReasonsForRemovalExist_ReturnsOk()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetAllReasonsForRemovalsAsync())
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetAllReasonsForRemoval();

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }

    [TestMethod]
    public async Task GetAllReasonsForRemoval_WhenFetchingIsFailed_ReturnInternalServiceError()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.InternalServerError)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockComplianceSchemeServiceMock
            .Setup(x => x.GetAllReasonsForRemovalsAsync())
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetAllReasonsForRemoval() as StatusCodeResult;

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        result?.StatusCode.Should().Be(500);
    }
}
