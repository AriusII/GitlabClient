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
///     <para>
///         The <c>custom_role</c> member of the payload is not modelled here: it is an entire member-role
///         entity with several dozen permission flags, and it belongs to the member roles API area rather
///         than to this one.
///     </para>
/// </summary>
public sealed record GitLabBillableMembershipAccessLevel
{
    /// <summary>The numeric role - 10 Guest, 20 Reporter, 30 Developer, 40 Maintainer, 50 Owner.</summary>
    public int? IntegerValue { get; init; }

    /// <summary>The human-readable role name, for example "Developer".</summary>
    public string? StringValue { get; init; }
}