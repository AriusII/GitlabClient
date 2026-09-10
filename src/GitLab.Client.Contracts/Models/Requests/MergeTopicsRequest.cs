namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /topics/merge</c>, which folds one topic into another. Administrators only.
/// </summary>
/// <remarks>
///     The merge is destructive and one-way: every project assigned to
///     <see cref="SourceTopicId" /> moves to <see cref="TargetTopicId" /> and the source topic is deleted.
///     GitLab answers with the surviving target topic.
/// </remarks>
public sealed record MergeTopicsRequest
{
    /// <summary>The topic to merge away. Deleted once its projects have been moved.</summary>
    public required long SourceTopicId { get; init; }

    /// <summary>The topic that survives and inherits the source topic's projects.</summary>
    public required long TargetTopicId { get; init; }
}