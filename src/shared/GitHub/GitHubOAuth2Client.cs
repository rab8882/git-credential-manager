using System;
using System.Net.Http;
using GitCredentialManager;
using GitCredentialManager.Authentication.OAuth;

namespace GitHub
{
    public class GitHubOAuth2Client : OAuth2Client
    {
        public GitHubOAuth2Client(HttpClient httpClient, ISettings settings, Uri baseUri, ITrace2 trace2)
            : base(httpClient, CreateEndpoints(baseUri),
                GetClientId(settings), trace2, GetRedirectUri(settings, baseUri), GetClientSecret(settings)) { }

        private static OAuth2ServerEndpoints CreateEndpoints(Uri uri)
        {
            // Ensure that the base URI is normalized to support Gist subdomains
            Uri baseUri = GitHubHostProvider.NormalizeUri(uri);

            Uri authEndpoint = new Uri(baseUri, GitHubConstants.OAuthAuthorizationEndpointRelativeUri);
            Uri tokenEndpoint = new Uri(baseUri, GitHubConstants.OAuthTokenEndpointRelativeUri);
            Uri deviceAuthEndpoint = new Uri(baseUri, GitHubConstants.OAuthDeviceEndpointRelativeUri);

            return new OAuth2ServerEndpoints(authEndpoint, tokenEndpoint)
            {
                DeviceAuthorizationEndpoint = deviceAuthEndpoint
            };
        }

        private static string GetClientId(ISettings settings)
        {
            return settings.GetDevOverrideOrDefault(
                GitHubConstants.EnvironmentVariables.DevOAuthClientId,
                GitHubConstants.GitConfiguration.Credential.DevOAuthClientId,
                GitHubConstants.OAuthClientId);
        }

        private static Uri GetRedirectUri(ISettings settings, Uri targetUri)
        {
            // Only GitHub.com supports the new OAuth redirect URI today
            Uri defaultRedirectUri = GitHubHostProvider.IsGitHubDotCom(targetUri)
                ? GitHubConstants.OAuthRedirectUri
                : GitHubConstants.OAuthLegacyRedirectUri;

            return settings.GetDevOverrideUriOrDefault(
                GitHubConstants.EnvironmentVariables.DevOAuthRedirectUri,
                GitHubConstants.GitConfiguration.Credential.DevOAuthRedirectUri,
                defaultRedirectUri);
        }

        private static string GetClientSecret(ISettings settings)
        {
            return settings.GetDevOverrideOrDefault(
                GitHubConstants.EnvironmentVariables.DevOAuthClientSecret,
                GitHubConstants.GitConfiguration.Credential.DevOAuthClientSecret,
                GitHubConstants.OAuthClientSecret);
        }
    }
}
