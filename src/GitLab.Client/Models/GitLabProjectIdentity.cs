namespace GitLab.Client.Models;

/// <summary>
///     The minimal project shape GitLab embeds where only identity matters (a to-do item's
///     <c>project</c>, for one) - the wire's <c>APIEntitiesProjectIdentity</c>.
///     <para>
///         Deliberately not <see cref="GitLabProject" />: this payload carries no <c>visibility</c> and no
///         <c>web_url</c>, both of which <see cref="GitLabProject" /> marks <c>required</c>, and
///         System.Text.Json throws when a required member is absent. Reuse this type wherever GitLab sends
///         the identity-only projection.
///     </para>
/// </summary>
public sealed record GitLabProjectIdentity
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public string? NameWithNamespace { get; init; }

    public string? Path { get; init; }

    public string? PathWithNamespace { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}