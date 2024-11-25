using FacadeAccountCreation.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ErrorControllerTests
{
    private ErrorsController _sut = null!;
    private Mock<IHostEnvironment> _hostEnvironment;
    private readonly NullLogger<ErrorsController> _logger = new();
    private Mock<HttpContext> _httpContextMock;
    Mock<HttpResponse> _httpResponseMock = null!;
    Mock<IFeatureCollection> _featureCollectionMock = null!;
    Mock<IExceptionHandlerFeature> _exceptionFeatureMock = null!;

    [TestInitialize]
    public void SetUp()
    {
        _hostEnvironment = new Mock<IHostEnvironment>();
        _httpContextMock = new Mock<HttpContext>();
        _httpResponseMock = new Mock<HttpResponse>();
        _featureCollectionMock = new Mock<IFeatureCollection>();
        _exceptionFeatureMock = new Mock<IExceptionHandlerFeature>();

        _httpContextMock.Setup(x => x.Response).Returns(_httpResponseMock.Object);
        _httpContextMock.Setup(x => x.Features).Returns(_featureCollectionMock.Object);
        _featureCollectionMock.Setup(x => x.Get<IExceptionHandlerFeature>()).Returns(_exceptionFeatureMock.Object);

        _sut = new ErrorsController(_logger);
        _sut.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    public void HandleError_handlesWithCorrectException_WhenIsProblemResponseException()
    {
        //Arrange
        const string exceptionMessage = "Test exception";
        var statusCode = HttpStatusCode.BadRequest;

        var problemDetails = new ProblemDetails
        {
            Title = exceptionMessage,
            Status = (int)statusCode
        };

        var problemResponseException = new ProblemResponseException(problemDetails, statusCode);
        _exceptionFeatureMock.Setup(x => x.Error).Returns(problemResponseException);

        _hostEnvironment.SetupGet(x => x.EnvironmentName).Returns("Production");

        //Act
        var result = _sut.HandleError(_hostEnvironment.Object);

        //Assert
        result.Should().BeOfType(typeof(ObjectResult));
        var statusCodeResult = result as ObjectResult;
        statusCodeResult.StatusCode.Should().Be(400);

        var problem = statusCodeResult.Value as ProblemDetails;
        problem.Title.Should().Be(exceptionMessage);
    }

    [TestMethod]
    public void HandleError_handlesWithCorrectException_WhenIsNotProblemResponseException()
    {
        //Arrange
        const string exceptionMessage = "Test exception";
        const string exceptionStackTrace = "Test stack trace";

        var mockException = new Mock<Exception>();
        mockException.SetupGet(x => x.Message).Returns(exceptionMessage);
        mockException.SetupGet(x => x.StackTrace).Returns(exceptionStackTrace);
        _exceptionFeatureMock.Setup(x => x.Error).Returns(mockException.Object);

        _hostEnvironment.SetupGet(x => x.EnvironmentName).Returns("Production");

        //Act
        var result = _sut.HandleError(_hostEnvironment.Object);

        //Assert
        result.Should().BeOfType(typeof(ObjectResult));
        var statusCodeResult = result as ObjectResult;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().BeOfType(typeof(ProblemDetails));
    }

    [TestMethod]
    public void HandleErrorDevelopment_handlesWithCorrectException_WhenIsProblemResponseException()
    {
        //Arrange
        const string exceptionMessage = "Test exception";
        var statusCode = HttpStatusCode.BadRequest;

        var problemDetails = new ProblemDetails
        {
            Title = exceptionMessage,
            Status = (int)statusCode
        };

        var problemResponseException = new ProblemResponseException(problemDetails, statusCode);
        _exceptionFeatureMock.Setup(x => x.Error).Returns(problemResponseException);

        _hostEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");

        //Act
        var result = _sut.HandleErrorDevelopment(_hostEnvironment.Object);

        //Assert
        result.Should().BeOfType(typeof(ObjectResult));
        var statusCodeResult = result as ObjectResult;
        statusCodeResult.StatusCode.Should().Be(400);

        var problem = statusCodeResult.Value as ProblemDetails;
        problem.Title.Should().Be(exceptionMessage);
    }

    [TestMethod]
    public void HandleErrorDevelopment_handlesWithCorrectException_WhenIsNotProblemResponseException()
    {
        //Arrange
        const string exceptionMessage = "Test exception";
        const string exceptionStackTrace = "Test stack trace";

        var mockException = new Mock<Exception>();
        mockException.SetupGet(x => x.Message).Returns(exceptionMessage);
        mockException.SetupGet(x => x.StackTrace).Returns(exceptionStackTrace);
        _exceptionFeatureMock.Setup(x => x.Error).Returns(mockException.Object);

        _hostEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");

        //Act
        var result = _sut.HandleErrorDevelopment(_hostEnvironment.Object);

        //Assert
        result.Should().BeOfType(typeof(ObjectResult));
        var statusCodeResult = result as ObjectResult;
        statusCodeResult.StatusCode.Should().Be(500);

        var problem = statusCodeResult.Value as ProblemDetails;
        problem.Title.Should().Be(exceptionMessage);
        problem.Detail.Should().Be(exceptionStackTrace);
    }

    [TestMethod]
    public void HandleErrorDevelopment_handlesWithNotFound_WhenIsNotDevelopment()
    {
        //Arrange
        _hostEnvironment.SetupGet(x => x.EnvironmentName).Returns("Production");
        _exceptionFeatureMock.Setup(x => x.Error).Returns(new Exception());

        //Act
        var result = _sut.HandleErrorDevelopment(_hostEnvironment.Object);

        //Assert
        result.Should().BeOfType(typeof(NotFoundResult));
    }
}