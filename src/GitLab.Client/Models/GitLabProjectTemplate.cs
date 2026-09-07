namespace GitLab.Client.Models;

/// <summary>
///     One entry in a project's template list (<c>GET /projects/:id/templates/:type</c>). Fetch the body
///     with <c>GET /projects/:id/templates/:type/:name</c>, passing <see cref="Key" /> as the name.
/// </summary>
public sealed record GitLabProjectTemplate
{
    /// <summary>The template's identifier, and what the single-template route takes as its <c>name</c> segment.</summary>
    public required string Key { get; init; }

    /// <summary>The template's display name, such as <c>MIT License</c>.</summary>
    public string? Name { get; init; }
}