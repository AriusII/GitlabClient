namespace GitLab.Client.Models.Responses;

/// <summary>
///     The outcome of scheduling stopped review apps for deletion
///     (<c>DELETE /projects/:id/environments/review_apps</c>).
/// </summary>
/// <remarks>
///     GitLab deliberately returns an envelope rather than a bare success status. In particular, the default
///     <c>dry_run=true</c> makes <see cref="ScheduledEntries" /> the only way to inspect what the operation would
///     affect. Entries use the environment basic projection, represented here by <see cref="GitLabEnvironment" />:
///     its nullable members accurately accommodate that smaller payload shape.
/// </remarks>
public sealed record GitLabReviewAppDeletionResult
{
    /// <summary>The stopped review apps GitLab scheduled, or would schedule during a dry run.</summary>
    public IReadOnlyList<GitLabEnvironment>? ScheduledEntries { get; init; }

    /// <summary>Entries GitLab could not schedule, for example because they are not in a stopped state.</summary>
    public IReadOnlyList<GitLabEnvironment>? UnprocessableEntries { get; init; }
}