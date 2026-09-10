using System.Globalization;
using System.Text;

using GitLab.Client.Domain;

namespace GitLab.Client.Infrastructure.Routing;

/// <summary>
///     Builds a single GitLab API route, one path segment and query parameter at a time. The one
///     place that knows how to percent-encode a namespaced-path segment and how query strings are
///     assembled — Repositories build every route through this, never by hand-interpolating strings.
///     <para>
///         <c>Segment</c> and <c>Query</c> have deliberately opposite encoding contracts: a
///         <c>Segment</c> overload appends text that is already encoded (which is why
///         <see cref="ProjectId" />/<see cref="GroupId" /> have a <c>ToRouteValue()</c> and a free-text
///         segment must be escaped by the caller), while <c>Query</c> escapes what it is given. That is
///         why there is no <c>Query(string, ProjectId?)</c> overload: routing an already-encoded
///         <c>%2F</c> through <c>Uri.EscapeDataString</c> would send <c>%252F</c>.
///     </para>
/// </summary>
internal sealed class GitLabRouteBuilder
{
    /// <summary>
    ///     GitLab's OpenAPI spec declares every one of its array query parameters as
    ///     <c>style: form, explode: false</c> — a single parameter whose value is a comma-separated
    ///     list — and not one as <c>explode: true</c>. <see cref="QueryRepeated(string, IReadOnlyList{string})" />
    ///     covers the <c>name[]=a&amp;name[]=b</c> form that some GitLab doc pages show instead.
    /// </summary>
    private const char MultiValueSeparator = ',';

    /// <summary>
    ///     GitLab documents its date-time filters as "ISO 8601 YYYY-MM-DDTHH:MM:SSZ". Values are
    ///     normalised to UTC first, so a caller in a non-UTC zone cannot silently shift the filter window.
    /// </summary>
    private const string DateTimeQueryFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

    /// <summary>Equivalent to the "O" specifier for <see cref="DateOnly" />, spelled out so it cannot drift.</summary>
    private const string DateQueryFormat = "yyyy'-'MM'-'dd";

    // A route normally stays well below this, while retaining a multi-megabyte query buffer on a thread-pool
    // thread after one exceptional request is never useful.
    private const int MaximumCachedPathCapacity = 8 * 1024;

    /// <summary>
    ///     A per-thread reusable buffer. A <see cref="GitLabRouteBuilder" /> checks the buffer out of
    ///     this slot while it is building a route and puts it back only from <see cref="Build" />. That
    ///     makes the slot an exception-safe lease: if validation or escaping throws before <c>Build</c>,
    ///     the abandoned builder cannot poison a separate in-use flag for the remainder of the thread's
    ///     lifetime. The next route simply creates a replacement buffer and normal reuse resumes.
    ///     <para>
    ///         A route builder is short-lived and single-use — constructed, chained fluently,
    ///         <see cref="Build" /> called once, then discarded — so one available buffer per thread removes
    ///         the normal per-call allocation from <see cref="Create" />. The slot is never shared across
    ///         threads.
    ///     </para>
    /// </summary>
    [ThreadStatic] private static StringBuilder? t_cachedPath;

    private readonly StringBuilder _path;
    private bool _hasQuery;

    private GitLabRouteBuilder(string root)
    {
        StringBuilder? cached = t_cachedPath;
        if (cached is null)
        {
            _path = new StringBuilder(root, 64);
            return;
        }

        // Check the buffer out before mutating it. A reentrant builder therefore gets its own buffer,
        // and a builder abandoned by an exception leaves no "in use" marker behind on this thread.
        t_cachedPath = null;
        cached.Clear();
        cached.Append(root);
        _path = cached;
    }

    public static GitLabRouteBuilder Create(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        return new GitLabRouteBuilder(root);
    }

    /// <summary>
    ///     Appends a fixed path word from the route template - "repository", "merge_requests", "stop" - exactly
    ///     as given. Never pass caller-supplied text here: use <see cref="Escaped" />, which owns the encoding.
    /// </summary>
    public GitLabRouteBuilder Literal(string pathWord)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pathWord);
        _path.Append('/').Append(pathWord);
        return this;
    }

    /// <summary>
    ///     Appends caller-supplied free text - a branch or tag name, a file path, a ref - percent-encoding it so
    ///     that a value containing '/' stays one path segment. Encoding lives here rather than at the call site
    ///     precisely so it cannot be forgotten: GitLab answers an unescaped "release/1.0" with a 404 against a
    ///     path that does not exist, which reads like a missing resource rather than a client bug.
    /// </summary>
    public GitLabRouteBuilder Escaped(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _path.Append('/').Append(Uri.EscapeDataString(value));
        return this;
    }

    /// <summary>
    ///     Appends one path segment built from a literal template with one or more caller-supplied values
    ///     substituted into it - for the rare route where GitLab packs more than one dynamic value into a
    ///     single "/"-delimited segment instead of chaining separate ones, such as NuGet v2's OData key
    ///     predicate <c>Packages(Id='{package_name}',Version='{package_version}')</c>. Every value is
    ///     percent-encoded independently before substitution, never the composed whole - escaping the
    ///     finished string would also encode the template's own literal characters (the parentheses, the
    ///     quotes), and not escaping the values at all would let one containing <c>/</c> or <c>'</c> break
    ///     out of its slot. <paramref name="template" /> is always a compile-time literal, never
    ///     caller-supplied text - it plays the same role <see cref="Literal" />'s argument does, just with
    ///     placeholders spliced in.
    /// </summary>
    public GitLabRouteBuilder EscapedTemplate(string template, params string[] values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(values);

        object[] escaped = new object[values.Length];
        for (int index = 0; index < values.Length; index++)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(values[index]);
            escaped[index] = Uri.EscapeDataString(values[index]);
        }

        _path.Append('/').AppendFormat(CultureInfo.InvariantCulture, template, escaped);
        return this;
    }

    public GitLabRouteBuilder Segment(ProjectId projectId)
    {
        _path.Append('/').Append(projectId.ToRouteValue());
        return this;
    }

    public GitLabRouteBuilder Segment(GroupId groupId)
    {
        _path.Append('/').Append(groupId.ToRouteValue());
        return this;
    }

    public GitLabRouteBuilder Segment(long id)
    {
        _path.Append('/');
        AppendInvariant(id);
        return this;
    }

    public GitLabRouteBuilder Query(string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            AppendQuerySeparator().Append(name).Append('=').Append(Uri.EscapeDataString(value));
        }

        return this;
    }

    public GitLabRouteBuilder Query(string name, int? value)
    {
        if (value is { } notNull)
        {
            AppendQuerySeparator().Append(name).Append('=');
            AppendInvariant(notNull);
        }

        return this;
    }

    public GitLabRouteBuilder Query(string name, long? value)
    {
        if (value is { } notNull)
        {
            AppendQuerySeparator().Append(name).Append('=');
            AppendInvariant(notNull);
        }

        return this;
    }

    public GitLabRouteBuilder Query(string name, bool? value)
    {
        if (value is { } notNull)
        {
            AppendQuerySeparator().Append(name).Append('=').Append(notNull ? "true" : "false");
        }

        return this;
    }

    public GitLabRouteBuilder Query(string name, DateTimeOffset? value)
    {
        if (value is { } notNull)
        {
            // The formatted value contains only digits, '-', 'T', ':' and 'Z'. ':' is legal unescaped in
            // a query (RFC 3986 pchar), so it is appended raw rather than through Uri.EscapeDataString,
            // which would render it %3A and make every recorded URL in the tests unreadable.
            AppendQuerySeparator().Append(name).Append('=');
            AppendInvariant(notNull);
        }

        return this;
    }

    public GitLabRouteBuilder Query(string name, DateOnly? value)
    {
        if (value is { } notNull)
        {
            AppendQuerySeparator().Append(name).Append('=');
            AppendInvariant(notNull);
        }

        return this;
    }

    /// <summary>Writes <c>name=a,b,c</c>, skipping blank elements and omitting the parameter if none remain.</summary>
    public GitLabRouteBuilder Query(string name, IReadOnlyList<string>? values)
    {
        if (values is null)
        {
            return this;
        }

        bool wroteAny = false;

        for (int index = 0; index < values.Count; index++)
        {
            string? value = values[index];
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (wroteAny)
            {
                _path.Append(MultiValueSeparator);
            }
            else
            {
                AppendQuerySeparator().Append(name).Append('=');
                wroteAny = true;
            }

            // Each element is escaped on its own and the separators are appended literally. Escaping the
            // joined string instead would emit %2C for the separator and — the actual bug — would make an
            // element that legitimately contains a comma indistinguishable from a separator.
            _path.Append(Uri.EscapeDataString(value));
        }

        return this;
    }

    /// <summary>Writes <c>name=1,2,3</c>, omitting the parameter entirely when the list is empty.</summary>
    public GitLabRouteBuilder Query(string name, IReadOnlyList<long>? values)
    {
        if (values is null || values.Count == 0)
        {
            return this;
        }

        AppendQuerySeparator().Append(name).Append('=');

        for (int index = 0; index < values.Count; index++)
        {
            if (index > 0)
            {
                _path.Append(MultiValueSeparator);
            }

            AppendInvariant(values[index]);
        }

        return this;
    }

    /// <summary>Writes <c>name[]=a&amp;name[]=b</c> — the opt-in alternative to the comma-joined default.</summary>
    public GitLabRouteBuilder QueryRepeated(string name, IReadOnlyList<string>? values)
    {
        if (values is null)
        {
            return this;
        }

        for (int index = 0; index < values.Count; index++)
        {
            string? value = values[index];
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            // '[' and ']' are appended raw: they survive new Uri(.., UriKind.Relative) and combination
            // with the HttpClient BaseAddress byte for byte, and GitLab's own docs use that literal form.
            AppendQuerySeparator().Append(name).Append("[]=").Append(Uri.EscapeDataString(value));
        }

        return this;
    }

    /// <summary>Writes <c>name[]=1&amp;name[]=2</c> — the opt-in alternative to the comma-joined default.</summary>
    public GitLabRouteBuilder QueryRepeated(string name, IReadOnlyList<long>? values)
    {
        if (values is null)
        {
            return this;
        }

        for (int index = 0; index < values.Count; index++)
        {
            AppendQuerySeparator().Append(name).Append("[]=");
            AppendInvariant(values[index]);
        }

        return this;
    }

    public Uri Build()
    {
        try
        {
            return new Uri(_path.ToString(), UriKind.Relative);
        }
        finally
        {
            if (_path.Capacity <= MaximumCachedPathCapacity)
            {
                _path.Clear();
                t_cachedPath ??= _path;
            }
            else if (ReferenceEquals(t_cachedPath, _path))
            {
                t_cachedPath = null;
            }
        }
    }

    private StringBuilder AppendQuerySeparator()
    {
        _path.Append(_hasQuery ? '&' : '?');
        _hasQuery = true;
        return _path;
    }

    private void AppendInvariant(int value)
    {
        Span<char> buffer = stackalloc char[11];
        if (!value.TryFormat(buffer, out int written, provider: CultureInfo.InvariantCulture))
        {
            throw new InvalidOperationException("The route integer could not be formatted.");
        }

        _path.Append(buffer[..written]);
    }

    private void AppendInvariant(long value)
    {
        Span<char> buffer = stackalloc char[20];
        if (!value.TryFormat(buffer, out int written, provider: CultureInfo.InvariantCulture))
        {
            throw new InvalidOperationException("The route integer could not be formatted.");
        }

        _path.Append(buffer[..written]);
    }

    /// <summary>
    ///     Appends GitLab's fixed UTC timestamp form without creating the intermediate string that
    ///     <see cref="DateTimeOffset.ToString(string, IFormatProvider)" /> would return. Route building
    ///     happens for every request, so the small stack buffer removes a per-request allocation while
    ///     retaining the wire format pinned by the public query contract.
    /// </summary>
    private void AppendInvariant(DateTimeOffset value)
    {
        Span<char> buffer = stackalloc char[20];
        if (!value.ToUniversalTime().TryFormat(
                buffer,
                out int written,
                DateTimeQueryFormat,
                CultureInfo.InvariantCulture))
        {
            throw new InvalidOperationException("The route timestamp could not be formatted.");
        }

        _path.Append(buffer[..written]);
    }

    /// <summary>
    ///     Appends an ISO calendar date directly into the route buffer. The format is always ten ASCII
    ///     characters, but TryFormat keeps the invariant explicit and avoids an intermediate string.
    /// </summary>
    private void AppendInvariant(DateOnly value)
    {
        Span<char> buffer = stackalloc char[10];
        if (!value.TryFormat(buffer, out int written, DateQueryFormat, CultureInfo.InvariantCulture))
        {
            throw new InvalidOperationException("The route date could not be formatted.");
        }

        _path.Append(buffer[..written]);
    }
}