using MediatR;
using Microsoft.Extensions.Logging;

namespace IBS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior for logging requests.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation(
            "Handling {RequestName} {@Request}",
            requestName,
            request);

        var response = await next();

        if (response is Result result && result.IsFailure)
        {
            logger.LogWarning(
                "Request {RequestName} failed with error {ErrorCode}: {ErrorMessage}",
                requestName,
                result.Error.Code,
                result.Error.Message);
        }
        else
        {
            logger.LogInformation(
                "Handled {RequestName}",
                requestName);
        }

        return response;
    }
}
