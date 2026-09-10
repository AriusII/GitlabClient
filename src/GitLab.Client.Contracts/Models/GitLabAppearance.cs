namespace GitLab.Client.Models;

/// <summary>
///     The instance-wide branding shown on the sign-in and sign-up pages, as returned by
///     <c>GET /application/appearance</c> and by every write to it.
///     <para>
///         GitLab's write side (<c>PUT /application/appearance</c>) is one route with two shapes: the
///         text and boolean fields below travel as a plain JSON body through
///         <see cref="Abstractions.IInstanceClient.UpdateAppearanceAsync" />, while the four binary
///         fields (<c>logo</c>, <c>pwa_icon</c>, <c>header_logo</c>, <c>favicon</c>) - each independently
///         settable - go through their own <c>multipart/form-data</c> call
///         (<see cref="Abstractions.IInstanceClient.SetAppearanceLogoAsync" /> and its three siblings),
///         since a single multipart request can carry only one file part at a time. This mirrors how
///         GitLab's own documentation demonstrates the endpoint.
///     </para>
/// </summary>
public sealed record GitLabAppearance
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

    /// <summary>The instance image used on the sign-in / sign-up page.</summary>
    public Uri? Logo { get; init; }

    /// <summary>The icon used for the Progressive Web App.</summary>
    public Uri? PwaIcon { get; init; }

    /// <summary>The instance image used for the main navigation bar.</summary>
    public Uri? HeaderLogo { get; init; }

    /// <summary>The instance favicon.</summary>
    public Uri? Favicon { get; init; }

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