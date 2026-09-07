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

    private readonly StringBuilder _path;
    private bool _hasQuery;

    private GitLabRouteBuilder(string root)
    {
        _path = new StringBuilder(root, 64);
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
        _path.Append('/').Append(id.ToString(CultureInfo.InvariantCulture));
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
            AppendQuerySeparator().Append(name).Append('=').Append(notNull.ToString(CultureInfo.InvariantCulture));
        }

        return this;
    }

    public GitLabRouteBuilder Query(string name, long? value)
    {
        if (value is { } notNull)
        {
            AppendQuerySeparator().Append(name).Append('=').Append(notNull.ToString(CultureInfo.InvariantCulture));
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
            AppendQuerySeparator().Append(name).Append('=')
                .Append(notNull.ToUniversalTime().ToString(DateTimeQueryFormat, CultureInfo.InvariantCulture));
        }

        return this;
    }

    public GitLabRouteBuilder Query(string name, DateOnly? value)
    {
        if (value is { } notNull)
        {
            AppendQuerySeparator().Append(name).Append('=')
                .Append(notNull.ToString(DateQueryFormat, CultureInfo.InvariantCulture));
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

            _path.Append(values[index].ToString(CultureInfo.InvariantCulture));
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
            AppendQuerySeparator().Append(name).Append("[]=")
                .Append(values[index].ToString(CultureInfo.InvariantCulture));
        }

        return this;
    }

    public Uri Build()
    {
        return new Uri(_path.ToString(), UriKind.Relative);
    }

    private StringBuilder AppendQuerySeparator()
    {
        _path.Append(_hasQuery ? '&' : '?');
        _hasQuery = true;
        return _path;
    }
}