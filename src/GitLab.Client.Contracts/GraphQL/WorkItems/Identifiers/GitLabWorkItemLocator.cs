namespace GitLab.Client.GraphQL.WorkItems.Identifiers;

/// <summary>
///     Identifies a work item by its namespace path and namespace-local internal ID (IID).
/// </summary>
/// <remarks>
///     This is intentionally distinct from <see cref="GitLabGraphQLGlobalId" />. A locator is suitable for the
///     <c>namespace(fullPath: …) { workItem(iid: …) }</c> query; mutations require a global ID instead.
/// </remarks>
public sealed record GitLabWorkItemLocator
{
    /// <summary>Initializes a validated namespace/IID locator.</summary>
    /// <param name="namespacePath">The full project or group namespace path.</param>
    /// <param name="iid">The positive namespace-local work-item IID.</param>
    /// <exception cref="ArgumentException"><paramref name="namespacePath" /> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="iid" /> is not positive.</exception>
    public GitLabWorkItemLocator(string namespacePath, long iid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namespacePath);

        if (iid <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iid), iid, "A work-item IID must be positive.");
        }

        NamespacePath = namespacePath;
        Iid = iid;
    }

    /// <summary>The full path of the project or group namespace containing the work item.</summary>
    public string NamespacePath { get; }

    /// <summary>The positive internal ID, unique only within <see cref="NamespacePath" />.</summary>
    public long Iid { get; }
}