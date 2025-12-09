using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DrugsApp.Features.Common.Behaviors;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public RequestLoggingPipelineBehavior(ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        _logger.LogInformation("Request {RequestName} processing", requestName);

        try
        {
            TResponse response = await next();

            if (response is IErrorOr errorOrResponse)
            {
                if (errorOrResponse.IsError)
                {
                    _logger.LogWarning(
                        "Request {RequestName} completed with error  {@Errors}",
                        requestName,
                        errorOrResponse.Errors);
                }
                else
                {
                    _logger.LogInformation("Request {RequestName} Successfully processed", requestName);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Request {RequestName} processed with non ErrorOr response type {ResponseType}",
                    requestName,
                    typeof(TResponse).Name);
            }

            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing request {RequestName}",
                requestName);
            throw;
        }
    }
}
