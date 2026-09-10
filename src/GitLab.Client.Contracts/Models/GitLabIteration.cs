namespace GitLab.Client.Models;

/// <summary>
///     An iteration (a timeboxed planning period owned by a group), as returned by the GitLab Iterations
///     API and embedded in board lists and issue iteration events.
///     <para>
///         Only <see cref="Id" /> is <c>required</c>, deliberately. This entity is projected into other
///         payloads - <c>APIEntitiesList</c> (board lists) and <c>APIEntitiesResourceIterationEvent</c> -
///         where GitLab may return a reduced shape, and a member marked required but absent there would
///         throw a <see cref="System.Text.Json.JsonException" /> and take down an unrelated call. That is a
///         conscious departure from <see cref="GitLabMilestone" />, whose <c>Iid</c>/<c>Title</c>/<c>State</c>
///         are required.
///     </para>
/// </summary>
public sealed record GitLabIteration
{
    public required long Id { get; init; }

    public long? Iid { get; init; }

    public long? Sequence { get; init; }

    public long? GroupId { get; init; }

    /// <summary>
    ///     Null for iterations created by "Enable automatic scheduling" in an iteration cadence - GitLab
    ///     documents both this and <see cref="Description" /> as null in that case.
    /// </summary>
    public string? Title { get; init; }

    /// <inheritdoc cref="Title" />
    public string? Description { get; init; }

    /// <summary>
    ///     GitLab sends the iteration's state as an <b>integer</b> here (1 upcoming, 2 current, 3 closed),
    ///     unlike every other state field in this library. The <c>state</c> <i>query</i> parameter on
    ///     <see cref="IterationListOptions.State" /> is the string-valued
    ///     <see cref="GitLabIterationStateFilter" />; the two are not interchangeable.
    /// </summary>
    public int? State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? DueDate { get; init; }

    public Uri? WebUrl { get; init; }
}