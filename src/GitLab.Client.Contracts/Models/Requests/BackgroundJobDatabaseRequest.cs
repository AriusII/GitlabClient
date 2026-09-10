namespace GitLab.Client.Models.Requests;

/// <summary>
///     The optional body shared by every batched-background-migration/operation action endpoint that
///     merely selects which database the action applies to (<c>pause</c>, <c>resume</c>,
///     <c>restart</c>, <c>stop</c>) and by <c>POST /admin/migrations/:timestamp/mark</c>. Omitting
///     <see cref="Database" /> leaves GitLab to use its own default, <c>main</c>.
/// </summary>
public sealed record BackgroundJobDatabaseRequest
{
    public GitLabBackgroundJobDatabase? Database { get; init; }
}