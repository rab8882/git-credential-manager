// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Net.Http;
using GitCredentialManager;
using GitCredentialManager.Authentication.OAuth;

namespace Atlassian.Bitbucket.Cloud
{
    public class BitbucketOAuth2Client : Bitbucket.BitbucketOAuth2Client
    {
        public BitbucketOAuth2Client(HttpClient httpClient, ISettings settings, ITrace2 trace2)
            : base(httpClient, GetEndpoints(),
                GetClientId(settings), GetRedirectUri(settings), GetClientSecret(settings), trace2)
        {
        }

        public override IEnumerable<string> Scopes => new string[] {
            CloudConstants.OAuthScopes.RepositoryWrite,
            CloudConstants.OAuthScopes.Account,
        };

        private static string GetClientId(ISettings settings)
        {
            return settings.GetDevOverrideOrDefault(
                CloudConstants.EnvironmentVariables.OAuthClientId,
                CloudConstants.GitConfiguration.Credential.OAuthClientId,
                CloudConstants.OAuth2ClientId);
        }

        private static Uri GetRedirectUri(ISettings settings)
        {
            return settings.GetDevOverrideUriOrDefault(
                CloudConstants.EnvironmentVariables.OAuthRedirectUri,
                CloudConstants.GitConfiguration.Credential.OAuthRedirectUri,
                CloudConstants.OAuth2RedirectUri);
        }

        private static string GetClientSecret(ISettings settings)
        {
            return settings.GetDevOverrideOrDefault(
                CloudConstants.EnvironmentVariables.OAuthClientSecret,
                CloudConstants.GitConfiguration.Credential.OAuthClientSecret,
                CloudConstants.OAuth2ClientSecret);
        }

        private static OAuth2ServerEndpoints GetEndpoints()
        {
            return new OAuth2ServerEndpoints(
                CloudConstants.OAuth2AuthorizationEndpoint,
                CloudConstants.OAuth2TokenEndpoint
            );
        }
    }
}
