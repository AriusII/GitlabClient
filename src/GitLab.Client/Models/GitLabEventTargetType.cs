using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>target_type</c> filter of the GitLab activity feeds, exactly as the spec enumerates it.
///     <para>
///         Deliberately not reused for <see cref="GitLabEvent.TargetType" />: the response reports the
///         target's Ruby class name rather than this filter vocabulary (<c>Note</c>, <c>WikiPage::Meta</c>,
///         <c>DesignManagement::Design</c>), and a value outside a string-serialised enum is a
///         <see cref="System.Text.Json.JsonException" />, not a null.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEventTargetType>))]
public enum GitLabEventTargetType
{
    [JsonStringEnumMemberName("issue")] Issue,

    [JsonStringEnumMemberName("milestone")]
    Milestone,

    [JsonStringEnumMemberName("merge_request")]
    MergeRequest,

    [JsonStringEnumMemberName("note")] Note,

    [JsonStringEnumMemberName("project")] Project,

    [JsonStringEnumMemberName("snippet")] Snippet,

    [JsonStringEnumMemberName("user")] User,

    [JsonStringEnumMemberName("wiki")] Wiki,

    [JsonStringEnumMemberName("design")] Design
}