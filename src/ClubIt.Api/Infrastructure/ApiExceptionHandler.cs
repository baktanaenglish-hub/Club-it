using ClubIt.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ClubIt.Api.Infrastructure;

/// <summary>Translates application exceptions into RFC 7807 problem responses.</summary>
public class ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException e => new ValidationProblemDetails(e.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = e.Message
            },
            NotFoundException e => Problem(StatusCodes.Status404NotFound, e.Message),
            ForbiddenException e => Problem(StatusCodes.Status403Forbidden, e.Message),
            UnauthorizedException e => Problem(StatusCodes.Status401Unauthorized, e.Message),
            ConflictException e => Problem(StatusCodes.Status409Conflict, e.Message),
            _ => Problem(StatusCodes.Status500InternalServerError, "אירעה שגיאה בלתי צפויה.")
        };

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ProblemDetails Problem(int status, string title) => new() { Status = status, Title = title };
}
