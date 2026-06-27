using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.ClientUserManagementFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.ClientUserManagementFeature.ApplicationServices;

/// <summary>
/// Abstraction for listing client users.
/// </summary>
public interface IClientUserManagementApplicationService
{
    /// <summary>
    /// Retrieves a paged list of client users with optional search.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="search">Optional search text.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged client user list with total count.</returns>
    Task<ErrorOr<ListClientUsersResponse>> ListAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken);
}
