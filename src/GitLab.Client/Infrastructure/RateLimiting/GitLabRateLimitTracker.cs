namespace GitLab.Client.Infrastructure.RateLimiting;

/// <summary>
///     The single observation of GitLab's rate-limit headers, shared by the whole client.
/// </summary>
/// <remarks>
///     The <see cref="Lock" /> is deliberate and measured: <see cref="GitLabRateLimitSnapshot" />? is a
///     nullable four-field struct that cannot be assigned atomically, so a torn read is genuinely possible
///     without it. The obvious "lock-free" rewrite - a volatile reference to an immutable holder - would add
///     one heap allocation per HTTP response to avoid an uncontended lock costing tens of nanoseconds. Do not
///     change it.
/// </remarks>
internal sealed class GitLabRateLimitTracker : IGitLabRateLimitTracker, IGitLabRateLimitWriter
{
    private readonly Lock _gate = new();
    private GitLabRateLimitSnapshot? _current;

    public GitLabRateLimitSnapshot? Current
    {
        get
        {
            lock (_gate)
            {
                return _current;
            }
        }
    }

    public void Update(GitLabRateLimitSnapshot snapshot)
    {
        lock (_gate)
        {
            _current = snapshot;
        }
    }
}