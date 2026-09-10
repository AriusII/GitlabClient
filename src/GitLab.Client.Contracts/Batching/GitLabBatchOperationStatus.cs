namespace GitLab.Client.Batching;

/// <summary>Represents the terminal or unscheduled state of one operation in a batch execution.</summary>
public enum GitLabBatchOperationStatus
{
    /// <summary>The operation was not started, for example because a fail-fast batch stopped scheduling work.</summary>
    NotStarted = 0,

    /// <summary>The operation completed and its typed result is available.</summary>
    Succeeded = 1,

    /// <summary>The operation faulted. Inspect <see cref="GitLabBatchOperation.GetException" /> for its cause.</summary>
    Failed = 2,

    /// <summary>The operation observed cancellation after it started.</summary>
    Canceled = 3
}