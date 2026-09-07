using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Window into a job's log for <c>GET /projects/:id/jobs/:job_id/trace</c>. Leaving both members unset
///     streams the whole log; setting them tails or resumes a long-running job's output without re-reading
///     what has already been consumed.
/// </summary>
[GitLabQuery]
public sealed record JobTraceOptions
{
    /// <summary>Byte offset into the log to start reading from.</summary>
    public long? ByteOffset { get; init; }

    /// <summary>How many bytes to read from <see cref="ByteOffset" />.</summary>
    public long? ByteLimit { get; init; }
}