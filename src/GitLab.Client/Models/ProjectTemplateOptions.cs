using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Query parameters for <c>GET /projects/:id/templates/:type/:name</c>. Every member is optional;
///     <see cref="Project" /> and <see cref="Fullname" /> only affect licence templates, which are the
///     only ones with placeholders to expand.
/// </summary>
[GitLabQuery]
public readonly record struct ProjectTemplateOptions
{
    /// <summary>
    ///     The project the template is stored in. Disambiguates templates that share a name across several
    ///     template projects.
    /// </summary>
    public long? SourceTemplateProjectId { get; init; }

    /// <summary>The project name to substitute into the licence's placeholders.</summary>
    public string? Project { get; init; }

    /// <summary>The copyright holder's full name to substitute into the licence's placeholders.</summary>
    public string? Fullname { get; init; }
}