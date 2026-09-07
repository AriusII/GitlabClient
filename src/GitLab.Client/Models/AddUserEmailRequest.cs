namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /users/:id/emails</c>. Administrators only.</summary>
public sealed record AddUserEmailRequest
{
    public required string Email { get; init; }

    /// <summary>Treat the address as already verified instead of mailing a confirmation link.</summary>
    public bool? SkipConfirmation { get; init; }
}