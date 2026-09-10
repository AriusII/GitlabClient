using System.Net.Http.Headers;

namespace GitLab.Client.Infrastructure.Pagination;

/// <summary>
///     Parses the RFC 8288 (originally RFC 5988) <c>Link</c> response header GitLab uses to advertise
///     pagination cursors. Allocation-free apart from the one string handed back to the caller.
/// </summary>
internal static class GitLabLinkHeader
{
    private const string LinkHeaderName = "Link";
    private const string RelationParameterName = "rel";
    private const string NextRelationType = "next";

    public static string? GetNextPageUrl(HttpResponseHeaders headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        // The NonValidated view is a struct with a struct enumerator. The IEnumerable<string>-returning
        // TryGetValues materialises a string[] and boxes an array enumerator on every single response.
        if (!headers.NonValidated.TryGetValues(LinkHeaderName, out HeaderStringValues values))
        {
            return null;
        }

        foreach (string headerValue in values)
        {
            ReadOnlySpan<char> nextPageUrl = FindNextPageUrl(headerValue);
            if (!nextPageUrl.IsEmpty)
            {
                return nextPageUrl.ToString();
            }
        }

        return null;
    }

    /// <summary>
    ///     Scans one <c>Link</c> field value for the <c>rel="next"</c> link-value and returns its URI-Reference
    ///     without the angle brackets. Commas and semicolons inside the brackets or inside a quoted parameter
    ///     value are not separators, which is exactly why this cannot be a pair of <c>string.Split</c> calls:
    ///     a URI containing a comma silently truncated the enumeration.
    /// </summary>
    private static ReadOnlySpan<char> FindNextPageUrl(ReadOnlySpan<char> header)
    {
        int index = 0;

        while (index < header.Length)
        {
            SkipSeparators(header, ref index);
            if (index >= header.Length)
            {
                break;
            }

            if (header[index] != '<')
            {
                // Malformed link-value: skip it rather than abandoning the whole header.
                SkipToNextLinkValue(header, ref index);
                continue;
            }

            int closingBracket = header[index..].IndexOf('>');
            if (closingBracket < 0)
            {
                break;
            }

            closingBracket += index;
            ReadOnlySpan<char> url = header[(index + 1)..closingBracket];
            index = closingBracket + 1;

            bool isNextRelation = false;
            while (index < header.Length && header[index] != ',')
            {
                if (header[index] != ';')
                {
                    index++;
                    continue;
                }

                index++;
                if (ReadParameterIsRelNext(header, ref index))
                {
                    isNextRelation = true;
                }
            }

            if (isNextRelation)
            {
                return url;
            }
        }

        return default;
    }

    private static bool ReadParameterIsRelNext(ReadOnlySpan<char> header, ref int index)
    {
        SkipWhitespace(header, ref index);

        int nameStart = index;
        while (index < header.Length && header[index] != '=' && header[index] != ';' && header[index] != ',')
        {
            index++;
        }

        ReadOnlySpan<char> name = header[nameStart..index].TrimEnd();
        if (index >= header.Length || header[index] != '=')
        {
            return false;
        }

        index++;
        SkipWhitespace(header, ref index);

        ReadOnlySpan<char> value = index < header.Length && header[index] == '"'
            ? ReadQuotedString(header, ref index)
            : ReadToken(header, ref index);

        return name.Equals(RelationParameterName, StringComparison.OrdinalIgnoreCase)
               && ContainsNextRelationType(value);
    }

    private static ReadOnlySpan<char> ReadQuotedString(ReadOnlySpan<char> header, ref int index)
    {
        index++;
        int valueStart = index;

        while (index < header.Length && header[index] != '"')
        {
            // RFC 9110 quoted-pair: a backslash escapes the next character, a closing quote included.
            index += header[index] == '\\' && index + 1 < header.Length ? 2 : 1;
        }

        // Escapes are left undecoded: relation types are tokens, so this never matters for rel.
        ReadOnlySpan<char> value = header[valueStart..Math.Min(index, header.Length)];
        if (index < header.Length)
        {
            index++;
        }

        return value;
    }

    private static ReadOnlySpan<char> ReadToken(ReadOnlySpan<char> header, ref int index)
    {
        int valueStart = index;
        while (index < header.Length && header[index] != ';' && header[index] != ',')
        {
            index++;
        }

        return header[valueStart..index].TrimEnd();
    }

    private static bool ContainsNextRelationType(ReadOnlySpan<char> relationTypes)
    {
        // RFC 8288 allows a space-separated list of relation types, e.g. rel="next last".
        foreach (Range relationType in relationTypes.Split(' '))
        {
            if (relationTypes[relationType].Equals(NextRelationType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void SkipSeparators(ReadOnlySpan<char> header, ref int index)
    {
        while (index < header.Length && (header[index] == ',' || char.IsWhiteSpace(header[index])))
        {
            index++;
        }
    }

    private static void SkipToNextLinkValue(ReadOnlySpan<char> header, ref int index)
    {
        while (index < header.Length && header[index] != ',')
        {
            index++;
        }
    }

    private static void SkipWhitespace(ReadOnlySpan<char> header, ref int index)
    {
        while (index < header.Length && char.IsWhiteSpace(header[index]))
        {
            index++;
        }
    }
}