using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class TopicsRepository(IGitLabApiConnection connection) : ITopicsRepository
{
    /// <summary>
    ///     The form field GitLab's topic endpoints read the avatar from. Forced here rather than trusted from
    ///     the caller's <see cref="GitLabFileUpload" />, whose default field name is "file" - sending it under
    ///     that name is accepted with a 200 and silently ignored.
    /// </summary>
    private const string AvatarFieldName = "avatar";

    public IAsyncEnumerable<GitLabTopic> ListAsync(TopicListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("topics").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabTopicArray,
            cancellationToken);
    }

    public Task<GitLabTopic> GetAsync(long topicId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("topics").Segment(topicId).Build(),
            GitLabJsonContext.Default.GitLabTopic,
            cancellationToken);
    }

    public Task<GitLabTopic> CreateAsync(CreateTopicRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("topics").Build(),
            request,
            GitLabJsonContext.Default.CreateTopicRequest,
            GitLabJsonContext.Default.GitLabTopic,
            cancellationToken);
    }

    public Task<GitLabTopic> UpdateAsync(long topicId, UpdateTopicRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("topics").Segment(topicId).Build(),
            request,
            GitLabJsonContext.Default.UpdateTopicRequest,
            GitLabJsonContext.Default.GitLabTopic,
            cancellationToken);
    }

    public Task<GitLabTopic> SetAvatarAsync(long topicId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(avatar);

        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("topics").Segment(topicId).Build(),
            avatar with { FieldName = AvatarFieldName },
            null,
            GitLabJsonContext.Default.GitLabTopic,
            cancellationToken);
    }

    public Task DeleteAsync(long topicId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("topics").Segment(topicId).Build(),
            cancellationToken);
    }

    // "merge" is a fixed word in the route template, not a topic name, so Literal - the topic ids travel
    // in the body here rather than in the path.
    public Task<GitLabTopic> MergeAsync(MergeTopicsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("topics").Literal("merge").Build(),
            request,
            GitLabJsonContext.Default.MergeTopicsRequest,
            GitLabJsonContext.Default.GitLabTopic,
            cancellationToken);
    }
}