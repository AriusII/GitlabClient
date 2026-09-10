namespace GitLab.Client.Models;

/// <summary>
///     The authenticated user's preferences, as returned by <c>GET /user/preferences</c> and
///     <c>PUT /user/preferences</c>.
///     <para>
///         This is the narrow, API-exposed slice of the user's settings - the diff-viewing and CI-identity
///         flags - not everything the web UI's preferences page offers.
///     </para>
///     <para>
///         The spec types every member below as a bare <c>string</c>; the wire values are integers and
///         booleans, and are modelled as such here.
///     </para>
/// </summary>
public sealed record GitLabUserPreferences
{
    /// <summary>The preferences record's own id, which is not the user's id.</summary>
    public required long Id { get; init; }

    /// <summary>The user these preferences belong to.</summary>
    public long? UserId { get; init; }

    /// <summary>Whether diffs are shown one file per page rather than all files at once.</summary>
    public bool? ViewDiffsFileByFile { get; init; }

    /// <summary>Whether whitespace-only changes are rendered in diffs.</summary>
    public bool? ShowWhitespaceInDiffs { get; init; }

    /// <summary>
    ///     Whether the user's external identities are embedded in the JSON web token handed to their CI
    ///     jobs.
    /// </summary>
    public bool? PassUserIdentitiesToCiJwt { get; init; }

    /// <summary>Whether the advanced editor is enabled for security policy editing.</summary>
    public bool? PolicyAdvancedEditor { get; init; }
}