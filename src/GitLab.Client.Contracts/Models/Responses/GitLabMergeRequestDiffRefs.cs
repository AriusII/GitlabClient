namespace GitLab.Client.Models.Responses;

/// <summary>
///     The three commits GitLab uses to calculate a merge request diff
///     (<c>APIEntitiesDiffRefs</c> in the GitLab 19.x OpenAPI schema).
/// </summary>
public sealed record GitLabMergeRequestDiffRefs
{
    /// <summary>The merge base between the source and target branch.</summary>
    public string? BaseSha { get; init; }

    /// <summary>The source branch tip GitLab compared.</summary>
    public string? HeadSha { get; init; }

    /// <summary>The target branch tip GitLab compared.</summary>
    public string? StartSha { get; init; }
}