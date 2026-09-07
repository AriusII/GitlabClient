using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>scope</c> vocabulary of the merge request listings - whose merge requests to return.
///     <para>
///         The spec also accepts the hyphenated <c>created-by-me</c> and <c>assigned-to-me</c>, which are
///         legacy aliases of the underscored values below. They are deliberately not exposed, so this enum
///         cannot be used to write a call against a spelling that is on its way out.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MergeRequestScope>))]
public enum MergeRequestScope
{
    [JsonStringEnumMemberName("created_by_me")]
    CreatedByMe,

    [JsonStringEnumMemberName("assigned_to_me")]
    AssignedToMe,

    [JsonStringEnumMemberName("reviews_for_me")]
    ReviewsForMe,

    [JsonStringEnumMemberName("all")] All
}