using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TestFeature.Entities;

namespace DrugsApp.Domain.TestFeature.Repositories;

public interface ITestRepository
{
    Task AddAsync(Test test, CancellationToken cancellationToken);

    Task<Test?> GetByIdAsync(Guid testId, CancellationToken cancellationToken);
}

