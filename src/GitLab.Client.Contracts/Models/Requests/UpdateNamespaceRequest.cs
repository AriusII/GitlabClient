using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>PUT /namespaces/:id</c> - GitLab's own compute-minutes, storage and subscription
///     attributes for a namespace. GitLab marks this endpoint deprecated since 17.8 in favour of managing
///     these attributes through the GitLab Customers Portal, but it remains the only REST API way to set
///     them directly.
/// </summary>
public sealed record UpdateNamespaceRequest
{
    /// <summary>Compute minutes quota for this namespace.</summary>
    public int? SharedRunnersMinutesLimit { get; init; }

    /// <summary>Extra compute minutes purchased for this namespace, on top of <see cref="SharedRunnersMinutesLimit" />.</summary>
    public int? ExtraSharedRunnersMinutesLimit { get; init; }

    /// <summary>Additional storage size purchased for this namespace, in megabytes.</summary>
    public long? AdditionalPurchasedStorageSize { get; init; }

    /// <summary>End date of the subscription for the additional purchased storage.</summary>
    public DateOnly? AdditionalPurchasedStorageEndsOn { get; init; }

    /// <summary>
    ///     Information on the GitLab subscription. GitLab's spec types it as an untyped object with no
    ///     declared shape, so it is carried as a raw <see cref="JsonElement" /> rather than an invented DTO.
    /// </summary>
    [JsonPropertyName("gitlab_subscription_attributes")]
    public JsonElement? GitLabSubscriptionAttributes { get; init; }
}