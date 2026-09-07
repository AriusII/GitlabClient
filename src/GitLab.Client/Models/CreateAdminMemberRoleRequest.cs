namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /admin_member_roles</c> - a custom admin role. An admin role carries no base access
///     level and only the <c>read_admin_*</c> abilities, which is why it is a separate shape from
///     <see cref="CreateMemberRoleRequest" />.
/// </summary>
public sealed record CreateAdminMemberRoleRequest
{
    /// <summary>Name for role (default: 'Admin role - custom')</summary>
    public string? Name { get; init; }

    /// <summary>Description for role</summary>
    public string? Description { get; init; }

    /// <summary>Read CI/CD details for runners and jobs in the Admin Area.</summary>
    public bool? ReadAdminCicd { get; init; }

    /// <summary>Read group details in the Admin Area.</summary>
    public bool? ReadAdminGroups { get; init; }

    /// <summary>Read project details in the Admin Area.</summary>
    public bool? ReadAdminProjects { get; init; }

    /// <summary>Read subscription details in the Admin area.</summary>
    public bool? ReadAdminSubscription { get; init; }

    /// <summary>
    ///     Read system information such as background migrations, health checks, and Gitaly in the Admin Area.
    /// </summary>
    public bool? ReadAdminMonitoring { get; init; }

    /// <summary>Read the user list and user details in the Admin area.</summary>
    public bool? ReadAdminUsers { get; init; }
}