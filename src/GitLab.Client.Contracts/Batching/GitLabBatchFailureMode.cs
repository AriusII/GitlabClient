namespace GitLab.Client.Batching;

/// <summary>
///     Determines how a <see cref="GitLabBatchPlan" /> reacts when an operation cannot complete.
/// </summary>
public enum GitLabBatchFailureMode
{
    /// <summary>
    ///     Runs every independently scheduled operation and records each outcome. This is the default because it
    ///     lets callers use every successfully retrieved DTO in a later local mapper composition.
    /// </summary>
    CollectAll = 0,

    /// <summary>
    ///     Cancels the batch's internal token after the first failure, preventing work that has not started from
    ///     being scheduled. Calls already sent to GitLab cannot be retracted and may still complete.
    /// </summary>
    FailFast = 1
}