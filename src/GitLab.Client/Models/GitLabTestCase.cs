using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One test case inside a pipeline's test report (<c>/projects/:id/pipelines/:pipeline_id/test_report</c>),
///     parsed by GitLab out of the JUnit XML a job published as an artifact.
/// </summary>
public sealed record GitLabTestCase
{
    /// <summary>
    ///     "success", "failed", "skipped" or "error". Kept a plain string rather than an enum, matching this
    ///     library's rule for GitLab's open-ended status fields.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>The test case name. GitLab substitutes "(No name)" when the JUnit report carries none.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The JUnit <c>classname</c> attribute - the suite, spec file or class the case belongs to. The wire
    ///     name is one word, so it cannot come from the snake_case policy.
    /// </summary>
    [JsonPropertyName("classname")]
    public string? ClassName { get; init; }

    /// <summary>The source file the case came from, when the JUnit report recorded one.</summary>
    public string? File { get; init; }

    /// <summary>
    ///     Run time in seconds. A float on the wire despite the spec typing it as an integer - JUnit reports
    ///     routinely carry sub-second times.
    /// </summary>
    public double? ExecutionTime { get; init; }

    /// <summary>The captured output of a failing case.</summary>
    public string? SystemOutput { get; init; }

    /// <summary>The stack trace of a failing case.</summary>
    public string? StackTrace { get; init; }

    /// <summary>How often this case has failed recently on the default branch, when GitLab tracks it.</summary>
    public GitLabTestCaseRecentFailures? RecentFailures { get; init; }

    /// <summary>
    ///     A screenshot or other attachment the case published. GitLab sends this as a path relative to the
    ///     instance root, so the value is a relative <see cref="Uri" />.
    /// </summary>
    public Uri? AttachmentUrl { get; init; }
}