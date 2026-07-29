using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var errorCode = exception is IErrorCodeException codedException
            ? codedException.Code
            : "INTERNAL_SERVER_ERROR";

        var problemDetails = exception switch
        {
            NotFoundException => CreateProblemDetails(
                httpContext,
                errorCode,
                StatusCodes.Status404NotFound,
                "Resource not found",
                exception.Message
            ),

            ConflictException => CreateProblemDetails(
                httpContext,
                errorCode,
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message
            ),

            BadRequestException => CreateProblemDetails(
                httpContext,
                errorCode,
                StatusCodes.Status400BadRequest,
                "Bad request",
                exception.Message
            ),

            ExternalServiceException => CreateProblemDetails(
                httpContext,
                errorCode,
                StatusCodes.Status503ServiceUnavailable,
                "Service unavailable",
                exception.Message
            ),

            IdentityOperationException identityException => new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = identityException.Message,
                Type = GetProblemType(errorCode),
                Instance = httpContext.Request.Path,
                Errors = identityException
                    .Errors.GroupBy(error => error.Code)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.Description).ToArray()
                    ),
            },

            InvalidCredentialsException => CreateProblemDetails(
                httpContext,
                errorCode,
                StatusCodes.Status401Unauthorized,
                "Invalid credentials",
                exception.Message
            ),

            _ => CreateProblemDetails(
                httpContext,
                errorCode,
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred."
            ),
        };

        problemDetails.Extensions["code"] = errorCode;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception,
            }
        );
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        string errorCode,
        int status,
        string title,
        string detail
    )
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = GetProblemType(errorCode),
            Instance = httpContext.Request.Path,
        };
    }

    private static string GetProblemType(string errorCode)
    {
        var problemName = errorCode.ToLowerInvariant().Replace('_', '-');

        return $"https://sorasora.com/problems/{problemName}";
    }
}
