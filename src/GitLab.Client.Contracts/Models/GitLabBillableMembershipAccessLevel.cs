namespace GitLab.Client.Models;

/// <summary>
///     The role a <see cref="GitLabBillableMembership" /> grants, as GitLab's billable-member endpoints
///     nest it: the numeric ladder value and its display name side by side.
///     <para>
///         Deliberately a separate type from <see cref="GitLabAccessLevel" />, which models the
///         push/merge entries of a protected branch and has nothing in common with this shape but the
///         name. GitLab types <c>integer_value</c> as a string in the spec and sends a number; the
///         context's <c>AllowReadingFromString</c> handling covers both.
///     </para>
/// </summary>
public sealed record GitLabBillableMembershipAccessLevel
{
    /// <summary>The numeric role - 10 Guest, 20 Reporter, 30 Developer, 40 Maintainer, 50 Owner.</summary>
    public int? IntegerValue { get; init; }

    /// <summary>The human-readable role name, for example "Developer".</summary>
    public string? StringValue { get; init; }

    /// <summary>
    ///     The custom member-role value supplied by the billable-membership response. GitLab 19.4 declares
    ///     this member as a string, so it is intentionally not projected to the broader member-role entity.
    /// </summary>
    public string? CustomRole { get; init; }
}