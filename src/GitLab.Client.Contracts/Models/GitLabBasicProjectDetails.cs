using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>APIEntitiesBasicProjectDetails</c> projection embedded by board and environment responses.
///     It is intentionally distinct from <see cref="GitLabProject" />: the two wire schemas expose
///     different fields and neither promises any field as required.
/// </summary>
public sealed record GitLabBasicProjectDetails
{
    public long? Id { get; init; }

    public string? Description { get; init; }

    public string? Name { get; init; }

    public string? NameWithNamespace { get; init; }

    public string? Path { get; init; }

    public string? PathWithNamespace { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? DefaultBranch { get; init; }

    public IReadOnlyList<string>? TagList { get; init; }

    public IReadOnlyList<string>? Topics { get; init; }

    /// <summary>
    ///     GitLab's SSH clone location in its scp-like <c>git@host:namespace/project.git</c> notation.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab's documented SSH clone value is scp-like rather than a URI.")]
    public string? SshUrlToRepo { get; init; }

    public Uri? HttpUrlToRepo { get; init; }

    public Uri? WebUrl { get; init; }

    public Uri? ReadmeUrl { get; init; }

    public int? ForksCount { get; init; }

    public Uri? LicenseUrl { get; init; }

    public GitLabBasicProjectLicense? License { get; init; }

    public Uri? AvatarUrl { get; init; }

    public int? StarCount { get; init; }

    public DateTimeOffset? LastActivityAt { get; init; }

    public string? Visibility { get; init; }

    public GitLabBasicNamespace? Namespace { get; init; }

    public GitLabBasicCustomAttributeEntry? CustomAttributes { get; init; }

    public string? RepositoryStorage { get; init; }
}