namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /projects/:id/feature_flags_user_lists</c>.</summary>
public sealed record CreateFeatureFlagUserListRequest
{
    /// <summary>The name of the new list.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The external user IDs the list holds, as one comma-separated string - GitLab stores and returns
    ///     them in exactly that form rather than as an array.
    /// </summary>
    public required string UserXids { get; init; }
}