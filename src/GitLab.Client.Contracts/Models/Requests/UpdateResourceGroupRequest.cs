namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/resource_groups/:key</c>. The process mode is the only
///     mutable property of a resource group.
/// </summary>
public sealed record UpdateResourceGroupRequest
{
    /// <summary>
    ///     One of <c>unordered</c>, <c>oldest_first</c>, <c>newest_first</c> or <c>newest_ready_first</c>.
    ///     GitLab answers <c>400</c> for anything else.
    /// </summary>
    public required string ProcessMode { get; init; }
}