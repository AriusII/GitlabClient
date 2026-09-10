using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Batching;

/// <summary>
///     A finite set of deferred, independently executable GitLab operations with bounded asynchronous concurrency.
/// </summary>
/// <remarks>
///     <para>
///         Adding an operation never invokes it. <see cref="ExecuteAsync" /> starts at most
///         <see cref="GitLabBatchOptions.MaxConcurrency" /> operations at once and returns all independently
///         observed outcomes for explicit consumption or mapper composition.
///     </para>
///     <para>
///         A plan is single-use. This prevents an accidental replay of a mutation when callers inspect its result
///         more than once. The plan does not merge HTTP requests or replace GitLab's endpoint-specific batch APIs.
///     </para>
/// </remarks>
public sealed class GitLabBatchPlan
{
    private readonly object _gate = new();
    private readonly List<BatchOperationState> _operations = [];
    private readonly GitLabBatchOptions _options;
    private bool _executionStarted;

    internal GitLabBatchPlan(GitLabBatchOptions? options)
    {
        _options = options ?? new GitLabBatchOptions();

        if (_options.MaxConcurrency < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), _options.MaxConcurrency,
                "MaxConcurrency must be greater than zero.");
        }

        if (!Enum.IsDefined(_options.FailureMode))
        {
            throw new ArgumentOutOfRangeException(nameof(options), _options.FailureMode,
                "FailureMode must be a defined GitLabBatchFailureMode value.");
        }
    }

    /// <summary>Adds a deferred operation that produces a typed result when the plan executes.</summary>
    /// <typeparam name="TResult">The result produced by <paramref name="operation" />.</typeparam>
    /// <param name="operation">The operation to invoke only during <see cref="ExecuteAsync" />.</param>
    /// <returns>A typed handle for reading this operation's result from the completed execution.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Execution has already started for this single-use plan.</exception>
    public GitLabBatchOperation<TResult> Add<TResult>(Func<CancellationToken, Task<TResult>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        lock (_gate)
        {
            ThrowIfExecutionStarted();

            int index = _operations.Count;
            _operations.Add(new ResultBatchOperationState<TResult>(operation));
            return new GitLabBatchOperation<TResult>(this, index);
        }
    }

    /// <summary>Adds a deferred operation that has no result value.</summary>
    /// <param name="operation">The operation to invoke only during <see cref="ExecuteAsync" />.</param>
    /// <returns>A handle for inspecting this operation's outcome.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Execution has already started for this single-use plan.</exception>
    public GitLabBatchOperation Add(Func<CancellationToken, Task> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        lock (_gate)
        {
            ThrowIfExecutionStarted();

            int index = _operations.Count;
            _operations.Add(new VoidBatchOperationState(operation));
            return new GitLabBatchOperation(this, index);
        }
    }

    /// <summary>
    ///     Executes every registered operation with bounded concurrency and returns their individual outcomes.
    /// </summary>
    /// <remarks>
    ///     The caller token is propagated to every started operation and always causes this method to complete as
    ///     canceled. In <see cref="GitLabBatchFailureMode.FailFast" /> mode, internally canceled work is represented
    ///     in the returned execution; it does not hide the original operation failure.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Execution has already started for this single-use plan.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken" /> was canceled.</exception>
    public Task<GitLabBatchExecution> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        BatchOperationState[] operations;
        lock (_gate)
        {
            ThrowIfExecutionStarted();
            _executionStarted = true;
            operations = [.. _operations];
        }

        cancellationToken.ThrowIfCancellationRequested();

        return operations.Length == 0
            ? Task.FromResult(new GitLabBatchExecution(this, operations))
            : ExecuteCoreAsync(operations, cancellationToken);
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Each independent SDK operation must retain its own exception as a batch outcome.")]
    private async Task<GitLabBatchExecution> ExecuteCoreAsync(
        BatchOperationState[] operations,
        CancellationToken cancellationToken)
    {
        using CancellationTokenSource? failureCancellationSource =
            _options.FailureMode == GitLabBatchFailureMode.FailFast
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : null;
        CancellationToken effectiveCancellationToken = failureCancellationSource?.Token ?? cancellationToken;
        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = _options.MaxConcurrency, CancellationToken = effectiveCancellationToken
        };

        try
        {
            await Parallel.ForEachAsync(operations, parallelOptions, async (operation, operationCancellationToken) =>
            {
                try
                {
                    object? result = await operation.ExecuteAsync(operationCancellationToken).ConfigureAwait(false);
                    operation.SetSucceeded(result);
                }
                catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
                {
                    operation.SetCanceled(exception);
                    throw;
                }
                catch (OperationCanceledException exception)
                {
                    operation.SetCanceled(exception);
                    if (failureCancellationSource is not null)
                    {
                        await failureCancellationSource.CancelAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    operation.SetFailed(exception);
                    if (failureCancellationSource is not null)
                    {
                        await failureCancellationSource.CancelAsync().ConfigureAwait(false);
                    }
                }
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (failureCancellationSource is { IsCancellationRequested: true })
        {
            // A fail-fast request cancels the Parallel.ForEachAsync scheduling loop. Individual operation states
            // retain the fault/cancellation that caused it, while unstarted operations remain NotStarted.
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new GitLabBatchExecution(this, operations);
    }

    private void ThrowIfExecutionStarted()
    {
        if (_executionStarted)
        {
            throw new InvalidOperationException("A GitLab batch plan can be executed only once.");
        }
    }

    internal abstract class BatchOperationState
    {
        public GitLabBatchOperationStatus Status { get; private set; }

        public object? Result { get; private set; }

        public Exception? Exception { get; private set; }

        public abstract Task<object?> ExecuteAsync(CancellationToken cancellationToken);

        public void SetSucceeded(object? result)
        {
            Result = result;
            Status = GitLabBatchOperationStatus.Succeeded;
        }

        public void SetFailed(Exception exception)
        {
            Exception = exception;
            Status = GitLabBatchOperationStatus.Failed;
        }

        public void SetCanceled(OperationCanceledException exception)
        {
            Exception = exception;
            Status = GitLabBatchOperationStatus.Canceled;
        }
    }

    private sealed class ResultBatchOperationState<TResult>(Func<CancellationToken, Task<TResult>> operation)
        : BatchOperationState
    {
        public override async Task<object?> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class VoidBatchOperationState(Func<CancellationToken, Task> operation) : BatchOperationState
    {
        public override async Task<object?> ExecuteAsync(CancellationToken cancellationToken)
        {
            await operation(cancellationToken).ConfigureAwait(false);
            return null;
        }
    }
}