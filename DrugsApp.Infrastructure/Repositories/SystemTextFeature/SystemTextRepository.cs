using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SystemTextFeature.Entities;
using DrugsApp.Domain.SystemTextFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.SystemTextFeature;

public sealed class SystemTextRepository : ISystemTextRepository
{
    private readonly DrugsAppDbContext _context;

    public SystemTextRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<SystemText?> GetByKeyAsync(string key, CancellationToken cancellationToken)
    {
        return await _context.SystemTexts
            .FirstOrDefaultAsync(systemText => systemText.Key == key, cancellationToken);
    }

}
