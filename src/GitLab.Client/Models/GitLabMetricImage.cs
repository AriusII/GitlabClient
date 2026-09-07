using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A metric image attached to an incident
///     (<c>/projects/:id/issues/:issue_iid/metric_images</c>) - a screenshot of a dashboard, optionally
///     linked back to the dashboard it came from.
/// </summary>
public sealed record GitLabMetricImage
{
    public required long Id { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Filename { get; init; }

    /// <summary>The instance-relative path the image is served from.</summary>
    public string? FilePath { get; init; }

    /// <summary>Where the metric can be seen in full, if the uploader supplied it.</summary>
    public Uri? Url { get; init; }

    /// <summary>
    ///     A caption for the image or for <see cref="Url" />. GitLab calls the field <c>url_text</c>; a
    ///     string property whose name contains "Url" is CA1056, hence the explicit wire name.
    /// </summary>
    [JsonPropertyName("url_text")]
    public string? Caption { get; init; }
}