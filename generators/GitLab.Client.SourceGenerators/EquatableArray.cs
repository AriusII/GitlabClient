namespace GitLab.Client.SourceGenerators;

/// <summary>
///     A value-equatable sequence. Both <c>T[]</c> and <c>ImmutableArray&lt;T&gt;</c> compare by
///     reference, so putting either one in an incremental-generator model silently defeats the pipeline
///     cache: the driver compares the previous and current model with
///     <see cref="EqualityComparer{T}.Default" />, sees "changed", and re-runs every downstream node on
///     every keystroke. This compares element-wise instead.
///     <para>
///         It deliberately does not implement <see cref="IEnumerable{T}" /> - <c>foreach</c> binds to the
///         public <see cref="GetEnumerator" /> by pattern, and staying off the interface keeps this out of
///         the collection-naming analyzer rules while avoiding a boxing allocation per enumeration.
///     </para>
/// </summary>
/// <typeparam name="T">Element type; must compare by value for the wrapper to do the same.</typeparam>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    private readonly T[]? _items;

    public EquatableArray(T[]? items)
    {
        _items = items;
    }

    public static EquatableArray<T> Empty => new(null);

    public int Length => _items is null ? 0 : _items.Length;

    private T[] Items => _items ?? Array.Empty<T>();

    public T this[int index] => Items[index];

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
    {
        return !left.Equals(right);
    }

    public static EquatableArray<T> From(List<T> items)
    {
        return new EquatableArray<T>(items.Count == 0 ? null : items.ToArray());
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)Items).GetEnumerator();
    }

    public bool Equals(EquatableArray<T> other)
    {
        T[] left = Items;
        T[] right = other.Items;

        if (left.Length != right.Length)
        {
            return false;
        }

        for (int index = 0; index < left.Length; index++)
        {
            if (!left[index].Equals(right[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            foreach (T item in Items)
            {
                hash = (hash * 31) + (item is null ? 0 : item.GetHashCode());
            }

            return hash;
        }
    }
}