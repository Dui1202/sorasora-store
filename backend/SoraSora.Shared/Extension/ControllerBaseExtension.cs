using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace SoraSora.Shared.Extension;

public static class ControllerBaseExtension
{
    public static ActionResult ValidationProblem(
        this ControllerBase controllerBase,
        ValidationResult result
    )
    {
        var problemDetails = new ValidationProblemDetails(result.ToDictionary());

        return controllerBase.ValidationProblem(problemDetails);
    }
}
