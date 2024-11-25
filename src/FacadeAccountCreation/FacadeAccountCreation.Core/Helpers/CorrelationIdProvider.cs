using Microsoft.AspNetCore.Http;

namespace FacadeAccountCreation.Core.Helpers;

public class CorrelationIdProvider(
    ILogger<CorrelationIdProvider> logger,
    IHttpContextAccessor httpContextAccessor)
    : ICorrelationIdProvider
{
    private readonly string _correlationIdHeader = "X-EPR-Correlation";

    public Guid GetHttpRequestCorrelationIdOrNew()
    {
        if (httpContextAccessor.HttpContext?.Request?.Headers.ContainsKey(_correlationIdHeader) == true &&
            Guid.TryParse(httpContextAccessor.HttpContext.Request.Headers[_correlationIdHeader], out var correlationId))
        {
            return correlationId;
        }

        correlationId = Guid.NewGuid();

        logger.LogWarning("Failed to get the correlation ID. A new correlation ID has been generated: {CorrelationId}", correlationId);

        return correlationId;
    }
}