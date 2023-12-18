﻿using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FacadeAccountCreation.API.HealthChecks;

public static class HealthCheckOptionBuilder
{
    public static HealthCheckOptions Build() => new()
    {
        AllowCachingResponses = false,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK
        }
    };
}
