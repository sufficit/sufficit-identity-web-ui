﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Sufficit.Identity.Web
{
    /// <summary>
    /// Information about the App Services configuration on the host.
    /// </summary>
    public static class AppServicesAuthenticationInformation
    {
        // Environment variables.
        internal const string AppServicesAuthEnabledEnvironmentVariable = "WEBSITE_AUTH_ENABLED";            // True
        internal const string AppServicesAuthOpenIdIssuerEnvironmentVariable = "WEBSITE_AUTH_OPENID_ISSUER"; // for instance https://sts.windows.net/<tenantId>/
        internal const string AppServicesAuthClientIdEnvironmentVariable = "WEBSITE_AUTH_CLIENT_ID";         // A GUID
        internal const string AppServicesAuthClientSecretEnvironmentVariable = "WEBSITE_AUTH_CLIENT_SECRET"; // A string
        internal const string AppServicesAuthLogoutPathEnvironmentVariable = "WEBSITE_AUTH_LOGOUT_PATH";    // /.auth/logout
        internal const string AppServicesAuthIdentityProviderEnvironmentVariable = "WEBSITE_AUTH_DEFAULT_PROVIDER"; // AzureActiveDirectory
        internal const string AppServicesAuthAzureActiveDirectory = "AzureActiveDirectory";
        internal const string AppServicesAuthAAD = "AAD";
        internal const string AppServicesAuthIdTokenHeader = "X-MS-TOKEN-AAD-ID-TOKEN";
        internal const string AppServicesWebSiteAuthApiPrefix = "WEBSITE_AUTH_API_PREFIX";
        private const string AppServicesAuthIdpTokenHeader = "X-MS-CLIENT-PRINCIPAL-IDP";

        // Artificially added by Microsoft.Identity.Web to help debugging App Services. See the Debug controller of the test app
        internal const string AppServicesAuthDebugHeadersEnvironmentVariable = "APP_SERVICES_AUTH_LOCAL_DEBUG";

        /// <summary>
        /// Is App Services authentication enabled?.
        /// </summary>
        public static bool IsAppServicesAadAuthenticationEnabled
        {
            get
            {
                return

                    string.Equals(
                        Environment.GetEnvironmentVariable(AppServicesAuthEnabledEnvironmentVariable),
                        Constants.True,
                        StringComparison.OrdinalIgnoreCase) &&

                     (string.Equals(
                         Environment.GetEnvironmentVariable(AppServicesAuthIdentityProviderEnvironmentVariable),
                         AppServicesAuthAzureActiveDirectory,
                         StringComparison.OrdinalIgnoreCase)
                     ||
                     string.Equals(
                         Environment.GetEnvironmentVariable(AppServicesAuthIdentityProviderEnvironmentVariable),
                         AppServicesAuthAAD,
                         StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Logout URL for App Services Auth web sites.
        /// </summary>
        public static string? LogoutUrl
        {
            get
            {
                // Try $AppServicesAuthLogoutPathEnvironmentVariable (AppServices auth v1.0)
                string? logoutPath = Environment.GetEnvironmentVariable(AppServicesAuthLogoutPathEnvironmentVariable);
                if (!string.IsNullOrEmpty(logoutPath))
                {
                    return logoutPath;
                }

                // Try the $(AppServicesWebSiteAuthApiPrefix)
                string? webSite = Environment.GetEnvironmentVariable(AppServicesWebSiteAuthApiPrefix);
                if (!string.IsNullOrEmpty(webSite))
                {
                    return $"{webSite}/logout";
                }

                // Fallback
                return "/.auth/logout";
            }
        }

        /// <summary>
        /// ClientID of the App Services Auth web site.
        /// </summary>
        internal static string? ClientId
        {
            get
            {
                return Environment.GetEnvironmentVariable(AppServicesAuthClientIdEnvironmentVariable);
            }
        }

        /// <summary>
        /// Client secret of the App Services Auth web site.
        /// </summary>
        internal static string? ClientSecret
        {
            get
            {
                return Environment.GetEnvironmentVariable(AppServicesAuthClientSecretEnvironmentVariable);
            }
        }

        /// <summary>
        /// Issuer of the App Services Auth web site.
        /// </summary>
        internal static string? Issuer
        {
            get
            {
                return Environment.GetEnvironmentVariable(AppServicesAuthOpenIdIssuerEnvironmentVariable);
            }
        }

#if DEBUG
        /// <summary>
        /// Get headers from environment to help debugging App Services authentication.
        /// </summary>
        internal static string? SimulateGetttingHeaderFromDebugEnvironmentVariable(string header)
        {
            string? headerPlusValue = Environment.GetEnvironmentVariable(AppServicesAuthDebugHeadersEnvironmentVariable)
                ?.Split(';')
                ?.FirstOrDefault(h => h.StartsWith(header, StringComparison.OrdinalIgnoreCase));
            return headerPlusValue?.Substring(header.Length + 1);
        }
#endif

        /// <summary>
        /// Get the ID token from the headers sent by App services authentication.
        /// </summary>
        /// <param name="headers">Headers.</param>
        /// <returns>The ID Token.</returns>
        internal static string? GetIdToken(IDictionary<string, StringValues> headers)
        {
            if (headers is null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            headers.TryGetValue(AppServicesAuthIdTokenHeader, out var idToken);

#if DEBUG
            if (string.IsNullOrEmpty(idToken))
            {
                idToken = SimulateGetttingHeaderFromDebugEnvironmentVariable(AppServicesAuthIdTokenHeader);
            }
#endif
            return idToken;
        }

        /// <summary>
        /// Get the IDP from the headers sent by App services authentication.
        /// </summary>
        /// <param name="headers">Headers.</param>
        /// <returns>The IDP.</returns>
        internal static string? GetIdp(IDictionary<string, StringValues> headers)
        {
            if (headers is null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            headers.TryGetValue(AppServicesAuthIdpTokenHeader, out var idp);
#if DEBUG
            if (string.IsNullOrEmpty(idp))
            {
                idp = SimulateGetttingHeaderFromDebugEnvironmentVariable(AppServicesAuthIdpTokenHeader);
            }
#endif
            return idp;
        }

        /// <summary>
        /// Get the user claims from the headers and environment variables.
        /// </summary>
        /// <param name="headers">Headers.</param>
        /// <returns>User claims.</returns>
        internal static ClaimsPrincipal? GetUser(IDictionary<string, StringValues> headers)
        {
            ClaimsPrincipal? claimsPrincipal;
            string? idToken = GetIdToken(headers);
            string? idp = GetIdp(headers);
            if (idToken != null && idp != null)
            {
                JsonWebToken jsonWebToken = new JsonWebToken(idToken);
                bool isAadV1Token = jsonWebToken.Claims
                    .Any(c => c.Type == Constants.Version && c.Value == Constants.V1);
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                    jsonWebToken.Claims,
                    idp,
                    isAadV1Token ? Constants.NameClaim : Constants.PreferredUserName,
                    ClaimsIdentity.DefaultRoleClaimType));
            }
            else
            {
                claimsPrincipal = null;
            }

            return claimsPrincipal;
        }
    }
}
