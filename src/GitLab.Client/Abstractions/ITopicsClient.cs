using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Project topics" API area (<c>/topics</c>) - the instance-wide vocabulary of tags
///     projects are labelled with, and the merge operation that folds one tag into another.
///     <para>
///         Listing and reading topics is open to any authenticated user. Creating, updating, deleting and
///         merging them is administrator-only and answers
///         <see cref="Exceptions.GitLabForbiddenException" /> otherwise.
///     </para>
/// </summary>
public interface ITopicsClient
{
    /// <summary>
    ///     Streams every topic on the instance, sorted by how many projects carry it, descending. Follows
    ///     pagination automatically.
    /// </summary>
    IAsyncEnumerable<GitLabTopic> ListAsync(TopicListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one topic by its numeric id.</summary>
    Task<GitLabTopic> GetAsync(long topicId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a topic. Administrators only. The avatar is not part of this call - GitLab takes it as a
    ///     file part, so set it afterwards with <see cref="SetAvatarAsync" />.
    /// </summary>
    Task<GitLabTopic> CreateAsync(CreateTopicRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a topic's slug, title or description. Administrators only. Omitted fields keep their
    ///     current value.
    /// </summary>
    Task<GitLabTopic> UpdateAsync(long topicId, UpdateTopicRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a topic's avatar as a <c>multipart/form-data</c> part on the same <c>PUT /topics/:id</c>
    ///     endpoint <see cref="UpdateAsync" /> uses. Administrators only.
    ///     <para>
    ///         The part is always sent under the field name <c>avatar</c>, whatever
    ///         <see cref="GitLabFileUpload.FieldName" /> says: GitLab answers <c>200</c> and silently ignores an
    ///         avatar sent under any other name, so the caller's value is overridden rather than trusted.
    ///     </para>
    ///     <para>The upload's stream is read but not disposed - the caller keeps ownership of it.</para>
    /// </summary>
    Task<GitLabTopic> SetAvatarAsync(long topicId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a topic. Administrators only. The projects that carried it simply lose the label; nothing
    ///     else about them changes.
    /// </summary>
    Task DeleteAsync(long topicId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Merges one topic into another: every project on the source topic moves to the target topic and the
    ///     source topic is deleted. Administrators only, destructive, and answers with the surviving target
    ///     topic.
    /// </summary>
    Task<GitLabTopic> MergeAsync(MergeTopicsRequest request, CancellationToken cancellationToken = default);
}