using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     The "basic" user shape GitLab embeds in other resources (issue/MR author, assignee, ...) and
///     returns from the Users list/get endpoints.
///     <para>
///         GitLab returns this one entity at three widths - <c>UserBasic</c> in every embedding,
///         <c>User</c> on <c>GET /users/:id</c>, and <c>UserWithAdmin</c> to administrators - so only the
///         four members the narrowest of them always carries are <c>required</c>. Everything else is
///         nullable and simply absent at the narrower widths; that is what lets the same type deserialize
///         a note author and an administrator's view of an account. Never make a member here
///         <c>required</c>: it would turn every embedding that omits it into a runtime failure.
///     </para>
/// </summary>
public sealed record GitLabUser
{
    public required long Id { get; init; }

    public required string Username { get; init; }

    public required string Name { get; init; }

    /// <summary>
    ///     <c>active</c>, <c>blocked</c>, <c>deactivated</c>, <c>banned</c> or <c>ldap_blocked</c>. Left as
    ///     free text because GitLab declares it as a bare string and adds states over time; a closed enum
    ///     here would fail to deserialize the first account in a state this package has not heard of.
    /// </summary>
    public string? State { get; init; }

    public bool? Locked { get; init; }

    public string? PublicEmail { get; init; }

    public Uri? AvatarUrl { get; init; }

    /// <summary>Instance-relative avatar path (<c>/uploads/-/system/user/avatar/1/avatar.png</c>), not an absolute URL.</summary>
    public string? AvatarPath { get; init; }

    public required Uri WebUrl { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Bio { get; init; }

    /// <summary><see cref="Bio" /> rendered to HTML by GitLab.</summary>
    public string? BioHtml { get; init; }

    public string? Location { get; init; }

    /// <summary>The LinkedIn username, not a URL. GitLab sends an empty string when unset.</summary>
    public string? Linkedin { get; init; }

    /// <summary>The X/Twitter username, not a URL. GitLab sends an empty string when unset.</summary>
    public string? Twitter { get; init; }

    /// <summary>The Discord user ID. GitLab sends an empty string when unset.</summary>
    public string? Discord { get; init; }

    /// <summary>
    ///     The user's own website. Typed as a string rather than a <see cref="Uri" /> because GitLab sends
    ///     an empty string - not null - for an account that has not set one.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab sends \"\" rather than null for a profile with no website, and System.Uri cannot "
            + "represent an empty value, so typing this as Uri would throw on deserialization of a "
            + "perfectly ordinary user payload.")]
    public string? WebsiteUrl { get; init; }

    /// <summary>The GitHub username. GitLab sends an empty string when unset.</summary>
    public string? Github { get; init; }

    public string? JobTitle { get; init; }

    public string? Pronouns { get; init; }

    public string? Organization { get; init; }

    /// <summary>Whether the account is a bot (a project or group access token's user, the support bot, ...).</summary>
    public bool? Bot { get; init; }

    /// <summary>The rendered "job title at organization" line shown on the profile.</summary>
    public string? WorkInformation { get; init; }

    public int? Followers { get; init; }

    public int? Following { get; init; }

    /// <summary>Whether the caller follows this user. Present only when the request was authenticated.</summary>
    public bool? IsFollowed { get; init; }

    /// <summary>The user's wall-clock time in their configured time zone, pre-formatted by GitLab ("3:38 PM").</summary>
    public string? LocalTime { get; init; }

    public DateTimeOffset? LastSignInAt { get; init; }

    public DateTimeOffset? ConfirmedAt { get; init; }

    /// <summary>The day the user was last active. A plain date, not a timestamp.</summary>
    public DateOnly? LastActivityOn { get; init; }

    /// <summary>The primary email address. Returned to administrators and to the account itself only.</summary>
    public string? Email { get; init; }

    public int? ThemeId { get; init; }

    public int? ColorSchemeId { get; init; }

    public int? ProjectsLimit { get; init; }

    public DateTimeOffset? CurrentSignInAt { get; init; }

    /// <summary>The external authentication providers linked to this account.</summary>
    public IReadOnlyList<GitLabUserIdentity>? Identities { get; init; }

    public bool? CanCreateGroup { get; init; }

    public bool? CanCreateProject { get; init; }

    public bool? TwoFactorEnabled { get; init; }

    public bool? External { get; init; }

    public bool? PrivateProfile { get; init; }

    /// <summary>The address commits are attributed to, which may be the private <c>_private</c> sentinel.</summary>
    public string? CommitEmail { get; init; }

    public string? PreferredLanguage { get; init; }

    /// <summary>Compute-minutes quota for this user. Administrators only.</summary>
    public int? SharedRunnersMinutesLimit { get; init; }

    /// <summary>Additional compute-minutes quota on top of the plan's. Administrators only.</summary>
    public int? ExtraSharedRunnersMinutesLimit { get; init; }

    /// <summary>Whether the account is an instance administrator. Administrators only.</summary>
    public bool? IsAdmin { get; init; }

    /// <summary>The administrator's private note about this account. Administrators only.</summary>
    public string? Note { get; init; }

    /// <summary>The user's personal namespace.</summary>
    public long? NamespaceId { get; init; }

    /// <summary>The project whose bot user this is, for project access tokens.</summary>
    public long? ProvisionedByProjectId { get; init; }

    /// <summary>The administrator who created the account, when it was not self-registered.</summary>
    public GitLabUser? CreatedBy { get; init; }

    /// <summary>Whether the account consumes a licensed seat. Administrators only.</summary>
    public bool? UsingLicenseSeat { get; init; }

    /// <summary>Whether the account has the auditor role. Administrators only, GitLab Premium and above.</summary>
    public bool? IsAuditor { get; init; }

    /// <summary>The group that provisioned this account through SAML/SCIM.</summary>
    public long? ProvisionedByGroupId { get; init; }

    /// <summary>The group that claimed this account as an enterprise user.</summary>
    public long? EnterpriseGroupId { get; init; }

    /// <summary>When the account became an enterprise user of <see cref="EnterpriseGroupId" />.</summary>
    public DateTimeOffset? EnterpriseGroupAssociatedAt { get; init; }

    /// <summary>
    ///     The account's administrator-defined custom attributes. Present when the request opted in with
    ///     <c>with_custom_attributes=true</c> (<see cref="UserListOptions.WithCustomAttributes" />), and
    ///     always present on the enterprise-user endpoints.
    /// </summary>
    public IReadOnlyList<GitLabCustomAttribute>? CustomAttributes { get; init; }

    /// <summary>
    ///     The account's SCIM-provisioned identities. The spec declares no shape for this member, so it is
    ///     carried as raw JSON rather than a shape the spec does not promise.
    /// </summary>
    public IReadOnlyList<JsonElement>? ScimIdentities { get; init; }
}