using System.Collections.ObjectModel;

namespace GitLab.Client.Batching;

/// <summary>Contains the outcomes of one completed <see cref="GitLabBatchPlan" /> execution.</summary>
public sealed class GitLabBatchExecution
{
    private readonly ReadOnlyCollection<Exception> _failures;

    internal GitLabBatchExecution(GitLabBatchPlan plan, GitLabBatchPlan.BatchOperationState[] operations)
    {
        Plan = plan;
        Operations = operations;

        List<Exception>? failures = null;
        foreach (GitLabBatchPlan.BatchOperationState operation in operations)
        {
            if (operation.Status == GitLabBatchOperationStatus.Failed && operation.Exception is { } exception)
            {
                (failures ??= []).Add(exception);
            }
        }

        _failures = new ReadOnlyCollection<Exception>(failures ?? []);
        Count = operations.Length;
        SucceededCount = operations.Count(static operation => operation.Status == GitLabBatchOperationStatus.Succeeded);
        FailedCount = _failures.Count;
        CanceledCount = operations.Count(static operation => operation.Status == GitLabBatchOperationStatus.Canceled);
        NotStartedCount =
            operations.Count(static operation => operation.Status == GitLabBatchOperationStatus.NotStarted);
    }

    internal GitLabBatchPlan Plan { get; }

    internal GitLabBatchPlan.BatchOperationState[] Operations { get; }

    /// <summary>Gets the number of operations registered in the plan at execution time.</summary>
    public int Count { get; }

    /// <summary>Gets the number of operations that completed with a result.</summary>
    public int SucceededCount { get; }

    /// <summary>Gets the number of operations that faulted.</summary>
    public int FailedCount { get; }

    /// <summary>Gets the number of started operations that observed cancellation.</summary>
    public int CanceledCount { get; }

    /// <summary>Gets the number of operations that did not start because a fail-fast plan stopped scheduling work.</summary>
    public int NotStartedCount { get; }

    /// <summary>Gets whether every registered operation completed successfully.</summary>
    public bool IsSuccess => FailedCount == 0 && CanceledCount == 0 && NotStartedCount == 0;

    /// <summary>Gets recorded operation failures in registration order.</summary>
    public IReadOnlyList<Exception> Failures => _failures;

    /// <summary>Throws an <see cref="AggregateException" /> containing all recorded failures in registration order.</summary>
    public void ThrowIfFailed()
    {
        if (_failures.Count > 0)
        {
            throw new AggregateException("One or more GitLab batch operations failed.", _failures);
        }
    }
}