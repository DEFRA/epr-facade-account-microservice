using AutoFixture.AutoMoq;
using AutoFixture;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Services.Organisation;
using FacadeAccountCreation.Core.Services.PaymentCalculation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using FacadeAccountCreation.Core.Models.PaymentCalculation;
using FluentAssertions;

namespace FacadeAccountCreation.UnitTests.API.Controllers
{
    [TestClass]
    public class PaymentCalculationControllerTests
    {
        private readonly NullLogger<PaymentCalculationController> _nullLogger = new();
        private readonly Mock<IPaymentCalculationService> _mockPaymentCalculationService = new();
        private PaymentCalculationController _sut = null!;
        private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
        private Mock<HttpContext>? _httpContextMock;

        private PaymentCalculationRequest _paymentCalculationRequest;

        [TestInitialize]
        public void Setup()
        {
            _httpContextMock = new Mock<HttpContext>();
            _sut = new PaymentCalculationController(_nullLogger, _mockPaymentCalculationService.Object);
            _paymentCalculationRequest = new PaymentCalculationRequest { };
        }

        [TestMethod]
        public async Task ProducerRegistrationFees_ReturnsNoContent_WhenResultIsNull()
        {
            // Arrange
            _mockPaymentCalculationService.Setup(x =>
                    x.ProducerRegistrationFees(It.IsAny<PaymentCalculationRequest>()))
                .ReturnsAsync((PaymentCalculationResponse)null);

            // Act
            var result = await _sut.ProducerRegistrationFees(_paymentCalculationRequest);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task ProducerRegistrationFees_Returns500Error_WhenErrorIsThrown()
        {
            // Arrange
            _mockPaymentCalculationService.Setup(x =>
                    x.ProducerRegistrationFees(It.IsAny<PaymentCalculationRequest>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _sut.ProducerRegistrationFees(_paymentCalculationRequest) as StatusCodeResult;

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [TestMethod]
        public async Task ProducerRegistrationFees_ShouldReturnCorrectResponse_OnSuccessfulRequest()
        {
            // Arrange

            PaymentCalculationResponse respone = new PaymentCalculationResponse
            {
                ProducerRegistrationFee = 20000,
                SubsidiariesFee = 15000
            };

            _mockPaymentCalculationService.Setup(x =>
                    x.ProducerRegistrationFees(It.IsAny<PaymentCalculationRequest>())).ReturnsAsync(respone);

            // Act
            var result = await _sut.ProducerRegistrationFees(_paymentCalculationRequest) as ObjectResult;

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}
