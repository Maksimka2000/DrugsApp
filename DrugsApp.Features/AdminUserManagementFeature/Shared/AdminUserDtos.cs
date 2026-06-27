using System;
using System.Collections.Generic;

namespace DrugsApp.Features.AdminUserManagementFeature.Shared;

/// <summary>
/// List item projection for admin users.
/// </summary>
public sealed record AdminUserListItemDto
{
    /// <summary>Admin user identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Full name of the admin user.</summary>
    public string FullName { get; init; }

    /// <summary>Login email of the admin user.</summary>
    public string Email { get; init; }

    /// <summary>Assigned admin role.</summary>
    public string AdminRole { get; init; }

    /// <summary>Current lockout or active status.</summary>
    public string Status { get; init; }

    /// <summary>Creation timestamp in UTC.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Indicates whether the account is protected from mutation.</summary>
    public bool IsProtected { get; init; }

    /// <summary>Indicates whether this entry represents the requesting user.</summary>
    public bool IsCurrentUser { get; init; }

    /// <summary>
    /// Creates an admin list item projection.
    /// </summary>
    public AdminUserListItemDto(
        Guid Id,
        string FullName,
        string Email,
        string AdminRole,
        string Status,
        DateTime CreatedAtUtc,
        bool IsProtected,
        bool IsCurrentUser)
    {
        this.Id = Id;
        this.FullName = FullName;
        this.Email = Email;
        this.AdminRole = AdminRole;
        this.Status = Status;
        this.CreatedAtUtc = CreatedAtUtc;
        this.IsProtected = IsProtected;
        this.IsCurrentUser = IsCurrentUser;
    }
}

/// <summary>
/// Paged response for admin user list queries.
/// </summary>
public sealed record ListAdminUsersResponse
{
    /// <summary>Returned admin users.</summary>
    public IReadOnlyList<AdminUserListItemDto> Items { get; init; }

    /// <summary>Total count matching the query.</summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Creates a paged response for admin users.
    /// </summary>
    public ListAdminUsersResponse(IReadOnlyList<AdminUserListItemDto> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}

/// <summary>
/// Detail projection for a single admin user.
/// </summary>
public sealed record AdminUserDetailDto
{
    /// <summary>Admin user identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Full name of the admin user.</summary>
    public string FullName { get; init; }

    /// <summary>Login email of the admin user.</summary>
    public string Email { get; init; }

    /// <summary>Assigned admin role.</summary>
    public string AdminRole { get; init; }

    /// <summary>Current lockout or active status.</summary>
    public string Status { get; init; }

    /// <summary>Creation timestamp in UTC.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Indicates whether the account is protected from mutation.</summary>
    public bool IsProtected { get; init; }

    /// <summary>Indicates whether this entry represents the requesting user.</summary>
    public bool IsCurrentUser { get; init; }

    /// <summary>
    /// Creates an admin user detail projection.
    /// </summary>
    public AdminUserDetailDto(
        Guid Id,
        string FullName,
        string Email,
        string AdminRole,
        string Status,
        DateTime CreatedAtUtc,
        bool IsProtected,
        bool IsCurrentUser)
    {
        this.Id = Id;
        this.FullName = FullName;
        this.Email = Email;
        this.AdminRole = AdminRole;
        this.Status = Status;
        this.CreatedAtUtc = CreatedAtUtc;
        this.IsProtected = IsProtected;
        this.IsCurrentUser = IsCurrentUser;
    }
}

/// <summary>
/// Request payload for creating an admin user.
/// </summary>
public sealed record CreateAdminUserRequest
{
    /// <summary>Full name of the new admin.</summary>
    public string FullName { get; init; }

    /// <summary>Login email of the new admin.</summary>
    public string Email { get; init; }

    /// <summary>Admin role to assign.</summary>
    public string AdminRole { get; init; }

    /// <summary>
    /// Creates a request to add an admin user.
    /// </summary>
    public CreateAdminUserRequest(string fullName, string email, string adminRole)
    {
        FullName = fullName;
        Email = email;
        AdminRole = adminRole;
    }
}

/// <summary>
/// Response payload after creating an admin user.
/// </summary>
public sealed record CreateAdminUserResponse
{
    /// <summary>New admin identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Full name of the new admin.</summary>
    public string FullName { get; init; }

    /// <summary>Login email of the new admin.</summary>
    public string Email { get; init; }

    /// <summary>Assigned admin role.</summary>
    public string AdminRole { get; init; }

    /// <summary>Generated initial password.</summary>
    public string Password { get; init; }

    /// <summary>
    /// Creates an admin creation response.
    /// </summary>
    public CreateAdminUserResponse(
        Guid Id,
        string FullName,
        string Email,
        string AdminRole,
        string Password)
    {
        this.Id = Id;
        this.FullName = FullName;
        this.Email = Email;
        this.AdminRole = AdminRole;
        this.Password = Password;
    }
}

/// <summary>
/// Request payload for updating an admin user's role.
/// </summary>
public sealed record UpdateAdminUserRoleRequest
{
    /// <summary>New admin role.</summary>
    public string AdminRole { get; init; }

    /// <summary>
    /// Creates an update role request.
    /// </summary>
    public UpdateAdminUserRoleRequest(string adminRole) => AdminRole = adminRole;
}

/// <summary>
/// Request payload for changing an admin user's password.
/// </summary>
public sealed record ChangeAdminUserPasswordRequest
{
    /// <summary>Desired new password.</summary>
    public string NewPassword { get; init; }

    /// <summary>Confirmation of the desired new password.</summary>
    public string ConfirmNewPassword { get; init; }

    /// <summary>
    /// Creates a change password request.
    /// </summary>
    public ChangeAdminUserPasswordRequest(string newPassword, string confirmNewPassword)
    {
        NewPassword = newPassword;
        ConfirmNewPassword = confirmNewPassword;
    }
}

/// <summary>
/// Request payload for changing an admin user's login email.
/// </summary>
public sealed record ChangeAdminUserLoginEmailRequest
{
    /// <summary>New login email.</summary>
    public string Email { get; init; }

    /// <summary>
    /// Creates a login email change request.
    /// </summary>
    public ChangeAdminUserLoginEmailRequest(string email) => Email = email;
}

/// <summary>
/// Request payload for suspending or unsuspending an admin user.
/// </summary>
public sealed record SuspendAdminUserRequest
{
    /// <summary>Flag indicating whether the admin is suspended.</summary>
    public bool IsSuspended { get; init; }

    /// <summary>
    /// Creates a suspension request.
    /// </summary>
    public SuspendAdminUserRequest(bool isSuspended) => IsSuspended = isSuspended;
}

/// <summary>
/// Response payload after changing an admin user's suspension state.
/// </summary>
public sealed record AdminUserSuspensionResponse
{
    /// <summary>Admin user identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Resulting status string.</summary>
    public string Status { get; init; }

    /// <summary>
    /// Creates a suspension response.
    /// </summary>
    public AdminUserSuspensionResponse(Guid id, string status)
    {
        Id = id;
        Status = status;
    }
}
