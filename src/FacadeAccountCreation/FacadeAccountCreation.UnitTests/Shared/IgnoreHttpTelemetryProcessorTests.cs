using AutoFixture;
using FacadeAccountCreation.API.Shared;
using FluentAssertions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace FacadeAccountCreation.UnitTests.Shared
{
    [TestClass]
    public class IgnoreHttpTelemetryProcessorTests
    {
        private Fixture _fixture;

        [TestInitialize]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [TestMethod]
        public void Process_HttpRequestTelemetry_ShouldNotForwardToNext()
        {
            // Arrange
            var next = new Mock<ITelemetryProcessor>(MockBehavior.Strict);
            var sut = new IgnoreHttpTelemetryProcessor(next.Object);

            var path = Uri.EscapeDataString(_fixture.Create<string>());
            var telemetry = new RequestTelemetry
            {
                Url = new Uri($"http://example.test/{path}")
            };

            // Act
            Action act = () => sut.Process(telemetry);

            // Assert
            act.Should().NotThrow();
            next.Verify(n => n.Process(It.IsAny<ITelemetry>()), Times.Never,
                "HTTP requests should be ignored and not forwarded");
        }

        [TestMethod]
        public void Process_HttpsRequestTelemetry_ShouldForwardToNext()
        {
            // Arrange
            var next = new Mock<ITelemetryProcessor>(MockBehavior.Strict);
            var sut = new IgnoreHttpTelemetryProcessor(next.Object);

            var path = Uri.EscapeDataString(_fixture.Create<string>());
            var telemetry = new RequestTelemetry
            {
                Url = new Uri($"https://example.test/{path}")
            };

            next.Setup(n => n.Process(It.Is<ITelemetry>(t => ReferenceEquals(t, telemetry))));

            // Act
            sut.Process(telemetry);

            // Assert
            next.Verify(n => n.Process(It.Is<ITelemetry>(t => ReferenceEquals(t, telemetry))), Times.Once);
        }

        [TestMethod]
        public void Process_NonRequestTelemetry_ShouldForwardToNext()
        {
            // Arrange
            var next = new Mock<ITelemetryProcessor>(MockBehavior.Strict);
            var sut = new IgnoreHttpTelemetryProcessor(next.Object);

            var telemetry = new TraceTelemetry(_fixture.Create<string>());

            next.Setup(n => n.Process(It.Is<ITelemetry>(t => ReferenceEquals(t, telemetry))));

            // Act
            sut.Process(telemetry);

            // Assert
            next.Verify(n => n.Process(It.Is<ITelemetry>(t => ReferenceEquals(t, telemetry))), Times.Once);
        }

        [TestMethod]
        public void Process_NullTelemetry_ShouldForwardNullToNext()
        {
            // Arrange
            var next = new Mock<ITelemetryProcessor>(MockBehavior.Strict);
            var sut = new IgnoreHttpTelemetryProcessor(next.Object);

            next.Setup(n => n.Process((ITelemetry)null));

            // Act
            sut.Process(null);

            // Assert
            next.Verify(n => n.Process((ITelemetry)null), Times.Once);
        }
    }
}
