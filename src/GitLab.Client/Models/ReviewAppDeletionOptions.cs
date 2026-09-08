using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>DELETE /projects/:id/environments/review_apps</c>, which schedules stopped review
///     apps for deletion. GitLab takes them as query parameters rather than a body.
/// </summary>
[GitLabQuery]
public readonly record struct ReviewAppDeletionOptions
{
    /// <summary>The date before which stopped review apps can be deleted. GitLab defaults to 30 days ago.</summary>
    public DateTimeOffset? Before { get; init; }

    /// <summary>The maximum number of environments to delete. GitLab defaults to 100.</summary>
    public int? Limit { get; init; }

    /// <summary>
    ///     GitLab defaults this to <c>true</c> for safety, so nothing is deleted unless it is explicitly set
    ///     to <c>false</c>.
    /// </summary>
    public bool? DryRun { get; init; }
}