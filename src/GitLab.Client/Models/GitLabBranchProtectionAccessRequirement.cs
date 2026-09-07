namespace GitLab.Client.Models;

/// <summary>One access-level entry within <see cref="GitLabBranchProtectionDefaults" />.</summary>
public sealed record GitLabBranchProtectionAccessRequirement
{
    /// <summary>
    ///     GitLab's numeric access-level ladder: <c>0</c> (no access), <c>30</c> (Developer), <c>40</c>
    ///     (Maintainer) or <c>60</c> (Admin).
    /// </summary>
    public required int AccessLevel { get; init; }
}