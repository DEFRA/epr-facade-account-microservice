using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Services.Organisation;
using Microsoft.Extensions.Configuration;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.AutoMoq;
using AutoFixture;
using Microsoft.Extensions.Logging.Abstractions;
using FacadeAccountCreation.Core.Services.PaymentCalculation;
using System.Text.Json;
using FacadeAccountCreation.Core.Models.PaymentCalculation;
using Microsoft.AspNetCore.Mvc;
using FacadeAccountCreation.Core.Exceptions;
using FluentAssertions;

namespace FacadeAccountCreation.UnitTests.Core.Services
{
    [TestClass]
    public class PaymentCalculationServiceTests
    {
        private const string BaseAddress = "http://localhost";
        private const string GetPayCalEndpointsConfig = "api/producer/registration-fee";

        private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
        private readonly NullLogger<PaymentCalculationService> _logger = new();
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
        private readonly IConfiguration _configuration = GetConfig();

        private PaymentCalculationRequest paymentCalculationRequest = new PaymentCalculationRequest()
        {
            ApplicationReferenceNumber = "Test",
            ProducerType = "Large",
            Regulator = "GB-ENG",
            NoOfSubsidiariesOnlineMarketplace = 0,
            IsProducerOnlineMarketplace = false,
            NumberOfSubsidiaries = 0,
            IsLateFeeApplicable = false,
            SubmissionDate = DateTime.UtcNow
        };

        private static IConfiguration GetConfig()
        {
            var config = new Dictionary<string, string?>
        {
            {"PaymentCalculationEndpoints:PayCalEndpointsConfig", GetPayCalEndpointsConfig}
        };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            return configuration;
        }

        [TestMethod]
        public async Task ProducerRegistrationFees_ShouldReturnCorrectResponse()
        {
            // Arrange
            var apiResponse = _fixture.Create<PaymentCalculationResponse>();

            var expectedUrl =
                $"{BaseAddress}/{GetPayCalEndpointsConfig}";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.Method == HttpMethod.Post &&
                               req.RequestUri != null &&
                               req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(apiResponse))
                }).Verifiable();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            httpClient.BaseAddress = new Uri(BaseAddress);

            var sut = new PaymentCalculationService(httpClient, _logger, _configuration);

            //Act
            await sut.ProducerRegistrationFees(paymentCalculationRequest);

            // Assert
            _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri != null &&
                           req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task ProducerRegistrationFees_ShouldReturnUnsuccessfulResponse()
        {
            // Arrange
            var apiResponse = _fixture.Create<HttpRequestException>();

            var expectedUrl =
                $"{BaseAddress}/{GetPayCalEndpointsConfig}";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.Method == HttpMethod.Post &&
                               req.RequestUri != null &&
                               req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonSerializer.Serialize(apiResponse))
                }).Verifiable();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            httpClient.BaseAddress = new Uri(BaseAddress);

            var sut = new PaymentCalculationService(httpClient, _logger, _configuration);

            //Act
            var act = async () => await sut.ProducerRegistrationFees(paymentCalculationRequest);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
