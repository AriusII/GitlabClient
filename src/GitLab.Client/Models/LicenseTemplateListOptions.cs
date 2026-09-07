using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /templates/licenses</c>.</summary>
[GitLabQuery]
public sealed record LicenseTemplateListOptions
{
    /// <summary>
    ///     When <see langword="true" />, returns only the licenses GitLab marks popular - the short list
    ///     its "add a license" UI offers first. Unset returns every license the instance knows.
    /// </summary>
    public bool? Popular { get; init; }

    /// <summary>
    ///     Page size. Pagination itself is automatic - the listing streams every page - so this only
    ///     tunes how many licenses each round trip carries.
    /// </summary>
    public int? PerPage { get; init; }
}