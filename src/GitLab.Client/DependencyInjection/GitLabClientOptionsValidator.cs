using Microsoft.Extensions.Options;

namespace GitLab.Client.DependencyInjection;

/// <summary>
///     Hand-written in place of <c>DataAnnotations</c> validation, which relies on reflection
///     and is not Native-AOT friendly.
/// </summary>
/// <remarks>
///     Every rule here exists because the corresponding value would otherwise blow up somewhere far less
///     diagnosable — inside the <c>AddHttpClient</c> configure delegate, or once per request from the
///     authentication handler. Failures are collected rather than returned one at a time so a consumer with
///     three bad settings fixes them in one pass instead of three restarts.
/// </remarks>
internal sealed class GitLabClientOptionsValidator : IValidateOptions<GitLabClientOptions>
{
    public ValidateOptionsResult Validate(string? name, GitLabClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (name is not null && !string.Equals(name, Options.DefaultName, StringComparison.Ordinal))
        {
            // Named GitLab instances are not supported yet. Skipping (rather than failing, or silently
            // validating someone else's options as if they were ours) keeps that door open.
            return ValidateOptionsResult.Skip;
        }

        List<string> failures = [];

        ValidateAccessToken(options, failures);
        ValidateBaseAddress(options, failures);

        if (!Enum.IsDefined(options.AuthenticationMode))
        {
            failures.Add(
                $"{nameof(GitLabClientOptions.AuthenticationMode)} '{options.AuthenticationMode}' is not a defined value.");
        }

        ValidateTimeout(options, failures);
        ValidateUserAgent(options, failures);

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateAccessToken(GitLabClientOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.AccessToken))
        {
            failures.Add($"{nameof(GitLabClientOptions.AccessToken)} must be provided.");
            return;
        }

        // The token is written with TryAddWithoutValidation on every request, which performs no value
        // validation at all; a stray newline picked up from a config file or a CI variable would be a
        // request-smuggling vector rather than a formatting nuisance.
        if (options.AccessToken.AsSpan().ContainsAny('\r', '\n', '\0'))
        {
            failures.Add(
                $"{nameof(GitLabClientOptions.AccessToken)} must not contain carriage return, line feed or null characters.");
        }
    }

    private static void ValidateBaseAddress(GitLabClientOptions options, List<string> failures)
    {
        if (options.BaseAddress is null)
        {
            // The property is non-nullable, but `BaseAddress = null!` compiles and would otherwise make the
            // validator itself throw NullReferenceException.
            failures.Add($"{nameof(GitLabClientOptions.BaseAddress)} must not be null.");
            return;
        }

        // AbsolutePath, not OriginalString: the raw input can carry a query or fragment, which would make the
        // trailing-slash check pass or fail for entirely the wrong reason.
        if (!options.BaseAddress.IsAbsoluteUri
            || (options.BaseAddress.Scheme != Uri.UriSchemeHttps && options.BaseAddress.Scheme != Uri.UriSchemeHttp)
            || !options.BaseAddress.AbsolutePath.EndsWith('/'))
        {
            failures.Add(
                $"{nameof(GitLabClientOptions.BaseAddress)} must be an absolute http(s) URI whose path ends with '/', e.g. 'https://gitlab.example.com/api/v4/'.");
        }
    }

    private static void ValidateTimeout(GitLabClientOptions options, List<string> failures)
    {
        if (options.Timeout == Timeout.InfiniteTimeSpan)
        {
            return;
        }

        // These are exactly the bounds HttpClient.Timeout enforces; catching them here turns an
        // ArgumentOutOfRangeException thrown from inside a DI factory into a startup message.
        if (options.Timeout <= TimeSpan.Zero || options.Timeout.TotalMilliseconds > int.MaxValue)
        {
            failures.Add(
                $"{nameof(GitLabClientOptions.Timeout)} must be a positive value below {int.MaxValue} milliseconds, or Timeout.InfiniteTimeSpan.");
        }
    }

    private static void ValidateUserAgent(GitLabClientOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.UserAgent) || !CanParseUserAgent(options.UserAgent))
        {
            failures.Add(
                $"{nameof(GitLabClientOptions.UserAgent)} must be a valid User-Agent product token list, e.g. 'MyApp/1.0'.");
        }
    }

    private static bool CanParseUserAgent(string userAgent)
    {
        // Validated with the very parser AddGitLabClient uses (DefaultRequestHeaders.UserAgent.ParseAdd),
        // which throws FormatException on anything that is not a product/comment list — "My App 1.0" being
        // precisely the shape a user types by hand.
        using HttpRequestMessage probe = new();
        return probe.Headers.UserAgent.TryParseAdd(userAgent);
    }
}