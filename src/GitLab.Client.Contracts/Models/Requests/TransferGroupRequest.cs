namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /groups/:id/transfer</c>.
///     <para>
///         Leaving <see cref="GroupId" /> null is meaningful, not a mistake: it turns the group into a
///         top-level group instead of moving it under a new parent.
///     </para>
/// </summary>
public sealed record TransferGroupRequest
{
    /// <summary>The new parent group. Null promotes the group to the top level.</summary>
    public long? GroupId { get; init; }
}