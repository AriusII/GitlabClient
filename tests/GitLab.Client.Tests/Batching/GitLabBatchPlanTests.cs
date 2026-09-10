using GitLab.Client.Batching;

namespace GitLab.Client.Tests.Batching;

public sealed class GitLabBatchPlanTests
{
    [Fact]
    public async Task ExecuteAsync_DefersOperationsUntilThePlanIsExecuted()
    {
        GitLabBatchPlan plan = CreatePlan();
        int invocationCount = 0;
        GitLabBatchOperation<int> operation = plan.Add(_ =>
        {
            invocationCount++;
            return Task.FromResult(42);
        });

        Assert.Equal(0, invocationCount);

        GitLabBatchExecution execution = await plan.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, invocationCount);
        Assert.Equal(42, operation.GetResult(execution));
        Assert.True(execution.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_PreservesOperationHandlesWhenOperationsCompleteOutOfOrder()
    {
        GitLabBatchPlan plan = CreatePlan(3);
        TaskCompletionSource<bool>[] entered = CreateSignals(3);
        TaskCompletionSource<int>[] completions =
        [
            CreateSignal<int>(),
            CreateSignal<int>(),
            CreateSignal<int>()
        ];

        GitLabBatchOperation<int>[] operations =
        [
            plan.Add(_ => EnterAndReturnAsync(entered[0], completions[0])),
            plan.Add(_ => EnterAndReturnAsync(entered[1], completions[1])),
            plan.Add(_ => EnterAndReturnAsync(entered[2], completions[2]))
        ];

        Task<GitLabBatchExecution> executionTask = plan.ExecuteAsync(TestContext.Current.CancellationToken);
        await Task.WhenAll(entered.Select(static signal => signal.Task))
            .WaitAsync(TestContext.Current.CancellationToken);

        completions[2].SetResult(30);
        completions[0].SetResult(10);
        completions[1].SetResult(20);

        GitLabBatchExecution execution = await executionTask.ConfigureAwait(true);

        Assert.Equal(10, operations[0].GetResult(execution));
        Assert.Equal(20, operations[1].GetResult(execution));
        Assert.Equal(30, operations[2].GetResult(execution));
    }

    [Fact]
    public async Task ExecuteAsync_NeverExceedsConfiguredMaxConcurrency()
    {
        const int maxConcurrency = 3;
        const int operationCount = 8;
        GitLabBatchPlan plan = CreatePlan(maxConcurrency);
        TaskCompletionSource<bool> initialWindowEntered = CreateSignal<bool>();
        TaskCompletionSource<bool> releaseOperations = CreateSignal<bool>();
        int activeOperations = 0;
        int peakConcurrency = 0;
        int startedOperations = 0;

        List<GitLabBatchOperation<int>> operations = new(operationCount);
        for (int index = 0; index < operationCount; index++)
        {
            int result = index;
            operations.Add(plan.Add(async cancellationToken =>
            {
                int active = Interlocked.Increment(ref activeOperations);
                TrackMaximum(ref peakConcurrency, active);
                if (Interlocked.Increment(ref startedOperations) == maxConcurrency)
                {
                    initialWindowEntered.TrySetResult(true);
                }

                try
                {
                    await releaseOperations.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                    return result;
                }
                finally
                {
                    Interlocked.Decrement(ref activeOperations);
                }
            }));
        }

        Task<GitLabBatchExecution> executionTask = plan.ExecuteAsync(TestContext.Current.CancellationToken);
        await initialWindowEntered.Task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(maxConcurrency, Volatile.Read(ref startedOperations));
        Assert.Equal(maxConcurrency, Volatile.Read(ref peakConcurrency));

        releaseOperations.SetResult(true);
        GitLabBatchExecution execution = await executionTask.ConfigureAwait(true);

        Assert.Equal(operationCount, execution.SucceededCount);
        Assert.Equal(operationCount, operations.Select(operation => operation.GetResult(execution)).Distinct().Count());
        Assert.Equal(maxConcurrency, Volatile.Read(ref peakConcurrency));
    }

    [Fact]
    public async Task ExecuteAsync_CollectsIndependentFailuresAndCancellation()
    {
        InvalidOperationException failure = new("expected");
        using CancellationTokenSource independentlyCanceled = new();
        await independentlyCanceled.CancelAsync();
        GitLabBatchPlan plan = CreatePlan(3);
        GitLabBatchOperation<int> success = plan.Add(_ => Task.FromResult(7));
        GitLabBatchOperation<int> faulted = plan.Add(_ => Task.FromException<int>(failure));
        GitLabBatchOperation<int> canceled = plan.Add(_ => Task.FromCanceled<int>(independentlyCanceled.Token));
        GitLabBatchOperation completed = plan.Add(_ => Task.CompletedTask);

        GitLabBatchExecution execution = await plan.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Equal(4, execution.Count);
        Assert.Equal(2, execution.SucceededCount);
        Assert.Equal(1, execution.FailedCount);
        Assert.Equal(1, execution.CanceledCount);
        Assert.Equal(0, execution.NotStartedCount);
        Assert.False(execution.IsSuccess);
        Assert.Same(failure, Assert.Single(execution.Failures));
        Assert.Equal(7, success.GetResult(execution));
        Assert.True(success.TryGetResult(execution, out int successfulResult));
        Assert.Equal(7, successfulResult);
        Assert.Equal(GitLabBatchOperationStatus.Failed, faulted.GetStatus(execution));
        Assert.Same(failure, faulted.GetException(execution));
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => faulted.GetResult(execution)));
        Assert.False(faulted.TryGetResult(execution, out _));
        Assert.Equal(GitLabBatchOperationStatus.Canceled, canceled.GetStatus(execution));
        Assert.IsType<TaskCanceledException>(canceled.GetException(execution));
        completed.ThrowIfFailed(execution);

        AggregateException aggregate = Assert.Throws<AggregateException>(execution.ThrowIfFailed);
        Assert.Same(failure, Assert.Single(aggregate.InnerExceptions));
    }

    [Fact]
    public async Task ExecuteAsync_FailFastStopsSchedulingOperationsThatHaveNotStarted()
    {
        InvalidOperationException failure = new("expected");
        GitLabBatchPlan plan = CreatePlan(1, GitLabBatchFailureMode.FailFast);
        GitLabBatchOperation<int> faulted = plan.Add(_ => Task.FromException<int>(failure));
        GitLabBatchOperation<int> notStarted = plan.Add<int>(_ =>
            throw new InvalidOperationException("The fail-fast plan must not invoke this operation."));

        GitLabBatchExecution execution = await plan.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Equal(GitLabBatchOperationStatus.Failed, faulted.GetStatus(execution));
        Assert.Equal(GitLabBatchOperationStatus.NotStarted, notStarted.GetStatus(execution));
        Assert.Equal(1, execution.FailedCount);
        Assert.Equal(1, execution.NotStartedCount);
        Assert.Same(failure, Assert.Single(execution.Failures));
    }

    [Fact]
    public async Task ExecuteAsync_FailFastCancelsAnAlreadyRunningOperation()
    {
        InvalidOperationException failure = new("expected");
        TaskCompletionSource<bool> secondOperationStarted = CreateSignal<bool>();
        TaskCompletionSource<bool> cancellationObserved = CreateSignal<bool>();
        TaskCompletionSource<bool> neverCompleted = CreateSignal<bool>();
        GitLabBatchPlan plan = CreatePlan(2, GitLabBatchFailureMode.FailFast);
        GitLabBatchOperation<int> faulted = plan.Add<int>(async cancellationToken =>
        {
            await secondOperationStarted.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            throw failure;
        });
        GitLabBatchOperation<int> canceled = plan.Add(async cancellationToken =>
        {
            secondOperationStarted.TrySetResult(true);
            try
            {
                await neverCompleted.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                return 2;
            }
            catch (OperationCanceledException)
            {
                cancellationObserved.TrySetResult(true);
                throw;
            }
        });

        Task<GitLabBatchExecution> executionTask = plan.ExecuteAsync(TestContext.Current.CancellationToken);
        await cancellationObserved.Task.WaitAsync(TestContext.Current.CancellationToken);
        GitLabBatchExecution execution = await executionTask.ConfigureAwait(true);

        Assert.Equal(GitLabBatchOperationStatus.Failed, faulted.GetStatus(execution));
        Assert.Equal(GitLabBatchOperationStatus.Canceled, canceled.GetStatus(execution));
        Assert.Same(failure, Assert.Single(execution.Failures));
    }

    [Fact]
    public async Task ThrowIfFailed_PreservesFailureRegistrationOrder()
    {
        InvalidOperationException firstFailure = new("first");
        ArgumentException secondFailure = new("second");
        GitLabBatchPlan plan = CreatePlan(1);
        plan.Add(_ => Task.FromException<int>(firstFailure));
        plan.Add(_ => Task.FromException<int>(secondFailure));

        GitLabBatchExecution execution = await plan.ExecuteAsync(TestContext.Current.CancellationToken);
        AggregateException aggregate = Assert.Throws<AggregateException>(execution.ThrowIfFailed);

        Assert.Equal([firstFailure, secondFailure], aggregate.InnerExceptions);
    }

    [Fact]
    public async Task ExecuteAsync_CallerCancellationBeforeStartInvokesNoOperation()
    {
        using CancellationTokenSource cancellationSource = new();
        await cancellationSource.CancelAsync();
        GitLabBatchPlan plan = CreatePlan();
        int invocationCount = 0;
        plan.Add(_ =>
        {
            invocationCount++;
            return Task.FromResult(1);
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => plan.ExecuteAsync(cancellationSource.Token));
        Assert.Equal(0, invocationCount);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCallerCancellationToAnInFlightOperation()
    {
        using CancellationTokenSource cancellationSource = new();
        TaskCompletionSource<bool> entered = CreateSignal<bool>();
        TaskCompletionSource<bool> cancellationObserved = CreateSignal<bool>();
        TaskCompletionSource<bool> neverCompleted = CreateSignal<bool>();
        GitLabBatchPlan plan = CreatePlan();
        plan.Add(async cancellationToken =>
        {
            entered.SetResult(true);
            try
            {
                await neverCompleted.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                return 1;
            }
            catch (OperationCanceledException)
            {
                cancellationObserved.TrySetResult(true);
                throw;
            }
        });

        Task<GitLabBatchExecution> executionTask = plan.ExecuteAsync(cancellationSource.Token);
        await entered.Task.WaitAsync(TestContext.Current.CancellationToken);
        await cancellationSource.CancelAsync();
        await cancellationObserved.Task.WaitAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => executionTask);
    }

    [Fact]
    public async Task Execution_RejectsAHandleFromAnotherPlan()
    {
        GitLabBatchPlan firstPlan = CreatePlan();
        GitLabBatchPlan secondPlan = CreatePlan();
        GitLabBatchOperation<int> firstOperation = firstPlan.Add(_ => Task.FromResult(1));
        secondPlan.Add(_ => Task.FromResult(2));

        GitLabBatchExecution secondExecution = await secondPlan.ExecuteAsync(TestContext.Current.CancellationToken);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => firstOperation.GetResult(secondExecution));
        Assert.Equal("execution", exception.ParamName);
    }

    [Fact]
    public async Task Plan_IsSingleUseAndCannotBeChangedAfterExecutionStarts()
    {
        GitLabBatchPlan plan = CreatePlan();
        plan.Add(_ => Task.FromResult(1));

        await plan.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Throws<InvalidOperationException>(() => plan.Add(_ => Task.FromResult(2)));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            plan.ExecuteAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_EmptyPlanCompletesSuccessfully()
    {
        GitLabBatchExecution execution = await CreatePlan().ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, execution.Count);
        Assert.True(execution.IsSuccess);
        Assert.Empty(execution.Failures);
    }

    [Fact]
    public void Create_RejectsInvalidOptionsAndNullOperations()
    {
        ArgumentOutOfRangeException concurrency = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreatePlan(0));
        ArgumentOutOfRangeException failureMode = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GitLabBatchPlan(new GitLabBatchOptions { FailureMode = (GitLabBatchFailureMode)99 }));
        GitLabBatchPlan plan = CreatePlan();

        Assert.Equal("options", concurrency.ParamName);
        Assert.Equal("options", failureMode.ParamName);
        Assert.Throws<ArgumentNullException>(() => plan.Add<int>(null!));
        Assert.Throws<ArgumentNullException>(() => plan.Add(null!));
    }

    private static GitLabBatchPlan CreatePlan(
        int maxConcurrency = 4,
        GitLabBatchFailureMode failureMode = GitLabBatchFailureMode.CollectAll)
    {
        return new GitLabBatchPlan(
            new GitLabBatchOptions { MaxConcurrency = maxConcurrency, FailureMode = failureMode });
    }

    private static TaskCompletionSource<T> CreateSignal<T>()
    {
        return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static TaskCompletionSource<bool>[] CreateSignals(int count)
    {
        TaskCompletionSource<bool>[] signals = new TaskCompletionSource<bool>[count];
        for (int index = 0; index < signals.Length; index++)
        {
            signals[index] = CreateSignal<bool>();
        }

        return signals;
    }

    private static async Task<int> EnterAndReturnAsync(TaskCompletionSource<bool> entered,
        TaskCompletionSource<int> result)
    {
        entered.TrySetResult(true);
        return await result.Task.ConfigureAwait(false);
    }

    private static void TrackMaximum(ref int maximum, int candidate)
    {
        int observed;
        do
        {
            observed = Volatile.Read(ref maximum);
            if (candidate <= observed)
            {
                return;
            }
        } while (Interlocked.CompareExchange(ref maximum, candidate, observed) != observed);
    }
}