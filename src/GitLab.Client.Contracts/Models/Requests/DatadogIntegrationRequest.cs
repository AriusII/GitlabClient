namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Datadog integration (<c>PUT /projects/:id/integrations/datadog</c>) - sends
///     pipeline and job events to Datadog CI Visibility.
/// </summary>
public sealed record DatadogIntegrationRequest
{
    /// <summary>Datadog site to send data to (for example <c>datadoghq.com</c>, <c>datadoghq.eu</c>).</summary>
    public string? DatadogSite { get; init; }

    /// <summary>Full URL of your Datadog site. Only required if you do not use a standard Datadog site.</summary>
    public Uri? ApiUrl { get; init; }

    /// <summary>API key used for authentication with Datadog.</summary>
    public required string ApiKey { get; init; }

    /// <summary>Enable CI Visibility.</summary>
    public bool? DatadogCiVisibility { get; init; }

    /// <summary>When enabled, job logs are collected by Datadog and displayed along with pipeline execution traces.</summary>
    public bool? ArchiveTraceEvents { get; init; }

    /// <summary>
    ///     Tag all pipeline data from this GitLab instance in Datadog. Useful when managing several self-managed
    ///     deployments.
    /// </summary>
    public string? DatadogService { get; init; }

    /// <summary>For self-managed deployments, the <c>env</c> tag for all the data sent to Datadog.</summary>
    public string? DatadogEnv { get; init; }

    /// <summary>Custom tags in Datadog. Specify one tag per line in the format <c>key:value</c>.</summary>
    public string? DatadogTags { get; init; }

    /// <summary>Trigger event when a pipeline status changes.</summary>
    public bool? PipelineEvents { get; init; }

    /// <summary>Trigger event when a build is created.</summary>
    public bool? BuildEvents { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Trigger event when a merge request is created, updated, or merged.</summary>
    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Trigger event for new comments.</summary>
    public bool? NoteEvents { get; init; }

    /// <summary>Trigger event for new tags pushed to the repository.</summary>
    public bool? TagPushEvents { get; init; }

    public bool? SubgroupEvents { get; init; }

    public bool? ProjectEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}