namespace GitLab.Client.DependencyInjection;

public enum GitLabAuthenticationMode
{
    /// <summary>Sends the token via the <c>PRIVATE-TOKEN</c> header.</summary>
    PersonalAccessToken,

    /// <summary>Sends the token via <c>Authorization: Bearer</c>.</summary>
    OAuthBearer,

    /// <summary>Sends the token via the <c>JOB-TOKEN</c> header, for use inside a GitLab CI/CD job.</summary>
    JobToken
}