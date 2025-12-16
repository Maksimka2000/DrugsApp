using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.Common.Behaviors;

/// <summary>
/// Wraps MediatR commands in a unit-of-work transaction, skipping queries.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    // Logger add if needed: Add logger with type (ILogger<TransactionBehavior<TRequest, TResponse>>)

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Begins a transaction for commands, commits on success, rolls back on errors.
    /// </summary>
    /// <param name="request">Incoming MediatR request.</param>
    /// <param name="next">Delegate for the rest of the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token propagated from ASP.NET Core.</param>
    /// <returns>Handler response.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip transaction for queries (read operations)
        if (IsQuery<TRequest>())
        {
            // Logger add if needed: Add logger with type (LogDebug)
            return await next();
        }

        // Logger add if needed: Add logger with type (LogInformation)

        // Start transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Execute the handler
            TResponse response = await next();

            // Check if response is ErrorOr and contains errors
            if (IsErrorResponse(response))
            {
                // Logger add if needed: Add logger with type (LogWarning)

                // Rollback transaction for business logic errors
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return response;
            }

            // Commit transaction if successful
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Logger add if needed: Add logger with type (LogInformation)

            return response;
        }
        catch (Exception)
        {
            // Logger add if needed: Add logger with type (LogError)

            // Rollback on exception
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static bool IsQuery<T>()
    {
        // Convention: Queries end with "Query"
        return typeof(T).Name.EndsWith("Query");
    }

    private static bool IsErrorResponse(TResponse response)
    {
        // Check if response is ErrorOr<T> and has errors
        Type responseType = typeof(TResponse);

        if (!responseType.IsGenericType)
            return false;

        Type genericType = responseType.GetGenericTypeDefinition();
        if (genericType != typeof(ErrorOr<>))
            return false;

        // Use reflection to check IsError property
        PropertyInfo? isErrorProperty = responseType.GetProperty("IsError");
        if (isErrorProperty == null)
            return false;

        return (bool)isErrorProperty.GetValue(response)!;
    }
}
