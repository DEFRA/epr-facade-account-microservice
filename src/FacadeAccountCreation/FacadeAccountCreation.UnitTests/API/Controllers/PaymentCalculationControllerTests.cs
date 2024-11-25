using FacadeAccountCreation.Core.Models.PaymentCalculation;
using FacadeAccountCreation.Core.Services.PaymentCalculation;
using Microsoft.Extensions.Logging;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class PaymentCalculationControllerTests
{
    private Mock<ILogger<PaymentCalculationController>> _loggerMock = null!;
    private Mock<IPaymentCalculationService> _paymentCalculationServiceMock = null!;
    private PaymentCalculationController _controller = null!;

    private PaymentCalculationRequest _paymentCalculationRequest = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PaymentCalculationController>>();
        _paymentCalculationServiceMock = new Mock<IPaymentCalculationService>();
        _controller = new PaymentCalculationController(_loggerMock.Object, _paymentCalculationServiceMock.Object);
    }

    [TestMethod]
    public async Task ProducerRegistrationFees_ReturnsNoContent_WhenResultIsNull()
    {
        // Arrange
        _paymentCalculationServiceMock.Setup(x =>
                x.ProducerRegistrationFees(It.IsAny<PaymentCalculationRequest>()))
            .ReturnsAsync((PaymentCalculationResponse)null!);

        // Act
        var result = await _controller.ProducerRegistrationFees(_paymentCalculationRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [TestMethod]
    public async Task ProducerRegistrationFees_Returns500Error_WhenErrorIsThrown()
    {
        // Arrange
        _paymentCalculationRequest = new PaymentCalculationRequest { ApplicationReferenceNumber = "1234" };
        _paymentCalculationServiceMock.Setup(x =>
                x.ProducerRegistrationFees(It.IsAny<PaymentCalculationRequest>()))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _controller.ProducerRegistrationFees(_paymentCalculationRequest) as StatusCodeResult;

        // Assert
        result!.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task ProducerRegistrationFees_ShouldReturnCorrectResponse_OnSuccessfulRequest()
    {
        // Arrange

        var response = new PaymentCalculationResponse
        {
            ProducerRegistrationFee = 20000,
            SubsidiariesFee = 15000
        };

        _paymentCalculationServiceMock.Setup(x =>
            x.ProducerRegistrationFees(It.IsAny<PaymentCalculationRequest>())).ReturnsAsync(response);

        // Act
        var result = await _controller.ProducerRegistrationFees(_paymentCalculationRequest) as ObjectResult;

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        result!.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldReturnOk_WhenServiceReturnsResponse()
    {
        // Arrange
        var request = new PaymentInitiationRequest { Reference = "TestRef", Regulator = "GB-ENG", Amount = 100 };
        var expectedResponse = "redirect-url";

        _paymentCalculationServiceMock
            .Setup(service => service.PaymentInitiation(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.PaymentInitiation(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(expectedResponse);
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldReturnNoContent_WhenServiceReturnsNull()
    {
        // Arrange
        var request = new PaymentInitiationRequest { Reference = "TestRef", Regulator = "GB-ENG", Amount = 100 };

        _paymentCalculationServiceMock
            .Setup(service => service.PaymentInitiation(request))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _controller.PaymentInitiation(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldReturnBadRequest_WhenServiceValidatesBadRequest()
    {
        // Arrange
        var request = new PaymentInitiationRequest { Reference = "TestRef", Regulator = "Invalid-Regulator", Amount = -100 };

        _paymentCalculationServiceMock
            .Setup(service => service.PaymentInitiation(request))
            .ThrowsAsync(new HttpRequestException(null, null, HttpStatusCode.BadRequest));

        // Act
        var result = await _controller.PaymentInitiation(request);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }

    [TestMethod]
    public async Task PaymentInitiation_ShouldReturnInternalServerError_WhenServiceThrowsException()
    {
        // Arrange
        var request = new PaymentInitiationRequest { Reference = "TestRef", Regulator = "GB-ENG", Amount = 100 };

        _paymentCalculationServiceMock
            .Setup(service => service.PaymentInitiation(request))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.PaymentInitiation(request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to Initialise Payment for application Reference : ") && v.ToString()!.Contains(request.Reference)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }
}