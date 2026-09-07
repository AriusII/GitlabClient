namespace GitLab.Client.Models;

/// <summary>
///     The minimal group shape GitLab embeds in a group repository storage move - the wire's
///     <c>APIEntitiesBasicGroupDetails</c>.
/// </summary>
/// <remarks>
///     Deliberately not <see cref="GitLabGroup" />: this projection carries only <c>id</c>,
///     <c>name</c> and <c>web_url</c>, while <see cref="GitLabGroup" /> marks <c>path</c> and
///     <c>visibility</c> required, and System.Text.Json throws when a required member is absent.
/// </remarks>
public sealed record GitLabStorageMoveGroup
{
    /// <summary>Numeric group id.</summary>
    public required long Id { get; init; }

    /// <summary>Display name of the group.</summary>
    public string? Name { get; init; }

    /// <summary>Browser URL of the group.</summary>
    public Uri? WebUrl { get; init; }
}