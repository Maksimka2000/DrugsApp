using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DrugsApp.Infrastructure.Persistence;

/// <summary>
/// EF Core unit-of-work implementation providing transactional boundaries for MediatR commands.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DrugsAppDbContext _context;
    private IDbContextTransaction? _transaction;
    // Logger add if needed: Add logger with type (ILogger<UnitOfWork>) 

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">EF Core database context.</param>
    public UnitOfWork(DrugsAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Persists staged changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditTimestamps();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        // Logger add if needed: Add logger with type (LogDebug)
    }

    /// <summary>
    /// Commits the current transaction after persisting staged changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
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

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
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


    private void UpdateAuditTimestamps()
    {
        DateTime utcNow = DateTime.UtcNow;
        foreach (EntityEntry<BaseEntity> entry in _context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property(e => e.CreatedAtUtc).CurrentValue == default)
                {
                    entry.Property(e => e.CreatedAtUtc).CurrentValue = utcNow;
                }
                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAtUtc).CurrentValue = utcNow;
                entry.Property(e => e.CreatedAtUtc).IsModified = false;
            }
        }
    }
}
