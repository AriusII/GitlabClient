namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /project_aliases</c>, which points a new alias at an existing project.</summary>
public sealed record CreateProjectAliasRequest
{
    /// <summary>
    ///     The project to alias, as its numeric id or its full path (<c>gitlab-org/gitlab</c>). A JSON body
    ///     value rather than a route segment, so pass the path raw - it must NOT be URL-encoded here.
    /// </summary>
    public required string ProjectId { get; init; }

    /// <summary>The alias to create. Must be unique across the instance.</summary>
    public required string Name { get; init; }
}