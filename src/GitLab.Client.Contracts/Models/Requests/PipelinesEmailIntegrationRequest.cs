namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the pipeline status email integration
///     (<c>PUT /projects/:id/integrations/pipelines-email</c>).
/// </summary>
public sealed record PipelinesEmailIntegrationRequest
{
    /// <summary>Recipients, separated by whitespace.</summary>
    public required string Recipients { get; init; }

    /// <summary>Sends notifications only for broken pipelines.</summary>
    public bool? NotifyOnlyBrokenPipelines { get; init; }

    /// <summary>Sends notifications only when the pipeline status changes.</summary>
    public bool? NotifyOnlyWhenPipelineStatusChanges { get; init; }

    /// <summary>Sends notifications only for the default branch.</summary>
    public bool? NotifyOnlyDefaultBranch { get; init; }

    /// <summary>Branches for which notifications are sent.</summary>
    public string? BranchesToBeNotified { get; init; }

    /// <summary>Includes child pipelines in notifications.</summary>
    public bool? NotifyChildPipelines { get; init; }

    /// <summary>Triggers events when a pipeline status changes.</summary>
    public bool? PipelineEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}