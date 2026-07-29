using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SoraSora.Shared.Extension;

public static class ControllerBaseExtension
{
    public static ActionResult ValidationProblem(
        this ControllerBase controllerBase,
        ValidationResult result,
        string errorCode = "VALIDATION_FAILED"
    )
    {
        var problemName = errorCode.ToLowerInvariant().Replace('_', '-');
        var problemDetails = new ValidationProblemDetails(result.ToDictionary())
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Type = $"https://sorasora.com/problems/{problemName}",
            Instance = controllerBase.HttpContext.Request.Path,
        };
        problemDetails.Extensions["code"] = errorCode;
        problemDetails.Extensions["traceId"] = controllerBase.HttpContext.TraceIdentifier;

        return controllerBase.ValidationProblem(problemDetails);
    }
}
