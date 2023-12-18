using FacadeAccountCreation.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

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
    public void HandleError_handlesWthCorrectException()
    {
        //Act
        var result = _sut.HandleError(_hostEnvironment.Object);
        
        //Assert
        result.Should().NotBeNull();
        var statusCodeResult = result as ObjectResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
    [TestMethod]
    public void HandleErrorDevelopment_handlesWthCorrectException()
    {
        //Act
        var result = _sut.HandleErrorDevelopment(_hostEnvironment.Object);
        
        //Assert
        result.Should().NotBeNull();
        var statusCodeResult = result as ObjectResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }
    
}