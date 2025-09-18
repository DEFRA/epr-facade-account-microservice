using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
namespace FacadeAccountCreation.API.Shared;
public class IgnoreHttpTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    public IgnoreHttpTelemetryProcessor(ITelemetryProcessor next) => _next = next;

    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry request &&
            request.Url.Scheme == Uri.UriSchemeHttp)
        {
            return; // ignore http:// requests
        }
        _next.Process(item);
    }
}
