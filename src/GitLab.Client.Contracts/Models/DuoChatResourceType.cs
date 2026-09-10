using System.Text.Json.Serialization;

using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     The kind of GitLab object a <c>POST /chat/completions</c> question is asked about, which decides
///     how <see cref="DuoChatRequest.ResourceId" /> is interpreted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DuoChatResourceType>))]
public enum DuoChatResourceType
{
    /// <summary>An issue, addressed by its id.</summary>
    [JsonStringEnumMemberName("issue")] Issue,

    /// <summary>An epic, addressed by its id.</summary>
    [JsonStringEnumMemberName("epic")] Epic,

    /// <summary>A group, addressed by its id.</summary>
    [JsonStringEnumMemberName("group")] Group,

    /// <summary>A project, addressed by its id.</summary>
    [JsonStringEnumMemberName("project")] Project,

    /// <summary>A merge request, addressed by its id.</summary>
    [JsonStringEnumMemberName("merge_request")]
    MergeRequest,

    /// <summary>
    ///     A commit, addressed by its SHA rather than a numeric id - the one resource type that also
    ///     requires <see cref="DuoChatRequest.ProjectId" />.
    /// </summary>
    [JsonStringEnumMemberName("commit")] Commit,

    /// <summary>A CI job (GitLab's internal name for it is <c>build</c>), addressed by its id.</summary>
    [JsonStringEnumMemberName("build")] Build,

    /// <summary>A work item, addressed by its id.</summary>
    [JsonStringEnumMemberName("work_item")]
    WorkItem
}