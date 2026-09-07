using GitLab.Client.Abstractions;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Project topics, sitting between the public <c>ITopicsClient</c>
///     controller and <c>ITopicsRepository</c>'s raw GitLab access. Mirrors the repository's method shapes
///     1:1 today (its implementation is generated); this is the seam where request validation, caching, or
///     cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ITopicsService
{
    IAsyncEnumerable<GitLabTopic> ListAsync(TopicListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTopic> GetAsync(long topicId, CancellationToken cancellationToken = default);

    Task<GitLabTopic> CreateAsync(CreateTopicRequest request, CancellationToken cancellationToken = default);

    Task<GitLabTopic> UpdateAsync(long topicId, UpdateTopicRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabTopic> SetAvatarAsync(long topicId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long topicId, CancellationToken cancellationToken = default);

    Task<GitLabTopic> MergeAsync(MergeTopicsRequest request, CancellationToken cancellationToken = default);
}