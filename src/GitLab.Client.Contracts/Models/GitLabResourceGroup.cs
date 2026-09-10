using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     A CI resource group (<c>/projects/:id/resource_groups</c>) - the mutex a job declares with
///     <c>resource_group:</c> so that only one job holding that key runs at a time.
///     <para>
///         Resource groups are created implicitly by the first pipeline that references the key; there is
///         no create endpoint, only read and update.
///     </para>
/// </summary>
public sealed record GitLabResourceGroup
{
    public required long Id { get; init; }

    /// <summary>The group's key, as written in <c>.gitlab-ci.yml</c>. May contain '/'.</summary>
    public required string Key { get; init; }

    /// <summary>
    ///     How queued jobs are picked: <c>unordered</c>, <c>oldest_first</c>, <c>newest_first</c> or
    ///     <c>newest_ready_first</c>. Left as a string because GitLab keeps adding modes; the four the spec
    ///     lists today are the ones <see cref="UpdateResourceGroupRequest.ProcessMode" /> accepts.
    /// </summary>
    public string? ProcessMode { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}