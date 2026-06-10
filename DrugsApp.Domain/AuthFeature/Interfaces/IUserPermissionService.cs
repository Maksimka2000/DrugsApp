using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Enums;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

public interface IUserPermissionService
{
    Task<IReadOnlyCollection<string>> GetPermissionNamesAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> HasPermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken);

    Task WarmAsync(Guid userId, CancellationToken cancellationToken);

    Task InvalidateAsync(Guid userId, CancellationToken cancellationToken);
}
