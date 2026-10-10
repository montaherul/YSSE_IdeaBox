using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YSSE.IdeaBox.Api.Common;

namespace YSSE.IdeaBox.Api.Middleware;

/// <summary>
/// Runs any registered FluentValidation validator against the action arguments and,
/// on failure, short-circuits with the standard 400 validation envelope.
/// </summary>
public sealed class FluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _services;

    public FluentValidationFilter(IServiceProvider services) => _services = services;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_services.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (result.IsValid)
            {
                continue;
            }

            var details = result.Errors
                .GroupBy(e => string.IsNullOrEmpty(e.PropertyName) ? "_" : e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            context.Result = new ObjectResult(ApiResults.Error(
                "One or more validation errors occurred.",
                "VALIDATION",
                context.HttpContext.TraceIdentifier,
                details))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            return;
        }

        await next();
    }
}
