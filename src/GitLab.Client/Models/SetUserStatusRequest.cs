namespace GitLab.Client.Models;

/// <summary>
///     Request body shared by <c>PUT /user/status</c> and <c>PATCH /user/status</c> - GitLab declares one
///     schema for both.
///     <para>
///         The verb, not the body, decides what an omitted member means. <c>PUT</c> ("set") nullifies
///         everything not passed, so a request carrying only a message clears the emoji. <c>PATCH</c>
///         ("update") ignores what is not passed and leaves it alone. Pick the method accordingly:
///         <c>SetStatusAsync</c> replaces the status, <c>UpdateStatusAsync</c> amends it.
///     </para>
/// </summary>
public sealed record SetUserStatusRequest
{
    /// <summary>
    ///     The status emoji by name, without colons - <c>coffee</c>, not the colon-wrapped shorthand.
    ///     GitLab falls back to <c>speech_balloon</c> when a message is set without one.
    /// </summary>
    public string? Emoji { get; init; }

    /// <summary>
    ///     The status message. Markdown and emoji shorthand are rendered into
    ///     <see cref="GitLabUserStatus.MessageHtml" />.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    ///     Whether to mark the user busy - <c>not_set</c> or <c>busy</c>. A bare string in the spec, with no
    ///     enumeration.
    /// </summary>
    public string? Availability { get; init; }

    /// <summary>
    ///     How long the status stands before GitLab clears it. Omitted means the status has no expiry;
    ///     GitLab turns this into the absolute <see cref="GitLabUserStatus.ClearStatusAt" /> it answers with.
    /// </summary>
    public GitLabUserStatusClearAfter? ClearStatusAfter { get; init; }
}