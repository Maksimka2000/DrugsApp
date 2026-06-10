using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Entities;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.AuthFeature;

public sealed class RefreshTokenRepository(DrugsAppDbContext _context) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        string normalizedToken = token.Trim();

        return await _context.RefreshTokens
            .FirstOrDefaultAsync(
                refreshToken => refreshToken.Token == normalizedToken,
                cancellationToken);
    }
}
