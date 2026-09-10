namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>PUT /projects/:id/feature_flags_user_lists/:iid</c>. Both members are optional; the
///     library's <c>WhenWritingNull</c> policy omits the one left unset rather than clearing it.
/// </summary>
public sealed record UpdateFeatureFlagUserListRequest
{
    /// <summary>Renames the list.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Replaces the list's members. This is a full replacement, not an append: the comma-separated
    ///     string sent here becomes the whole list.
    /// </summary>
    public string? UserXids { get; init; }
}