using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace DrugsApp.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly DrugsAppDbContext _context;
    private IDbContextTransaction? _transaction;
    // Logger add if needed: Add logger with type (ILogger<UnitOfWork>) 

    public UnitOfWork(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        // Logger add if needed: Add logger with type (LogDebug)
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction!.CommitAsync(cancellationToken);
            // Logger add if needed: Add logger with type (LogInformation)
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _transaction!.RollbackAsync(cancellationToken);
            // Logger add if needed: Add logger with type (LogWarning)
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
}