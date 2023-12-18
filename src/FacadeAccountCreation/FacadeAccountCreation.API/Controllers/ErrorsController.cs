using FacadeAccountCreation.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FacadeAccountCreation.API.Controllers;

public class ErrorsController : ControllerBase
{
    private readonly ILogger<ErrorsController> _logger;

    public ErrorsController(ILogger<ErrorsController> logger)
    {
        _logger = logger;
    }

    [Route("/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment)
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        _logger.LogError(exceptionHandlerFeature.Error, "Unhandled exception has occurred");

        if (exceptionHandlerFeature.Error is ProblemResponseException exception)
        {
            return new ObjectResult(exception.ProblemDetails)
            {
                StatusCode = exception.ProblemDetails?.Status ?? 500
            };
        }
        
        return Problem();
    }

    [Route("/error-development")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult HandleErrorDevelopment([FromServices] IHostEnvironment hostEnvironment)
    {
        if (!hostEnvironment.IsDevelopment())
        {
            return NotFound();
        }
        
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        if (exceptionHandlerFeature.Error is ProblemResponseException exception)
        {
            return new ObjectResult(exception.ProblemDetails)
            {
                StatusCode = exception.ProblemDetails?.Status ?? 500
            };
        }
        
        return Problem(
            detail: exceptionHandlerFeature.Error.StackTrace,
            title: exceptionHandlerFeature.Error.Message);
    }
}