using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /users</c>. Administrators only.
///     <para>
///         Exactly one of <see cref="Password" />, <see cref="ResetPassword" /> and
///         <see cref="ForceRandomPassword" /> needs to be set; GitLab rejects the request when none is.
///         Prefer <see cref="ResetPassword" />, which mails the new account a set-password link and keeps
///         a credential out of this process entirely - a <see cref="Password" /> set here travels through
///         your logs, your history and GitLab's request body.
///     </para>
///     <para>
///         Every other member is nullable so an unset one is omitted from the payload and GitLab applies
///         its own default. <c>avatar</c> is deliberately absent: it is a <c>multipart/form-data</c> file
///         part rather than a JSON field, so it cannot be expressed on this body.
///     </para>
/// </summary>
public sealed record CreateUserRequest
{
    public required string Email { get; init; }

    public required string Name { get; init; }

    public required string Username { get; init; }

    /// <summary>The initial password. Never log this value or echo it back into an error message.</summary>
    public string? Password { get; init; }

    /// <summary>Send the new user a password-reset link instead of setting a password here.</summary>
    public bool? ResetPassword { get; init; }

    /// <summary>Set a random password the user can never learn, for accounts that sign in through SSO.</summary>
    public bool? ForceRandomPassword { get; init; }

    /// <summary>Treat <see cref="Email" /> as already confirmed, skipping the confirmation mail.</summary>
    public bool? SkipConfirmation { get; init; }

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

    /// <summary>The account's UID at <see cref="Provider" />, for pre-linking an external identity.</summary>
    public string? ExternUid { get; init; }

    /// <summary>The external authentication provider <see cref="ExternUid" /> belongs to.</summary>
    public string? Provider { get; init; }

    public string? Bio { get; init; }

    public string? Location { get; init; }

    public string? Pronouns { get; init; }

    public string? PublicEmail { get; init; }

    /// <summary>The address commits are attributed to; <c>_private</c> selects the private commit email.</summary>
    public string? CommitEmail { get; init; }

    /// <summary>Make the account an instance administrator.</summary>
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

    /// <summary>Give the account the auditor role. GitLab Premium and above.</summary>
    public bool? Auditor { get; init; }
}