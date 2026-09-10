namespace GitLab.Client.Models.Requests;

/// <summary>The body of <c>PUT /groups/:id/security_settings</c>.</summary>
public sealed record UpdateGroupSecuritySettingsRequest
{
    public required bool SecretPushProtectionEnabled { get; init; }

    /// <summary>IDs of projects to exclude from secret push protection.</summary>
    public IReadOnlyList<long>? ProjectsToExclude { get; init; }
}