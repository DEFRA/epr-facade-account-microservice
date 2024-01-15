using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FacadeAccountCreation.Core.Helpers;

public class CorrelationIdProvider : ICorrelationIdProvider
{
    private readonly ILogger<CorrelationIdProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _correlationIdHeader = "X-EPR-Correlation";

    public CorrelationIdProvider(
        ILogger<CorrelationIdProvider> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetHttpRequestCorrelationIdOrNew()
    {
        if (_httpContextAccessor.HttpContext?.Request?.Headers.ContainsKey(_correlationIdHeader) == true &&
            Guid.TryParse(_httpContextAccessor.HttpContext.Request.Headers[_correlationIdHeader], out var correlationId))
        {
            return correlationId;
        }

        correlationId = Guid.NewGuid();

        _logger.LogWarning("Failed to get the correlation ID. A new correlation ID has been generated: {CorrelationId}", correlationId);

        return correlationId;
    }
}