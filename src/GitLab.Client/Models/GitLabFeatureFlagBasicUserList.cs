namespace GitLab.Client.Models;

/// <summary>
///     The trimmed user list GitLab embeds in a <c>gitlabUserList</c>
///     <see cref="GitLabFeatureFlagStrategy" />. The full entity, with its timestamps and paths, is
///     <see cref="GitLabFeatureFlagUserList" />.
/// </summary>
public sealed record GitLabFeatureFlagBasicUserList
{
    public long? Id { get; init; }

    /// <summary>The list's internal ID, which is what the <c>feature_flags_user_lists/:iid</c> routes take.</summary>
    public long? Iid { get; init; }

    public string? Name { get; init; }

    /// <summary>The external user IDs in the list, as GitLab stores them: one comma-separated string.</summary>
    public string? UserXids { get; init; }
}