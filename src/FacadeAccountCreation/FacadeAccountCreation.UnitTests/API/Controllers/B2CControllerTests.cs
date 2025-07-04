using FacadeAccountCreation.Core.Models.B2c;
using FacadeAccountCreation.Core.Services.B2c;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class B2CControllerTests
{
    private readonly Guid _oid = Guid.NewGuid();
    private readonly Mock<IB2CService> _mockB2CService = new();
    private readonly NullLogger<B2CController> _nullLogger = new();
    private B2CController _sut = null!;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [TestInitialize]
    public void Setup()
    {
        _sut = new B2CController(
            _nullLogger,
            _mockB2CService.Object);
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest { ObjectId = _oid };
        var apiResponse = _fixture.Create<UserOrganisationIdentifiersResponse>();

        _mockB2CService.Setup(s => s.GetUserOrganisationIds(request))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            });

        // Act
        var result = await _sut.GetUserOrganisationIds(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(apiResponse);
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_ReturnsErrorStatus_WhenServiceFails()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest { ObjectId = _oid };

        _mockB2CService.Setup(s => s.GetUserOrganisationIds(request))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var result = await _sut.GetUserOrganisationIds(request);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        var badRequestResult = result as BadRequestResult;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_Returns500_WhenUserIdIsEmpty()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest { ObjectId = Guid.Empty };

        // Act
        var result = await _sut.GetUserOrganisationIds(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_Returns500_WhenExceptionThrown()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest { ObjectId = _oid };

        _mockB2CService.Setup(s => s.GetUserOrganisationIds(It.IsAny<UserOrganisationIdentifiersRequest>()))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act
        var result = await _sut.GetUserOrganisationIds(request);

        // Assert
        var statusResult = result as StatusCodeResult;
        Assert.IsNotNull(statusResult);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }
}
