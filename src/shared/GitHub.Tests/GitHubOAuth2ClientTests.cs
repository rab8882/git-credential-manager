using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitCredentialManager;
using GitCredentialManager.Authentication.OAuth;
using GitCredentialManager.Tests.Objects;
using Xunit;

namespace GitHub.Tests
{
    public class GitHubOAuth2ClientTests
    {
        [Fact]
        public async Task GitHubOAuth2Client_GetAuthorizationCodeAsync_DotCom_UsesDefaultClientAndRedirectUri()
        {
            var baseUri = new Uri("https://github.com");

            var httpHandler = new TestHttpMessageHandler { ThrowOnUnexpectedRequest = true };

            var server = new TestOAuth2Server(CreateEndpoints(baseUri));
            server.RegisterApplication(new OAuth2Application(GitHubConstants.OAuthClientId)
            {
                Secret = GitHubConstants.OAuthClientSecret,
                RedirectUris = new[] { GitHubConstants.OAuthRedirectUri }
            });
            server.Bind(httpHandler);
            server.TokenGenerator.AuthCodes.Add("test-auth-code");

            Uri actualRedirectUri = null;
            server.AuthorizationEndpointInvoked += (_, request) =>
            {
                var query = request.RequestUri.GetQueryParameters();
                query.TryGetValue(OAuth2Constants.RedirectUriParameter, out string redirectUriStr);
                actualRedirectUri = new Uri(redirectUriStr);
            };

            var settings = new TestSettings { Environment = new TestEnvironment() };
            var client = new GitHubOAuth2Client(new HttpClient(httpHandler), settings, baseUri, new NullTrace2());

            IOAuth2WebBrowser browser = new TestOAuth2WebBrowser(httpHandler);

            OAuth2AuthorizationCodeResult result = await client.GetAuthorizationCodeAsync(
                new[] { "repo" }, browser, null, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(GitHubConstants.OAuthRedirectUri, actualRedirectUri);
        }

        [Fact]
        public async Task GitHubOAuth2Client_GetAuthorizationCodeAsync_Enterprise_UsesLegacyRedirectUri()
        {
            var baseUri = new Uri("https://github.contoso.com");

            var httpHandler = new TestHttpMessageHandler { ThrowOnUnexpectedRequest = true };

            var server = new TestOAuth2Server(CreateEndpoints(baseUri));
            server.RegisterApplication(new OAuth2Application(GitHubConstants.OAuthClientId)
            {
                Secret = GitHubConstants.OAuthClientSecret,
                RedirectUris = new[] { GitHubConstants.OAuthLegacyRedirectUri }
            });
            server.Bind(httpHandler);
            server.TokenGenerator.AuthCodes.Add("test-auth-code");

            Uri actualRedirectUri = null;
            server.AuthorizationEndpointInvoked += (_, request) =>
            {
                var query = request.RequestUri.GetQueryParameters();
                query.TryGetValue(OAuth2Constants.RedirectUriParameter, out string redirectUriStr);
                actualRedirectUri = new Uri(redirectUriStr);
            };

            var settings = new TestSettings { Environment = new TestEnvironment() };
            var client = new GitHubOAuth2Client(new HttpClient(httpHandler), settings, baseUri, new NullTrace2());

            IOAuth2WebBrowser browser = new TestOAuth2WebBrowser(httpHandler);

            OAuth2AuthorizationCodeResult result = await client.GetAuthorizationCodeAsync(
                new[] { "repo" }, browser, null, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(GitHubConstants.OAuthLegacyRedirectUri, actualRedirectUri);
        }

        [Fact]
        public async Task GitHubOAuth2Client_DevOverrides_ClientIdSecretAndRedirectUri_AreHonored()
        {
            const string devClientId = "dev-client-id";
            const string devClientSecret = "dev-client-secret";
            var devRedirectUri = new Uri("http://127.0.0.1:12345/callback");

            var baseUri = new Uri("https://github.com");

            var httpHandler = new TestHttpMessageHandler { ThrowOnUnexpectedRequest = true };

            var server = new TestOAuth2Server(CreateEndpoints(baseUri));
            server.RegisterApplication(new OAuth2Application(devClientId)
            {
                Secret = devClientSecret,
                RedirectUris = new[] { devRedirectUri }
            });
            server.Bind(httpHandler);
            server.TokenGenerator.AuthCodes.Add("test-auth-code");

            var settings = new TestSettings { Environment = new TestEnvironment() };
            settings.Environment.Variables[GitHubConstants.EnvironmentVariables.DevOAuthClientId] = devClientId;
            settings.Environment.Variables[GitHubConstants.EnvironmentVariables.DevOAuthClientSecret] = devClientSecret;
            settings.Environment.Variables[GitHubConstants.EnvironmentVariables.DevOAuthRedirectUri] = devRedirectUri.ToString();

            var client = new GitHubOAuth2Client(new HttpClient(httpHandler), settings, baseUri, new NullTrace2());

            IOAuth2WebBrowser browser = new TestOAuth2WebBrowser(httpHandler);

            OAuth2AuthorizationCodeResult authCodeResult = await client.GetAuthorizationCodeAsync(
                new[] { "repo" }, browser, null, CancellationToken.None);
            Assert.NotNull(authCodeResult);

            OAuth2TokenResult tokenResult = await client.GetTokenByAuthorizationCodeAsync(
                authCodeResult, CancellationToken.None);

            Assert.NotNull(tokenResult);
            Assert.NotNull(tokenResult.AccessToken);
        }

        [Fact]
        public async Task GitHubOAuth2Client_GetDeviceCodeAsync_UsesGitHubDeviceEndpoint()
        {
            var baseUri = new Uri("https://github.com");

            var httpHandler = new TestHttpMessageHandler { ThrowOnUnexpectedRequest = true };

            var server = new TestOAuth2Server(CreateEndpoints(baseUri));
            server.RegisterApplication(new OAuth2Application(GitHubConstants.OAuthClientId)
            {
                Secret = GitHubConstants.OAuthClientSecret,
                RedirectUris = new[] { GitHubConstants.OAuthRedirectUri }
            });
            server.Bind(httpHandler);

            bool deviceEndpointInvoked = false;
            server.DeviceAuthorizationEndpointInvoked += (_, _) => deviceEndpointInvoked = true;

            var settings = new TestSettings { Environment = new TestEnvironment() };
            var client = new GitHubOAuth2Client(new HttpClient(httpHandler), settings, baseUri, new NullTrace2());

            OAuth2DeviceCodeResult result = await client.GetDeviceCodeAsync(new[] { "repo" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(deviceEndpointInvoked);
        }

        private static OAuth2ServerEndpoints CreateEndpoints(Uri baseUri)
        {
            Uri authEndpoint = new Uri(baseUri, GitHubConstants.OAuthAuthorizationEndpointRelativeUri);
            Uri tokenEndpoint = new Uri(baseUri, GitHubConstants.OAuthTokenEndpointRelativeUri);
            Uri deviceEndpoint = new Uri(baseUri, GitHubConstants.OAuthDeviceEndpointRelativeUri);

            return new OAuth2ServerEndpoints(authEndpoint, tokenEndpoint)
            {
                DeviceAuthorizationEndpoint = deviceEndpoint
            };
        }
    }
}
