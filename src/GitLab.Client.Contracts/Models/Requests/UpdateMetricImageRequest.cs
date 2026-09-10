using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/issues/:issue_iid/metric_images/:metric_image_id</c>. The image
///     itself cannot be replaced - only the link and its caption.
/// </summary>
public sealed record UpdateMetricImageRequest
{
    /// <summary>Where the metric can be seen in full.</summary>
    public Uri? Url { get; init; }

    /// <inheritdoc cref="GitLabMetricImage.Caption" />
    [JsonPropertyName("url_text")]
    public string? Caption { get; init; }
}