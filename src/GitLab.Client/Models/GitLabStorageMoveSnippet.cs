using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     The snippet shape GitLab embeds in a snippet repository storage move - the wire's
///     <c>APIEntitiesBasicSnippet</c>.
/// </summary>
/// <remarks>
///     Every member except <see cref="Id" /> is nullable: this is an embedding, and a personal snippet
///     omits <c>project_id</c> while a project snippet omits nothing but may still hide the author from
///     an unprivileged caller. <c>visibility</c> stays a <c>string</c> rather than
///     <c>GitLabVisibility</c> because the spec types it as a bare string here with no enumeration.
/// </remarks>
public sealed record GitLabStorageMoveSnippet
{
    /// <summary>Numeric snippet id - the value the snippet-scoped storage move routes take.</summary>
    public required long Id { get; init; }

    /// <summary>Snippet title.</summary>
    public string? Title { get; init; }

    /// <summary>Snippet description.</summary>
    public string? Description { get; init; }

    /// <summary>"private", "internal" or "public".</summary>
    public string? Visibility { get; init; }

    /// <summary>Who created the snippet.</summary>
    public GitLabUser? Author { get; init; }

    /// <summary>Id of the owning project, or <c>null</c> for a personal snippet.</summary>
    public long? ProjectId { get; init; }

    /// <summary>When the snippet was created.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the snippet was last updated.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Browser URL of the snippet.</summary>
    public Uri? WebUrl { get; init; }

    /// <summary>URL serving the snippet's raw content.</summary>
    public Uri? RawUrl { get; init; }

    /// <summary>SSH clone address of the snippet repository, in git's SCP-like form.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab sends the SCP-like form, git@host:snippets/65.git, which is not a valid absolute URI: "
            + "System.Uri parses it only as a RELATIVE Uri, so exposing it as Uri would hand callers an "
            + "instance whose AbsoluteUri throws. The string is what git itself consumes.")]
    public string? SshUrlToRepo { get; init; }

    /// <summary>HTTP clone URL of the snippet repository.</summary>
    public Uri? HttpUrlToRepo { get; init; }
}