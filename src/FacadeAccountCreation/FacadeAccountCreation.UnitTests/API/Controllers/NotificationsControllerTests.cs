using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Models.Notifications;
using FacadeAccountCreation.Core.Services.Notification;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class NotificationsControllerTests
{
    private readonly Mock<INotificationsService> _mockNotificationsService = new();
    private readonly NullLogger<NotificationsController> _nullLogger = new();
    private NotificationsController _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _sut = new NotificationsController(_nullLogger, _mockNotificationsService.Object);
        _sut.AddDefaultContextWithOid(_userId, "TestAuth");
    }
    
    [TestMethod]
    public async Task GetNotifications_WhenFound_ReturnNotificationResponse()
    {
        var enrolmentId = Guid.NewGuid().ToString();
        var expectedModel = new NotificationsResponse
        { 
            Notifications =
            [
                new Notification
                {
                    Type = NotificationTypes.Packaging.DelegatedPersonNomination,
                    Data = new List<KeyValuePair<string, string>>
                        { new KeyValuePair<string, string>("enrolmentId", enrolmentId) }
                }
            ]
        };

        _mockNotificationsService.Setup(x => x.GetNotificationsForServiceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(expectedModel));

        var result = await _sut.GetNotifications("Packaging", Guid.NewGuid());
        result.Should().BeOfType<ActionResult<NotificationsResponse>>();
        result.Result.Should().BeOfType<OkObjectResult>();

        var okObjectResult = result.Result as OkObjectResult;
        okObjectResult.Value.Should().BeOfType<NotificationsResponse>();

        var notificationsResponse = okObjectResult.Value as NotificationsResponse;
        notificationsResponse.Should().Be(expectedModel);
    }

    [TestMethod]
    public async Task GetNotifications_WhenNotFound_ReturnNoContentResult()
    {
        _mockNotificationsService.Setup(x => x.GetNotificationsForServiceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult((NotificationsResponse?)null));

        var result = await _sut.GetNotifications("Packaging", Guid.NewGuid());

        result.Should().BeOfType<ActionResult<NotificationsResponse>>();
        result.Result.Should().BeOfType<NoContentResult>();
    }

    [TestMethod]
    public async Task GetNotifications_WhenUserNotInOrganisation_ReturnNoContentResult()
    {
        _mockNotificationsService.Setup(x => x.GetNotificationsForServiceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Throws(new HttpRequestException("", null, HttpStatusCode.Forbidden));

        var result = await _sut.GetNotifications("Packaging", Guid.NewGuid());

        result.Should().BeOfType<ActionResult<NotificationsResponse>>();
        result.Result.Should().BeOfType<StatusCodeResult>();

        var statusCodeResult = result.Result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }
}
