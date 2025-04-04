using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FacadeAccountCreation.UnitTests.API.HealthCheckOptionBuilder;

[TestClass]
public class HealthCheckTests
{
    [TestMethod]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        var healthReport = new HealthReport(null!, HealthStatus.Healthy, TimeSpan.Zero);
        var options = FacadeAccountCreation.API.HealthChecks.HealthCheckOptionBuilder.Build();

        // Act
        await options.ResponseWriter(httpContext, healthReport);

        // Assert
        ReadResponseBody(httpContext).Should().Be("Healthy");
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    private static string ReadResponseBody(DefaultHttpContext httpContext)
    {
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(httpContext.Response.Body);
        return reader.ReadToEnd();
    }
}