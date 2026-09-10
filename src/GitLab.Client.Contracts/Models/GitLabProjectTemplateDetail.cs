namespace GitLab.Client.Models;

/// <summary>
///     A single project template (<c>GET /projects/:id/templates/:type/:name</c>).
///     <para>
///         The spec types every type's response with the licence entity, which is its widest form: only
///         <see cref="GitLabProjectTemplateType.Licenses" /> populates
///         <see cref="Nickname" />, <see cref="HtmlUrl" />, <see cref="Popular" />,
///         <see cref="Conditions" />, <see cref="Permissions" /> and <see cref="Limitations" />. For every
///         other type the useful members are <see cref="Name" /> and <see cref="Content" />.
///     </para>
/// </summary>
public sealed record GitLabProjectTemplateDetail
{
    /// <summary>The template's identifier, such as <c>gpl-3.0</c>.</summary>
    public string? Key { get; init; }

    /// <summary>The template's display name.</summary>
    public string? Name { get; init; }

    /// <summary>The licence's short name, such as <c>GNU GPLv3</c>. Licences only.</summary>
    public string? Nickname { get; init; }

    /// <summary>Where the licence is described in human terms. Licences only.</summary>
    public Uri? HtmlUrl { get; init; }

    /// <summary>Where the licence text came from. Licences only.</summary>
    public Uri? SourceUrl { get; init; }

    /// <summary>Whether GitLab lists the licence among its popular choices. Licences only.</summary>
    public bool? Popular { get; init; }

    /// <summary>A one-line description of the template.</summary>
    public string? Description { get; init; }

    /// <summary>What the licence requires of a user, such as <c>include-copyright</c>. Licences only.</summary>
    public IReadOnlyList<string>? Conditions { get; init; }

    /// <summary>What the licence grants, such as <c>commercial-use</c>. Licences only.</summary>
    public IReadOnlyList<string>? Permissions { get; init; }

    /// <summary>What the licence disclaims, such as <c>liability</c>. Licences only.</summary>
    public IReadOnlyList<string>? Limitations { get; init; }

    /// <summary>
    ///     The template body, with placeholders already expanded when
    ///     <see cref="ProjectTemplateOptions.Project" /> or <see cref="ProjectTemplateOptions.Fullname" />
    ///     was supplied.
    /// </summary>
    public string? Content { get; init; }
}