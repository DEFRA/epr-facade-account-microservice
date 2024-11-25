using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Models.DelegatedPerson;
using FacadeAccountCreation.Core.Models.Enrolments;
using FacadeAccountCreation.Core.Services.RoleManagement;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class DelegatedPersonEnrolmentsControllerTests
{
    private DelegatedPersonEnrolmentsController _sut = null!;
    private readonly Mock<IRoleManagementService> _roleManagementService = new();
    private readonly NullLogger<DelegatedPersonEnrolmentsController> _logger = new();
    private readonly DeleteUserModel _deleteUserModel = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _enrolmentId = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly string _serviceKey = "Packaging";

    [TestInitialize]
    public void Setup()
    {
        _deleteUserModel.PersonExternalIdToDelete = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _deleteUserModel.ServiceRoleId = 1;

        _sut = new DelegatedPersonEnrolmentsController(_roleManagementService.Object, _logger);
        _sut.AddDefaultContextWithOid(_userId, "TestAuth");
    }

    [TestMethod]
    [TestCategory("AcceptNominationToDelegatedPerson")]
    public async Task
        AcceptNominationToDelegatedPerson_WhenRequestRespondsWithUnexpectedErrorCode_ThenReturnStatusCodeResult500()
    {
        var acceptNominationRequest = new AcceptNominationRequest
        {
            NomineeDeclaration = "Nominee declaration",
            Telephone = "01234000000"
        };

        var enrolmentId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        _roleManagementService
            .Setup(x => x.AcceptNominationToDelegatedPerson(enrolmentId, _userId, organisationId, "Packaging",
                acceptNominationRequest))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var result =
            await _sut.AcceptNominationToDelegatedPerson(enrolmentId, acceptNominationRequest, "Packaging",
                organisationId) as BadRequestResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [TestMethod]
    [TestCategory("AcceptNominationToDelegatedPerson")]
    public async Task AcceptNominationToDelegatedPerson_WhenRequestRespondsOk_ThenReturnOkResult()
    {
        var acceptNominationRequest = new AcceptNominationRequest
        {
            NomineeDeclaration = "Nominee declaration",
            Telephone = "01234000000"
        };

        var enrolmentId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        _roleManagementService
            .Setup(x => x.AcceptNominationToDelegatedPerson(enrolmentId, _userId, organisationId, "Packaging",
                acceptNominationRequest))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var result =
            await _sut.AcceptNominationToDelegatedPerson(enrolmentId, acceptNominationRequest, "Packaging",
                organisationId) as OkResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [TestMethod]
    public async Task GetDelegatedPersonNominator_WhenFound_ReturnDelegatedPersonNominatorModel()
    {
        var expectedModel = new DelegatedPersonNominatorModel { FirstName = "Johnny", LastName = "Cash", OrganisationName = "Org Name" };
        _roleManagementService
            .Setup(x => x.GetDelegatedPersonNominator(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<string>())).Returns(Task.FromResult(expectedModel));

        var result = await _sut.Get(_enrolmentId, _organisationId, _serviceKey);
        result.Should().BeOfType<ActionResult<DelegatedPersonNominatorModel>>();
        result.Result.Should().BeOfType<OkObjectResult>();

        var okObjectResult = result.Result as OkObjectResult;
        okObjectResult.Value.Should().BeOfType<DelegatedPersonNominatorModel>();

        var connectionPersonModel = okObjectResult.Value as DelegatedPersonNominatorModel;
        connectionPersonModel.Should().Be(expectedModel);
    }

    [TestMethod]
    public async Task GetDelegatedPersonNominator_WhenNotFound_ReturnNotFoundResult()
    {
        _roleManagementService
            .Setup(x => x.GetDelegatedPersonNominator(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<string>())).Returns(Task.FromResult<DelegatedPersonNominatorModel>(null));

        var result = await _sut.Get(_enrolmentId, _organisationId, _serviceKey);
        result.Should().BeOfType<ActionResult<DelegatedPersonNominatorModel>>();
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    [TestCategory("AcceptNominationToDelegatedPerson")]
    public async Task AcceptNominationToDelegatedPerson_WhenResponseThrowsException_ThenReturnInternalServerError()
    {
        var acceptNominationRequest = new AcceptNominationRequest
        {
            NomineeDeclaration = "Nominee declaration",
            Telephone = "01234000000"
        };

        var enrolmentId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        _roleManagementService
            .Setup(x => x.AcceptNominationToDelegatedPerson(enrolmentId, _userId, organisationId, "Packaging",
                acceptNominationRequest))
            .ThrowsAsync(new Exception("Test exception"));

        var result =
            await _sut.AcceptNominationToDelegatedPerson(enrolmentId, acceptNominationRequest, "Packaging",
                organisationId);

        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
        
    [TestMethod]
    public async Task GetDelegatedPersonNominator_WhenThrowsException_ReturnError()
    {
        _roleManagementService
            .Setup(x => x.GetDelegatedPersonNominator(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<string>())).ThrowsAsync(new Exception("Test exception"));

        var result = await _sut.Get(_enrolmentId, _organisationId, _serviceKey);
            
        result.Should().BeOfType<ActionResult<DelegatedPersonNominatorModel>>();
    }
}