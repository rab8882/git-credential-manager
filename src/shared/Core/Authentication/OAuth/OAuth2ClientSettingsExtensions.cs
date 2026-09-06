using System;

namespace GitCredentialManager.Authentication.OAuth
{
    /// <summary>
    /// Helpers for looking up developer override values (client ID, client secret, redirect URI, etc.)
    /// for <see cref="OAuth2Client"/> implementations from <see cref="ISettings"/>.
    /// </summary>
    /// <remarks>
    /// Host providers commonly need to allow developers to override the built-in OAuth2 application
    /// registration (client ID/secret/redirect URI) via environment variables or Git configuration.
    /// These helpers centralize that lookup pattern so it does not need to be duplicated per-provider.
    /// </remarks>
    public static class OAuth2ClientSettingsExtensions
    {
        /// <summary>
        /// Get a developer override string value if one is configured, otherwise the given default value.
        /// </summary>
        public static string GetDevOverrideOrDefault(
            this ISettings settings, string envarName, string configName, string defaultValue)
        {
            EnsureArgument.NotNull(settings, nameof(settings));

            if (settings.TryGetSetting(
                    envarName, Constants.GitConfiguration.Credential.SectionName, configName, out string value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get a developer override string value if one is configured, otherwise throw an
        /// <see cref="ArgumentException"/> with the given error message.
        /// </summary>
        public static string GetDevOverrideOrThrow(
            this ISettings settings, string envarName, string configName, string errorMessage)
        {
            EnsureArgument.NotNull(settings, nameof(settings));

            if (settings.TryGetSetting(
                    envarName, Constants.GitConfiguration.Credential.SectionName, configName, out string value))
            {
                return value;
            }

            throw new ArgumentException(errorMessage);
        }

        /// <summary>
        /// Get a developer override URI value if one is configured and valid, otherwise the given default value.
        /// </summary>
        public static Uri GetDevOverrideUriOrDefault(
            this ISettings settings, string envarName, string configName, Uri defaultValue)
        {
            EnsureArgument.NotNull(settings, nameof(settings));

            if (settings.TryGetSetting(
                    envarName, Constants.GitConfiguration.Credential.SectionName, configName, out string value) &&
                Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            {
                return uri;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get a developer override URI value if one is configured and valid, otherwise throw an
        /// <see cref="ArgumentException"/> with the given error message.
        /// </summary>
        public static Uri GetDevOverrideUriOrThrow(
            this ISettings settings, string envarName, string configName, string errorMessage)
        {
            EnsureArgument.NotNull(settings, nameof(settings));

            if (settings.TryGetSetting(
                    envarName, Constants.GitConfiguration.Credential.SectionName, configName, out string value) &&
                Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            {
                return uri;
            }

            throw new ArgumentException(errorMessage);
        }
    }
}
