using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /users/:id</c>. Administrators only.
///     <para>
///         Every member is nullable and unset members are omitted, so this is a partial update despite
///         the verb: a <see langword="null" /> here leaves the field alone rather than clearing it.
///         Clearing a text field is done by sending an empty string, which is why
///         <see cref="Organization" /> and its neighbours are <see cref="string" /> rather than a nullable
///         value type.
///     </para>
///     <para>
///         <c>avatar</c> is deliberately absent: it is a <c>multipart/form-data</c> file part rather than
///         a JSON field.
///     </para>
/// </summary>
public sealed record UpdateUserRequest
{
    public string? Email { get; init; }

    /// <summary>A new password for the account. Never log this value or echo it back into an error message.</summary>
    public string? Password { get; init; }

    /// <summary>Apply a changed <see cref="Email" /> immediately instead of mailing a confirmation first.</summary>
    public bool? SkipReconfirmation { get; init; }

    public string? Name { get; init; }

    public string? Username { get; init; }

    public string? Linkedin { get; init; }

    public string? Twitter { get; init; }

    public string? Discord { get; init; }

    /// <summary>The user's own website. A string, matching <see cref="GitLabUser.WebsiteUrl" />; "" clears it.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "Mirrors GitLabUser.WebsiteUrl, which is a string because GitLab uses \"\" for unset.")]
    public string? WebsiteUrl { get; init; }

    public string? Github { get; init; }

    /// <summary>An empty string clears the field.</summary>
    public string? Organization { get; init; }

    public int? ProjectsLimit { get; init; }

    /// <summary>The account's UID at <see cref="Provider" />.</summary>
    public string? ExternUid { get; init; }

    /// <summary>The external authentication provider <see cref="ExternUid" /> belongs to.</summary>
    public string? Provider { get; init; }

    public string? Bio { get; init; }

    public string? Location { get; init; }

    public string? Pronouns { get; init; }

    public string? PublicEmail { get; init; }

    /// <summary>The address commits are attributed to; <c>_private</c> selects the private commit email.</summary>
    public string? CommitEmail { get; init; }

    /// <summary>Grant or revoke instance administrator rights.</summary>
    public bool? Admin { get; init; }

    public bool? CanCreateGroup { get; init; }

    public bool? External { get; init; }

    public int? ThemeId { get; init; }

    public int? ColorSchemeId { get; init; }

    public bool? PrivateProfile { get; init; }

    /// <summary>The administrator's private note about this account.</summary>
    public string? Note { get; init; }

    public bool? ViewDiffsFileByFile { get; init; }

    public bool? PolicyAdvancedEditor { get; init; }

    /// <summary>Compute-minutes quota for this user.</summary>
    public int? SharedRunnersMinutesLimit { get; init; }

    /// <summary>Additional compute-minutes quota on top of the plan's.</summary>
    public int? ExtraSharedRunnersMinutesLimit { get; init; }

    /// <summary>The group whose SAML configuration <see cref="ExternUid" /> authenticates against.</summary>
    public long? GroupIdForSaml { get; init; }

    /// <summary>Grant or revoke the auditor role. GitLab Premium and above.</summary>
    public bool? Auditor { get; init; }
}