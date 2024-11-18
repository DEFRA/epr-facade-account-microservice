using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Models.Person;
using FacadeAccountCreation.Core.Services.Person;
using FacadeAccountCreation.Core.Services.RoleManagement;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ConnectionsControllerTests
{
    private readonly Mock<IRoleManagementService> _roleManagementService = new();
    private readonly Mock<IMessagingService> _mockMessagingService = new();
    private readonly Mock<IPersonService> _mockPersonService = new();
    private readonly NullLogger<ConnectionsController> _nullLogger = new();
    private ConnectionsController _sut = null!;
    private readonly Guid _connectionId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {

        var messagingConfig = new MessagingConfig
        { 
            ApiKey = "test", 
            ComplianceSchemeAccountConfirmationTemplateId = Guid.NewGuid().ToString(),
            NominateDelegatedUserTemplateId = Guid.NewGuid().ToString(),
            ProducerAccountConfirmationTemplateId = Guid.NewGuid().ToString()
        }; 
                                                                                       
        var mockMessagingConfig = new Mock<IOptions<MessagingConfig>>();
        mockMessagingConfig.Setup(ap => ap.Value).Returns(messagingConfig);


        _sut = new ConnectionsController(_nullLogger, _roleManagementService.Object, _mockMessagingService.Object, _mockPersonService.Object, mockMessagingConfig.Object);
        _sut.AddDefaultContextWithOid(_userId, "TestAuth");
    }
    
    [TestMethod]
    public async Task GetConnectionPerson_WhenFound_ReturnConnectionPersonModel()
    {
        var expectedModel = new ConnectionPersonModel { FirstName = "Johnny", LastName = "Cash" };
        _roleManagementService.Setup(x => x.GetPerson(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(expectedModel));

        var result = await _sut.GetConnectionPerson(Guid.NewGuid(), "Packaging", Guid.NewGuid());
        result.Should().BeOfType<ActionResult<ConnectionPersonModel>>();
        result.Result.Should().BeOfType<OkObjectResult>();

        var okObjectResult = result.Result as OkObjectResult;
        okObjectResult.Value.Should().BeOfType<ConnectionPersonModel>();

        var connectionPersonModel = okObjectResult.Value as ConnectionPersonModel;
        connectionPersonModel.Should().Be(expectedModel);
    }

    [TestMethod]
    public async Task GetConnectionPerson_WhenNotFound_ReturnNotFoundResult()
    {
        _roleManagementService.Setup(x => x.GetPerson(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<ConnectionPersonModel>(null));

        var result = await _sut.GetConnectionPerson(Guid.NewGuid(), "Packaging", Guid.NewGuid());
        result.Should().BeOfType<ActionResult<ConnectionPersonModel>>();
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task GetConnectionEnrolments_WhenFound_ReturnConnectionWithEnrolmentsModel()
    {

        var expectedModel = new ConnectionWithEnrolmentsModel();
        _roleManagementService.Setup(x => x.GetEnrolments(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(expectedModel));

        var result = await _sut.GetConnectionEnrollments(Guid.NewGuid(), "Packaging", Guid.NewGuid());
        result.Should().BeOfType<ActionResult<ConnectionWithEnrolmentsModel>>();
        result.Result.Should().BeOfType<OkObjectResult>();

        var okObjectResult = result.Result as OkObjectResult;
        okObjectResult.Value.Should().BeOfType<ConnectionWithEnrolmentsModel>();

        var connectionPersonModel = okObjectResult.Value as ConnectionWithEnrolmentsModel;
        connectionPersonModel.Should().Be(expectedModel);
    }

    [TestMethod]
    public async Task GetConnectionEnrolments_WhenNotFound_ReturnNotFoundResult()
    {
        _roleManagementService.Setup(x => x.GetEnrolments(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<ConnectionWithEnrolmentsModel>(null));

        var result = await _sut.GetConnectionEnrollments(Guid.NewGuid(), "Packaging", Guid.NewGuid());
        result.Should().BeOfType<ActionResult<ConnectionWithEnrolmentsModel>>();
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task UpdatePersonRole_WhenUpdatedOk_ThenReturnOkResult()
    {
        var updateRequest = new UpdatePersonRoleRequest
        {
            PersonRole = PersonRole.Employee
        };

        _roleManagementService
            .Setup(x => x.UpdatePersonRole(_connectionId, _userId, _organisationId, "Packaging", updateRequest))
            .ReturnsAsync(new UpdatePersonRoleResponse
            {
                RemovedServiceRoles = null
            });

        var result = await _sut.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _organisationId) as OkResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [TestMethod]
    public async Task UpdatePersonRole_WhenUpdateResponds403Forbidden_ThenReturnStatusCodeResult403()
    {
        var updateRequest = new UpdatePersonRoleRequest
        {
            PersonRole = PersonRole.Employee
        };

        _roleManagementService
            .Setup(x => x.UpdatePersonRole(_connectionId, _userId, _organisationId, "Packaging", updateRequest))
            .ThrowsAsync(new HttpRequestException(null, null, HttpStatusCode.Forbidden));

        var result = await _sut.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _organisationId) as StatusCodeResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [TestMethod]
    public async Task UpdatePersonRole_WhenUpdateResponds400BadRequest_ThenThrow()
    {
        var updateRequest = new UpdatePersonRoleRequest
        {
            PersonRole = PersonRole.Employee
        };

        _roleManagementService
            .Setup(x => x.UpdatePersonRole(_connectionId, _userId, _organisationId, "Packaging", updateRequest))
            .ThrowsAsync(new HttpRequestException(null, null, HttpStatusCode.BadRequest));

        var result = await _sut.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _organisationId) as BadRequestResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [TestMethod]
    public async Task UpdatePersonRole_WhenUpdateRespondsWithUnexpectedErrorCode_ThenReturnStatusCodeResult500()
    {
        var updateRequest = new UpdatePersonRoleRequest
        {
            PersonRole = PersonRole.Employee
        };

        _roleManagementService
            .Setup(x => x.UpdatePersonRole(_connectionId, _userId, _organisationId, "Packaging", updateRequest))
            .Throws(new HttpRequestException(null, null, HttpStatusCode.Conflict));
       
        var result = await _sut.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _organisationId) as StatusCodeResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
    
    [TestMethod]
    public async Task UpdatePersonRole_WhenUpdatedOkForDelagatedPerson_ThenSendNotficationEmailAndReturnOkResult()
    {
        //Arrange
        var updateRequest = new UpdatePersonRoleRequest
        {
            PersonRole = PersonRole.Employee
        };

        _roleManagementService
            .Setup(x => x.UpdatePersonRole(_connectionId, _userId, _organisationId, "Packaging", updateRequest))
            .ReturnsAsync(new UpdatePersonRoleResponse
            {
                RemovedServiceRoles =
                [
                    new()
                    {
                        ServiceRoleKey = ServiceRoles.Packaging.DelegatedPerson
                    }
                ]
            });
        _roleManagementService.Setup(x => x.GetPerson(_connectionId, "Packaging", _userId, _organisationId))
            .ReturnsAsync(new ConnectionPersonModel());
        _mockPersonService.Setup(x => x.GetPersonByUserIdAsync(_userId)).ReturnsAsync(new PersonResponseModel());

        //Act
        var result = await _sut.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _organisationId) as OkResult;

        //Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        _mockMessagingService.Verify(x => x.SendNominationCancelledNotification(It.IsAny<DelegatedRoleEmailInput>()),Times.Once());

    }

    [TestMethod]
    public async Task UpdatePersonRole_WhenNotUpdatedFineForDelagatedPerson_ThenResultIsNullAndShouldThrowNotFoundError()
    {
        //Arrange
        var updateRequest = new UpdatePersonRoleRequest
        {
            PersonRole = PersonRole.Employee
        };

        _roleManagementService
            .Setup(x => x.UpdatePersonRole(_connectionId, _userId, _organisationId, "Packaging", updateRequest))
            .ReturnsAsync(new UpdatePersonRoleResponse
            {
                RemovedServiceRoles =
                [
                    new()
                    {
                        ServiceRoleKey = ServiceRoles.Packaging.DelegatedPerson
                    }
                ]
            });
        _roleManagementService.Setup(x => x.GetPerson(_connectionId, "Packaging", _userId, _organisationId));

        //Act
        var result = await _sut.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _organisationId) as OkResult;

        //Assert
        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task UpdatePersonRole_WhenUpdatedForDelagatedPersonAndNoApprovedUser_ThenResultIsNullAndShouldThrowNotFoundError()
    {
        //Arrange
        var updateRequest = new UpdatePersonRoleRequest
        {
            PersonRole = PersonRole.Employee
        };

        _roleManagementService
            .Setup(x => x.UpdatePersonRole(_connectionId, _userId, _organisationId, "Packaging", updateRequest))
            .ReturnsAsync(new UpdatePersonRoleResponse
            {
                RemovedServiceRoles =
                [
                    new()
                    {
                        ServiceRoleKey = ServiceRoles.Packaging.DelegatedPerson
                    }
                ]
            });
        _roleManagementService.Setup(x => x.GetPerson(_connectionId, "Packaging", _userId, _organisationId)).ReturnsAsync(new ConnectionPersonModel());
        _mockPersonService.Setup(x => x.GetPersonByUserIdAsync(_userId));

        //Act
        var result = await _sut.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _organisationId) as OkResult;

        //Assert
        result.Should().BeNull();
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public async Task NominateToDelegatedPerson_WhenUpdatedOk_ThenReturnOkResult()
    {
        var nominationRequest = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.ComplianceScheme,
            ComplianceSchemeName = "Test Compliance Scheme",
        };

        _roleManagementService
            .Setup(x => x.NominateToDelegatedPerson(_connectionId, _userId, _organisationId, "Packaging", nominationRequest))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var result = await _sut.NominateToDelegatedPerson(_connectionId, nominationRequest, "Packaging", _organisationId) as OkResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public async Task NominateToDelegatedPerson_WhenUpdateResponds403Forbidden_ThenReturnStatusCodeResult403()
    {
        var nominationRequest = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.ComplianceScheme,
            ComplianceSchemeName = "Test Compliance Scheme",
        };

        _roleManagementService
            .Setup(x => x.NominateToDelegatedPerson(_connectionId, _userId, _organisationId, "Packaging", nominationRequest))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden
            });

        var result = await _sut.NominateToDelegatedPerson(_connectionId, nominationRequest, "Packaging", _organisationId) as StatusCodeResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public async Task NominateToDelegatedPerson_WhenUpdateResponds400BadRequest_ThenReturnBadRequestResult()
    {
        var nominationRequest = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.ComplianceScheme,
            ComplianceSchemeName = "Test Compliance Scheme",
        };

        _roleManagementService
            .Setup(x => x.NominateToDelegatedPerson(_connectionId, _userId, _organisationId, "Packaging", nominationRequest))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var result = await _sut.NominateToDelegatedPerson(_connectionId, nominationRequest, "Packaging", _organisationId) as BadRequestResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public async Task NominateToDelegatedPerson_WhenUpdateRespondsWithUnexpectedErrorCode_ThenReturnStatusCodeResult500()
    {
        var nominationRequest = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.ComplianceScheme,
            ComplianceSchemeName = "Test Compliance Scheme",
        };

        _roleManagementService
            .Setup(x => x.NominateToDelegatedPerson(_connectionId, _userId, _organisationId, "Packaging", nominationRequest))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict
            });

        var result = await _sut.NominateToDelegatedPerson(_connectionId, nominationRequest, "Packaging", _organisationId) as StatusCodeResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
    
    [TestMethod]
    public async Task GetConnectionPerson_WhenNotFound_ReturnError()
    {
        _roleManagementService.Setup(x => x.GetPerson(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ThrowsAsync(new Exception("Test exception"));

        var result = await _sut.GetConnectionPerson(Guid.NewGuid(), "Packaging", Guid.NewGuid());
        result.Should().BeOfType<ActionResult<ConnectionPersonModel>>();
        result.Result.Should().BeOfType<StatusCodeResult>();

        var statusCodeResult = result.Result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task GetConnectionEnrolments_WhenNotFound_ReturnError()
    {
        _roleManagementService.Setup(x => x.GetEnrolments(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ThrowsAsync(new Exception("Test exception"));

        var result = await _sut.GetConnectionEnrollments(Guid.NewGuid(), "Packaging", Guid.NewGuid());
        result.Should().BeOfType<ActionResult<ConnectionWithEnrolmentsModel>>();
        result.Result.Should().BeOfType<StatusCodeResult>();

        var statusCodeResult = result.Result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public async Task UpdatePersonRole_WhenUpdateThrowsError_ThenResultRetunsError()
    {
        //Arrange
        var updateRequest = new UpdatePersonRoleRequest
        {
            PersonRole = PersonRole.Employee
        };

        _roleManagementService
            .Setup(x => x.UpdatePersonRole(_connectionId, _userId, _organisationId, "Packaging", updateRequest))
            .ReturnsAsync(new UpdatePersonRoleResponse
            {
                RemovedServiceRoles =
                [
                    new()
                    {
                        ServiceRoleKey = ServiceRoles.Packaging.DelegatedPerson
                    }
                ]
            });
        _roleManagementService.Setup(x => x.GetPerson(_connectionId, "Packaging", _userId, _organisationId)).ThrowsAsync(new Exception("Test exception"));

        //Act
        var result = await _sut.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _organisationId) as OkResult;

        //Assert
        result.Should().BeNull();
    }
    
    [TestMethod]
    [TestCategory("Nominating Delegated Person")]
    public async Task NominateToDelegatedPerson_WhenUpdateRespondsWithException_ThenReturnStatusCodeResult500()
    {
        var nominationRequest = new DelegatedPersonNominationRequest
        {
            RelationshipType = RelationshipType.ComplianceScheme,
            ComplianceSchemeName = "Test Compliance Scheme",
        };

        _roleManagementService
            .Setup(x => x.NominateToDelegatedPerson(_connectionId, _userId, _organisationId, "Packaging", nominationRequest))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _sut.NominateToDelegatedPerson(_connectionId, nominationRequest, "Packaging", _organisationId) as StatusCodeResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
