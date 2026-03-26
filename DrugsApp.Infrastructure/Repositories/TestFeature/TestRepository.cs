using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TestFeature.Entities;
using DrugsApp.Domain.TestFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.TestFeature;

public sealed class TestRepository : ITestRepository
{
    private readonly DrugsAppDbContext _context;

    public TestRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Test test, CancellationToken cancellationToken)
    {
        await _context.Tests.AddAsync(test, cancellationToken);
    }

    public async Task<Test?> GetByIdAsync(Guid testId, CancellationToken cancellationToken)
    {
        return await _context.Tests
            .AsNoTracking()
            .SingleOrDefaultAsync(test => test.Id == testId, cancellationToken);
    }
    
}

