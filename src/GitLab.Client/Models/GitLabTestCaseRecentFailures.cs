namespace GitLab.Client.Models;

/// <summary>
///     How often a test case has failed recently, as nested under
///     <see cref="GitLabTestCase.RecentFailures" />. GitLab computes it over the default branch, so it is
///     absent for a project whose default branch has no test history.
/// </summary>
public sealed record GitLabTestCaseRecentFailures
{
    /// <summary>How many times the test case failed on <see cref="BaseBranch" /> recently.</summary>
    public int? Count { get; init; }

    /// <summary>The branch the failure count was measured against - normally the project's default branch.</summary>
    public string? BaseBranch { get; init; }
}