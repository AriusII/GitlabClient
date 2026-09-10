namespace GitLab.Client.Models.Responses;

/// <summary>Job identity and queue metadata included in a runner job-request response.</summary>
public sealed record GitLabJobRequestJobInfo
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Stage { get; init; }
    public string? PipelineId { get; init; }
    public string? ProjectId { get; init; }
    public string? ProjectName { get; init; }
    public string? ProjectFullPath { get; init; }
    public string? NamespaceId { get; init; }
    public string? RootNamespaceId { get; init; }
    public string? OrganizationId { get; init; }
    public string? InstanceId { get; init; }
    public string? InstanceUuid { get; init; }
    public string? UserId { get; init; }
    public string? ScopedUserId { get; init; }
    public string? TimeInQueueSeconds { get; init; }
    public string? ProjectJobsRunningOnInstanceRunnersCount { get; init; }
    public string? QueueSize { get; init; }
    public string? QueueDepth { get; init; }
}