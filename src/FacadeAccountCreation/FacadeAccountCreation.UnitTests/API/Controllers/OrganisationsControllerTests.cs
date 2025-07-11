using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;
using FacadeAccountCreation.Core.Models.Subsidiary;
using FacadeAccountCreation.Core.Models.User;
using FacadeAccountCreation.Core.Services.Organisation;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class OrganisationsControllerTests
{
    private readonly Guid _oid = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private const int ServiceRoleId = 1;
    private readonly NullLogger<OrganisationsController> _nullLogger = new();
    private readonly Mock<IOrganisationService> _mockOrganisationService = new();
    private readonly Mock<IServiceRolesLookupService> _serviceRolesLookupServiceMock = new();
    private OrganisationsController _sut = null!;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Mock<HttpContext>? _httpContextMock;

    private const string Token = "test token";

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _sut = new OrganisationsController(_nullLogger, _mockOrganisationService.Object, _serviceRolesLookupServiceMock.Object);
        _sut.AddDefaultContextWithOid(_oid, "TestAuth");
    }

    [TestMethod]
    public async Task Should_return_statusCode_404_when_GetOrganisationUsersList_throws_notFound()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
                x.GetOrganisationUserList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.NotFound));

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task Should_return_statusCode_500_when_GetOrganisationUsersList_throws_500()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.GetOrganisationUserList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.InternalServerError));

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as BadRequestResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task Should_return_organisation_user_list_if_no_exceptions()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockOrganisationService.Setup(x =>
            x.GetOrganisationUserList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }

    [TestMethod]
    public async Task Should_return_statusCode_200_when_Success()
    {
        // Arrange
        var apiResponse = _fixture.Create<OrganisationUser>();
        _mockOrganisationService.Setup(x =>
                x.GetOrganisationUserList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(apiResponse))
                });

        _serviceRolesLookupServiceMock.Setup(x => x.GetServiceRoles()).Returns([]);

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as OkObjectResult;
        statusCodeResult?.StatusCode.Should().Be(200);
    }

    [TestMethod]
    public async Task Should_return_500_statusCode_when_empty_user()
    {
        // Arrange
        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("emails", "_userEmail"),
            new(ClaimConstants.ObjectId, Guid.Empty.ToString()),
        }.AsEnumerable());
        _sut.ControllerContext.HttpContext = _httpContextMock.Object;

        // Act
        var result = await _sut.GetOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var statusCodeResult = result as ObjectResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task GetAllOrganisationUsers_should_return_statusCode_404_when_GetOrganisationAllUsersList_throws_notFound()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
                x.GetOrganisationAllUsersList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.NotFound));

        // Act
        var result = await _sut.GetAllOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task GetAllOrganisationUsers_should_return_statusCode_500_when_GetOrganisationAllUsersList_throws_500()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.GetOrganisationAllUsersList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.InternalServerError));

        // Act
        var result = await _sut.GetAllOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as BadRequestResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task GetOrganisationAllUsersList_should_return_organisation_user_list_if_no_exceptions()
    {
        // Arrange
        var handlerResponse =
            _fixture
                .Build<HttpResponseMessage>()
                .With(x => x.StatusCode, HttpStatusCode.OK)
                .With(x => x.Content, new StringContent(_fixture.Create<string>()))
                .Create();

        _mockOrganisationService.Setup(x =>
            x.GetOrganisationAllUsersList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.GetAllOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var obj = result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse.Content);
    }

    [TestMethod]
    public async Task GetOrganisationAllUsersList_should_return_statusCode_200_when_Success()
    {
        // Arrange
        var apiResponse = _fixture.Create<OrganisationUser>();
        _mockOrganisationService.Setup(x =>
                x.GetOrganisationAllUsersList(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(apiResponse))
                });

        _serviceRolesLookupServiceMock.Setup(x => x.GetServiceRoles()).Returns([]);

        // Act
        var result = await _sut.GetAllOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as OkObjectResult;
        statusCodeResult?.StatusCode.Should().Be(200);
    }

    [TestMethod]
    public async Task GetAllOrganisationUsers_should_return_500_statusCode_when_empty_user()
    {
        // Arrange
        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("emails", "_userEmail"),
            new(ClaimConstants.ObjectId, Guid.Empty.ToString()),
        }.AsEnumerable());
        _sut.ControllerContext.HttpContext = _httpContextMock.Object;

        // Act
        var result = await _sut.GetAllOrganisationUsers(_userId, ServiceRoleId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var statusCodeResult = result as ObjectResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task Should_Return_StatusCode_200_When_GetNationIdByOrganisationId_Succeeds()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expectedContent = "[1,2]";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };
        _mockOrganisationService.Setup(x => x.GetNationIdByOrganisationId(organisationId)).ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.GetNationIdByOrganisationId(organisationId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var resultValue = (result as OkObjectResult).Value as List<int>;
        resultValue.Should().Contain(1);
        resultValue.Should().Contain(2);
    }

    [TestMethod]
    public async Task Should_Return_StatusCode_400_When_GetNationIdByOrganisationId_Returns_BadRequest()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        _mockOrganisationService.Setup(x => x.GetNationIdByOrganisationId(organisationId)).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        // Act
        var result = await _sut.GetNationIdByOrganisationId(organisationId);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }

    [TestMethod]
    public async Task Should_Return_StatusCode_500_When_GetNationIdByOrganisationId_Returns_InternalServerError()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        _mockOrganisationService.Setup(x => x.GetNationIdByOrganisationId(organisationId)).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        var result = await _sut.GetNationIdByOrganisationId(organisationId);

        // Assert
        var statusCodeResult = result as StatusCodeResult;
        result.Should().BeOfType<StatusCodeResult>();
        statusCodeResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetOrganisationNameByInviteToken_Should_return_Success()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.GetOrganisationNameByInviteToken(It.IsAny<string>())).ReturnsAsync(new ApprovedPersonOrganisationModel
            {
                SubBuildingName = "",
                BuildingName = "",
                BuildingNumber = "",
                Street = "",
                Town = "",
                County = "",
                Postcode = "",
                Locality = "",
                DependentLocality = "",
                Country = "United Kingdom",
                IsUkAddress = true,
                OrganisationName = "testOrganisation",
                ApprovedUserEmail = "adas@sdad.com"
            });

        _serviceRolesLookupServiceMock.Setup(x => x.GetServiceRoles()).Returns([]);

        // Act
        var result = await _sut.GetOrganisationNameByInviteToken(Token);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task CreateAndAddSubsidiary_Should_return_Success()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.CreateAndAddSubsidiaryAsync(It.IsAny<LinkOrganisationModel>())).ReturnsAsync("Ref123456");

        // Act
        var result = await _sut.CreateAndAddSubsidiary(new LinkOrganisationModel()) as OkObjectResult;

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Value.Should().Be("Ref123456");
    }

    [TestMethod]
    public async Task CreateAndAddSubsidiary_WhenResult_Null_Should_return_500()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.CreateAndAddSubsidiaryAsync(It.IsAny<LinkOrganisationModel>())).ReturnsAsync((string)null);

        // Act
        var result = await _sut.CreateAndAddSubsidiary(new LinkOrganisationModel()) as ObjectResult;

        var resultValue = result.Value as ProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        resultValue.Detail.Should().Be("Failed to create and add organisation");
    }

    [TestMethod]
    public async Task AddSubsidiary_Should_return_Success()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.AddSubsidiaryAsync(It.IsAny<SubsidiaryAddModel>())).ReturnsAsync("Ref123456");

        // Act
        var result = await _sut.AddSubsidiary(new SubsidiaryAddModel()) as OkObjectResult;

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task TerminateSubsidiary_Should_return_Success()
    {
        // Arrange
        // Act
        var result = await _sut.TerminateSubsidiary(new SubsidiaryTerminateModel());

        // Assert
        result.Should().BeOfType<OkResult>();
    }


    [TestMethod]
    public async Task TerminateSubsidiary_Should_return_SuccessTest()
    {
        // Arrange
        // Act
        var result = await _sut.TerminateSubsidiary(new SubsidiaryTerminateModel()) as StatusCodeResult;

        // Assert
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TerminateSubsidiary_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.TerminateSubsidiaryAsync(It.IsAny<SubsidiaryTerminateModel>())).ThrowsAsync(new ProblemResponseException());

        // Act
        var result = await _sut.TerminateSubsidiary(new SubsidiaryTerminateModel()) as StatusCodeResult;

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task AddSubsidiary_WhenResult_Null_Should_return_500()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.AddSubsidiaryAsync(It.IsAny<SubsidiaryAddModel>())).ReturnsAsync((string)null);

        // Act
        var result = await _sut.AddSubsidiary(new SubsidiaryAddModel()) as ObjectResult;

        var resultValue = result.Value as ProblemDetails;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        resultValue.Detail.Should().Be("Failed to add subsidiary");
    }

    [TestMethod]
    public async Task GetPagedOrganisationRelationshipsAsync_ValidInputWithData_ReturnsOkResult()
    {
        // Arrange
        var page = 1;
        var showPerPage = 20;
        var search = "test";

        var mockResponse = new PagedOrganisationRelationshipsModel
        {
            CurrentPage = 1,
            TotalItems = 1,
            PageSize = 20,
            Items = new List<RelationshipResponseModel>
            {
                new RelationshipResponseModel
                {
                    OrganisationName = "Test1",
                    OrganisationNumber = "2345",
                    RelationshipType = "Parent",
                    CompaniesHouseNumber = "CH123455"
                }
            },
            SearchTerms = new List<string>()
        };

        _mockOrganisationService
            .Setup(service => service.GetPagedOrganisationRelationships(page, showPerPage, search))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _sut.GetPagedOrganisationRelationshipsAsync(page, showPerPage, search);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(mockResponse, okResult.Value);
    }

    [TestMethod]
    public async Task GetUnpagedOrganisationRelationshipsAsync_ValidInputWithData_ReturnsOkResult()
    {
        // Arrange
        var mockResponse = new List<RelationshipResponseModel>
            {
                new RelationshipResponseModel
                {
                    OrganisationName = "Organisation 1",
                    OrganisationNumber = "12",
                    CompaniesHouseNumber = "CH123455"
                },

                new RelationshipResponseModel
                {
                    OrganisationName = "Organisation 2",
                    OrganisationNumber = "34",
                    CompaniesHouseNumber = "CH123455"
                },


                new RelationshipResponseModel
                {
                    OrganisationName = "Organisation 3",
                    OrganisationNumber = "56",
                    CompaniesHouseNumber = "CH123455"
                }
            };

        _mockOrganisationService
            .Setup(service => service.GetUnpagedOrganisationRelationships())
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _sut.GetUnpagedOrganisationRelationshipsAsync();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(mockResponse, okResult.Value);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationIdAsync_ValidInputWithData_ReturnsOkResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var mockResponse = new OrganisationRelationshipModel { Organisation = new OrganisationDetailModel { OrganisationNumber = "12345", OrganisationType = "Producer" }, Relationships =
            [
                new RelationshipResponseModel
                {
                    OrganisationName = "Test1", OrganisationNumber = "2345", RelationshipType = "Parent",
                    CompaniesHouseNumber = "CH123455"
                }
            ]
        };

        _mockOrganisationService
            .Setup(service => service.GetOrganisationRelationshipsByOrganisationId(organisationId))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _sut.GetOrganisationRelationshipsByOrganisationIdAsync(organisationId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(mockResponse, okResult.Value);
    }


    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationIdAsync_ValidInputWithData_ReturnsOkResultWithJoinerDateAndReportingType()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var mockResponse = new OrganisationRelationshipModel
        {
            Organisation = new OrganisationDetailModel { OrganisationNumber = "12345", OrganisationType = "Producer" },
            Relationships =
            [
                new RelationshipResponseModel
                {
                    OrganisationName = "Test1", OrganisationNumber = "2345", RelationshipType = "Parent",
                    CompaniesHouseNumber = "CH123455", JoinerDate =  new DateTime(2024, 12, 17), ReportingType = "Self" 
                }
            ]
        };

        _mockOrganisationService
            .Setup(service => service.GetOrganisationRelationshipsByOrganisationId(organisationId))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _sut.GetOrganisationRelationshipsByOrganisationIdAsync(organisationId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(mockResponse, okResult.Value);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationIdAsync_ValidInputWithNoData_ReturnsNoContentResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();

        _mockOrganisationService
            .Setup(service => service.GetOrganisationRelationshipsByOrganisationId(organisationId))
            .ReturnsAsync((OrganisationRelationshipModel)null);

        // Act
        var result = await _sut.GetOrganisationRelationshipsByOrganisationIdAsync(organisationId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task GetExportOrganisationSubsidiariesAsync_ValidInputWithData_ReturnsOkResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var mockResponse = new List<ExportOrganisationSubsidiariesResponseModel>
        {
            new() { OrganisationId = "1", SubsidiaryId = null, OrganisationName = "ABC", CompaniesHouseNumber = "CH1", JoinerDate = null, ReportingType = null },
            new() { OrganisationId = "1", SubsidiaryId = "2", OrganisationName = "ABC", CompaniesHouseNumber = "CH2", JoinerDate = DateTime.Parse("2025-02-01", CultureInfo.InvariantCulture), ReportingType = "Individual" }
        };

        _mockOrganisationService
            .Setup(service => service.ExportOrganisationSubsidiaries(organisationId))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _sut.GetExportOrganisationSubsidiariesAsync(organisationId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(mockResponse, okResult.Value);
    }

    [TestMethod]
    public async Task GetExportOrganisationSubsidiariesAsync_ValidInputWithNoData_ReturnsNoContentResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        List<ExportOrganisationSubsidiariesResponseModel> mockResponse = null;

        _mockOrganisationService
            .Setup(service => service.ExportOrganisationSubsidiaries(organisationId))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _sut.GetExportOrganisationSubsidiariesAsync(organisationId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }
    [TestMethod]
    public async Task UpdateOrganisationDetails_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var nationId = new OrganisationUpdateDto();

        // Act
        var result = await _sut.UpdateOrganisationDetails(
            organisationId,
            nationId) as OkResult;

        // Assert
        Assert.IsNotNull(result);
        _mockOrganisationService.Verify(s =>
            s.UpdateOrganisationDetails(
                It.IsAny<Guid>(),
                organisationId,
                nationId),
            Times.Once);
    }

    [TestMethod]
    public async Task UpdateOrganisationDetails_WithNoNationId_ReturnsBadRequestResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();

        // Act
        var result = await _sut.UpdateOrganisationDetails(
            organisationId,
            null) as BadRequestResult;

        // Assert
        Assert.IsNotNull(result);
        _mockOrganisationService.Verify(s =>
            s.UpdateOrganisationDetails(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<OrganisationUpdateDto>()),
            Times.Never);
    }

    [TestMethod]
    public async Task UpdateOrganisationDetails_ThrowsException_InternalServerError()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var organisation = new OrganisationUpdateDto();
        _mockOrganisationService.Setup(s => s.UpdateOrganisationDetails(
            It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<OrganisationUpdateDto>()
            )).Throws(new Exception());

        // Act
        var result = await _sut.UpdateOrganisationDetails(
            organisationId,
            organisation) as StatusCodeResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
        _mockOrganisationService.Verify(s =>
            s.UpdateOrganisationDetails(
                It.IsAny<Guid>(),
                organisationId,
                organisation),
            Times.Once);
    }

    [TestMethod]
    public async Task GetOrganisationByReferenceNumber_OrganisationNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        const string referenceNumber = "ref:123456";

        _mockOrganisationService
            .Setup(service => service.GetOrganisationByReferenceNumber(referenceNumber))
            .ReturnsAsync((OrganisationDto)null);

        // Act
        var result = await _sut.GetOrganisationByReferenceNumber(referenceNumber);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        var response = result as NotFoundResult;
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

    }
    
    [TestMethod]
    public async Task GetOrganisationByReferenceNumber_OrganisationIsFound_ReturnsOkObjectResult()
    {
        // Arrange
        const string referenceNumber = "ref:123456";
        const int organisationId = 1234;
        var expectedOrganisation = new OrganisationDto { Id = organisationId, RegistrationNumber = referenceNumber };

        _mockOrganisationService
            .Setup(service => service.GetOrganisationByReferenceNumber(referenceNumber))
            .ReturnsAsync(expectedOrganisation);

        // Act
        var result = await _sut.GetOrganisationByReferenceNumber(referenceNumber);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var response = result as OkObjectResult;
        var output = response.Value as OrganisationDto;
        output.Should().BeEquivalentTo(expectedOrganisation);
    }

    [TestMethod]
    public async Task GetRegulatorNation_When_Api_Returns_200_SuccessResult()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.GetOrganisationNationCodeByExternalIdAsync(It.IsAny<Guid>())).ReturnsAsync("GB-ENG");

        // Act
        var result = await _sut.GetRegulatorNation(Guid.NewGuid());
        var resultValue = (result as OkObjectResult).Value as string;
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        resultValue.Should().Be("GB-ENG");

        _mockOrganisationService.Verify(s =>
             s.GetOrganisationNationCodeByExternalIdAsync(
                 It.IsAny<Guid>()),
             Times.Once);
    }

    [TestMethod]
    public async Task GetRegulatorNation_When_Api_Returns_404_NotFoundResult()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.GetOrganisationNationCodeByExternalIdAsync(It.IsAny<Guid>())).ReturnsAsync((string)null);

        // Act
        var result = await _sut.GetRegulatorNation(Guid.NewGuid()) as NotFoundObjectResult;

        // Assert
        _mockOrganisationService.Verify(s =>
             s.GetOrganisationNationCodeByExternalIdAsync(
                 It.IsAny<Guid>()),
             Times.Once);

        result.Should().BeOfType<NotFoundObjectResult>();
        result?.Value.Should().Be("Organisation not found");
        result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetRegulatorNation_When_Api_Returns_500_InternalServerError()
    {
        // Arrange
        _mockOrganisationService.Setup(x =>
            x.GetOrganisationNationCodeByExternalIdAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception());

        // Act
        var result = await _sut.GetRegulatorNation(Guid.NewGuid()) as StatusCodeResult;

        // Assert
        result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        _mockOrganisationService.Verify(s =>
            s.GetOrganisationNationCodeByExternalIdAsync(
                It.IsAny<Guid>()),
            Times.Once);
    }

	[TestMethod]
	public async Task GetOrganisationTeamMembers_ReturnsOk_WhenSuccessful()
	{
		// Arrange
		var organisationId = Guid.NewGuid();
		var serviceRoleId = 1;

		var teamMembers = new List<OrganisationTeamMemberModel>
		{
			new OrganisationTeamMemberModel { FirstName = "Alice", LastName = "Smith", Email = "alice@test.com" }
		};

		_mockOrganisationService.Setup(s =>
			s.GetOrganisationTeamMembers(It.IsAny<Guid>(), organisationId, serviceRoleId))
			.ReturnsAsync(teamMembers);

		// Act
		var result = await _sut.GetOrganisationTeamMembers(organisationId, serviceRoleId);

		// Assert
		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
		var model = okResult.Value.As<List<OrganisationTeamMemberModel>>();
		model.Should().NotBeNullOrEmpty();
	}

	[TestMethod]
	public async Task GetOrganisationTeamMembers_Returns500_WhenUserIdIsEmpty()
	{
		// Arrange
		var organisationId = Guid.NewGuid();
		var serviceRoleId = 1;

		// Clear UserId claim to simulate missing user
		_sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

		// Act
		var result = await _sut.GetOrganisationTeamMembers(organisationId, serviceRoleId);

		// Assert
		var problemResult = result.Should().BeOfType<StatusCodeResult>().Subject;
		problemResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
	}
}