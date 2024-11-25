using FacadeAccountCreation.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace FacadeAccountCreation.API.Controllers;

#pragma warning disable S6931
public class ErrorsController(ILogger<ErrorsController> logger) : ControllerBase
#pragma warning restore S6931
{
    [Route("/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment)
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        logger.LogError(exceptionHandlerFeature.Error, "Unhandled exception has occurred");

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