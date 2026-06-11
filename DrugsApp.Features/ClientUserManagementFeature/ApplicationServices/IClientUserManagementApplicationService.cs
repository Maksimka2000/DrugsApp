using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.ClientUserManagementFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.ClientUserManagementFeature.ApplicationServices;

public interface IClientUserManagementApplicationService
{
    Task<ErrorOr<ListClientUsersResponse>> ListAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken);
}
