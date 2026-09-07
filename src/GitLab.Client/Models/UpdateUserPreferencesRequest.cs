namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /user/preferences</c>. Every member is optional - an omitted property is not
///     sent and keeps its current server-side value, so this is a partial update despite the verb.
/// </summary>
public sealed record UpdateUserPreferencesRequest
{
    /// <summary>Show one file diff per page rather than all files at once.</summary>
    public bool? ViewDiffsFileByFile { get; init; }

    /// <summary>Render whitespace-only changes in diffs.</summary>
    public bool? ShowWhitespaceInDiffs { get; init; }

    /// <summary>Embed the user's external identities in the JSON web token handed to their CI jobs.</summary>
    public bool? PassUserIdentitiesToCiJwt { get; init; }

    /// <summary>Enable the advanced editor for security policy editing.</summary>
    public bool? PolicyAdvancedEditor { get; init; }
}