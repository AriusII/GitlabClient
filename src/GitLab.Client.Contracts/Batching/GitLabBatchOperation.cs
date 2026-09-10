using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace GitLab.Client.Batching;

/// <summary>Represents one deferred operation registered with a <see cref="GitLabBatchPlan" />.</summary>
/// <remarks>
///     An operation has no result or status until its owning plan has completed exactly one execution. Handles are
///     intentionally tied to their own plan so values cannot accidentally be read from a different batch.
/// </remarks>
public class GitLabBatchOperation
{
    private readonly int _index;
    private readonly GitLabBatchPlan _plan;

    internal GitLabBatchOperation(GitLabBatchPlan plan, int index)
    {
        _plan = plan;
        _index = index;
    }

    /// <summary>Gets the operation's outcome in <paramref name="execution" />.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="execution" /> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="execution" /> belongs to another plan.</exception>
    public GitLabBatchOperationStatus GetStatus(GitLabBatchExecution execution)
    {
        return GetState(execution).Status;
    }

    /// <summary>Gets the original operation exception, if the operation faulted or was canceled.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="execution" /> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="execution" /> belongs to another plan.</exception>
    public Exception? GetException(GitLabBatchExecution execution)
    {
        return GetState(execution).Exception;
    }

    /// <summary>
    ///     Rethrows the original operation exception when this operation did not complete successfully.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="execution" /> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="execution" /> belongs to another plan.</exception>
    /// <exception cref="InvalidOperationException">The operation did not start.</exception>
    public void ThrowIfFailed(GitLabBatchExecution execution)
    {
        GitLabBatchPlan.BatchOperationState state = GetState(execution);
        if (state.Status == GitLabBatchOperationStatus.Succeeded)
        {
            return;
        }

        RethrowOrThrowNotStarted(state);
    }

    internal GitLabBatchPlan.BatchOperationState GetState(GitLabBatchExecution execution)
    {
        ArgumentNullException.ThrowIfNull(execution);

        if (!ReferenceEquals(_plan, execution.Plan))
        {
            throw new ArgumentException("The execution belongs to a different GitLab batch plan.", nameof(execution));
        }

        return execution.Operations[_index];
    }

    internal static void RethrowOrThrowNotStarted(GitLabBatchPlan.BatchOperationState state)
    {
        if (state.Exception is { } exception)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        throw new InvalidOperationException(
            "The GitLab batch operation did not start because the batch stopped scheduling work.");
    }
}

/// <summary>Represents one deferred operation with a typed result.</summary>
/// <typeparam name="TResult">The result produced by the registered asynchronous operation.</typeparam>
public sealed class GitLabBatchOperation<TResult> : GitLabBatchOperation
{
    internal GitLabBatchOperation(GitLabBatchPlan plan, int index)
        : base(plan, index)
    {
    }

    /// <summary>
    ///     Gets this operation's result or rethrows its original failure. A successful nullable operation may
    ///     legitimately return <see langword="null" />.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="execution" /> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="execution" /> belongs to another plan.</exception>
    /// <exception cref="InvalidOperationException">The operation did not start.</exception>
    public TResult GetResult(GitLabBatchExecution execution)
    {
        GitLabBatchPlan.BatchOperationState state = GetState(execution);
        if (state.Status == GitLabBatchOperationStatus.Succeeded)
        {
            return (TResult)state.Result!;
        }

        RethrowOrThrowNotStarted(state);
        throw new UnreachableException();
    }

    /// <summary>Attempts to get this operation's typed result without throwing a recorded operation failure.</summary>
    /// <param name="execution">The completed execution produced by this operation's owning plan.</param>
    /// <param name="result">The result when the operation succeeded; otherwise the default value.</param>
    /// <returns><see langword="true" /> when the operation completed successfully.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="execution" /> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="execution" /> belongs to another plan.</exception>
    public bool TryGetResult(GitLabBatchExecution execution, out TResult? result)
    {
        GitLabBatchPlan.BatchOperationState state = GetState(execution);
        if (state.Status == GitLabBatchOperationStatus.Succeeded)
        {
            result = (TResult)state.Result!;
            return true;
        }

        result = default;
        return false;
    }
}