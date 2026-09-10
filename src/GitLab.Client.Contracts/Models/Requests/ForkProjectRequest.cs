using GitLab.Client.Domain;

namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/fork</c>.</summary>
/// <remarks>
///     Every member is optional: forking with an empty body puts the fork in the authenticated user's own
///     namespace under the source project's name and path. The spec's <c>namespace</c> parameter is
///     absent because it is marked deprecated in favour of <see cref="NamespaceId" /> and
///     <see cref="NamespacePath" />.
/// </remarks>
public sealed record ForkProjectRequest
{
    /// <summary>The ID of the namespace the project will be forked into.</summary>
    public long? NamespaceId { get; init; }

    /// <summary>The path of the namespace the project will be forked into.</summary>
    public string? NamespacePath { get; init; }

    /// <summary>The path that will be assigned to the fork.</summary>
    public string? Path { get; init; }

    /// <summary>The name that will be assigned to the fork.</summary>
    public string? Name { get; init; }

    /// <summary>The description that will be assigned to the fork.</summary>
    public string? Description { get; init; }

    /// <summary>The visibility of the fork.</summary>
    public GitLabVisibility? Visibility { get; init; }

    /// <summary>Whether merge requests of the forked project target the fork itself by default.</summary>
    public bool? MrDefaultTargetSelf { get; init; }

    /// <summary>The branches to fork, rather than the whole repository.</summary>
    public string? Branches { get; init; }
}