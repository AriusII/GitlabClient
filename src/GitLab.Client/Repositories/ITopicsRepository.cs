using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Project topics resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ITopicsService), typeof(ITopicsClient))]
internal interface ITopicsRepository
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