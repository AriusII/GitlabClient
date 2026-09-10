namespace GitLab.Client.Models.Responses;

/// <summary>
///     The runner-protocol payload returned by <c>POST /jobs/request</c> when GitLab schedules a job.
///     The GitLab 19.4 OpenAPI schema declares its scalar leaves as strings; those wire types are preserved
///     even where their values conventionally represent identifiers, booleans, or structured runner data.
/// </summary>
public sealed record GitLabJobRequestResponse
{
    public string? Id { get; init; }

    public string? Token { get; init; }

    public string? AllowGitFetch { get; init; }

    public GitLabJobRequestJobInfo? JobInfo { get; init; }

    public GitLabJobRequestGitInfo? GitInfo { get; init; }

    public GitLabJobRequestRunnerInfo? RunnerInfo { get; init; }

    public string? Inputs { get; init; }

    public string? Variables { get; init; }

    public GitLabJobRequestStep? Steps { get; init; }

    public GitLabJobRequestHook? Hooks { get; init; }

    public GitLabJobRequestImage? Image { get; init; }

    public GitLabJobRequestService? Services { get; init; }

    public GitLabJobRequestArtifacts? Artifacts { get; init; }

    public GitLabJobRequestCache? Cache { get; init; }

    public GitLabJobRequestCredentials? Credentials { get; init; }

    public string? Features { get; init; }

    public string? Dependencies { get; init; }

    public string? Run { get; init; }

    public string? SuspendOptions { get; init; }

    public string? Secrets { get; init; }

    public string? PolicyOptions { get; init; }
}