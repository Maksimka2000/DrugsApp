using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Read-only role queries for Identity users.
/// </summary>
public interface IUserRoleReadService
{
    Task<bool> IsInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken);
}
