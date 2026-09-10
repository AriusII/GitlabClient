namespace GitLab.Client.Models.Requests;

/// <summary>
///     The text and boolean fields of <c>PUT /application/appearance</c>, sent as <c>multipart/form-data</c>.
///     <para>
///         The endpoint also declares four independent binary fields - <c>logo</c>, <c>pwa_icon</c>,
///         <c>header_logo</c> and <c>favicon</c> - which GitLab's own documentation sends as separate
///         <c>multipart/form-data</c> requests rather than folding them into the typed request alongside
///         these fields. <see cref="Abstractions.IInstanceClient" /> mirrors that split: this request
///         type backs <see cref="Abstractions.IInstanceClient.UpdateAppearanceAsync" />, and the four
///         images each get their own <c>SetAppearance*Async</c> method.
///     </para>
///     <para>
///         Every member is nullable, and an unset member is omitted from the payload rather than sent
///         as null, so a call leaves fields it does not mention unchanged. Administrators only.
///     </para>
/// </summary>
public sealed record UpdateApplicationAppearanceRequest
{
    /// <summary>Instance title on the sign-in / sign-up page.</summary>
    public string? Title { get; init; }

    /// <summary>Markdown text shown on the sign-in / sign-up page.</summary>
    public string? Description { get; init; }

    /// <summary>Name of the Progressive Web App.</summary>
    public string? PwaName { get; init; }

    /// <summary>Short name for the Progressive Web App.</summary>
    public string? PwaShortName { get; init; }

    /// <summary>An explanation of what the Progressive Web App does.</summary>
    public string? PwaDescription { get; init; }

    /// <summary>Markdown text shown on the new project page.</summary>
    public string? NewProjectGuidelines { get; init; }

    /// <summary>Markdown text shown on the members page of a group or project.</summary>
    public string? MemberGuidelines { get; init; }

    /// <summary>Markdown text shown on the profile page below the public avatar.</summary>
    public string? ProfileImageGuidelines { get; init; }

    /// <summary>Message shown within the system header bar.</summary>
    public string? HeaderMessage { get; init; }

    /// <summary>Message shown within the system footer bar.</summary>
    public string? FooterMessage { get; init; }

    /// <summary>Background color for the system header / footer bar.</summary>
    public string? MessageBackgroundColor { get; init; }

    /// <summary>Font color for the system header / footer bar.</summary>
    public string? MessageFontColor { get; init; }

    /// <summary>Whether the header and footer message are added to all outgoing emails.</summary>
    public bool? EmailHeaderAndFooterEnabled { get; init; }

    /// <summary>The last part of the webpage title.</summary>
    public string? SiteName { get; init; }
}