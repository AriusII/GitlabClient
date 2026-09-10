using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     A user's status - the emoji, message and availability shown beside their name - as returned by
///     <c>GET /user/status</c>, <c>PUT /user/status</c>, <c>PATCH /user/status</c> and
///     <c>GET /users/:user_id/status</c>.
///     <para>
///         A user who has never set a status still gets a <c>200</c> here, with every member below
///         <see langword="null" /> or empty, rather than a <c>404</c>.
///     </para>
/// </summary>
public sealed record GitLabUserStatus
{
    /// <summary>
    ///     The status emoji by name, without colons - <c>coffee</c>, <c>palm_tree</c>. GitLab substitutes
    ///     <c>speech_balloon</c> when a message was set without one.
    /// </summary>
    public string? Emoji { get; init; }

    /// <summary>The status message as the user typed it, Markdown and emoji shorthand included.</summary>
    public string? Message { get; init; }

    /// <summary>
    ///     <see cref="Message" /> rendered to HTML by GitLab. It is sanitized server-side, but it is HTML:
    ///     do not concatenate it into a page without knowing that.
    /// </summary>
    public string? MessageHtml { get; init; }

    /// <summary>
    ///     Whether the user is marked busy - <c>not_set</c> or <c>busy</c>. Left as a <c>string</c> because
    ///     the spec declares it one with no enumeration to bind to; a value outside that pair would
    ///     otherwise be a deserialization failure rather than a value to read.
    /// </summary>
    public string? Availability { get; init; }

    /// <summary>
    ///     When GitLab will clear the status automatically, or <see langword="null" /> when it stands until
    ///     the user changes it. Set indirectly, through
    ///     <see cref="SetUserStatusRequest.ClearStatusAfter" />.
    /// </summary>
    public DateTimeOffset? ClearStatusAt { get; init; }
}