using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/ci/lint</c>.</summary>
public sealed record ValidateCiConfigurationRequest
{
    /// <summary>
    ///     The whole <c>.gitlab-ci.yml</c> document, newlines and all. It travels as an ordinary JSON string -
    ///     no pre-escaping by the caller. GitLab's schema requires the member but permits an explicit JSON
    ///     <c>null</c>, so the serializer must retain the property even when its value is <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Content { get; init; }

    /// <summary>Simulate pipeline creation instead of only checking the syntax. Defaults to false.</summary>
    public bool? DryRun { get; init; }

    /// <summary>Include the jobs the configuration would produce in the response. Defaults to false.</summary>
    public bool? IncludeJobs { get; init; }

    /// <summary>The branch or tag used as context for a dry run. Only honoured when <see cref="DryRun" /> is true.</summary>
    public string? Ref { get; init; }
}