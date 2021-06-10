﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Sufficit.Identity.Web
{
    /// <summary>
    /// Constants related to the log messages.
    /// </summary>
    internal static class LogMessages
    {
        public const string MissingRoles = "The 'roles' or 'role' claim does not contain roles '{0}' or was not found";
        public const string MissingScopes = "The 'scope' or 'scp' claim does not contain scopes '{0}' or was not found";
        public const string ExceptionOccurredWhenAddingAnAccountToTheCacheFromAuthCode = "Exception occurred while adding an account to the cache from the auth code. ";
        public const string ErrorAcquiringTokenForDownstreamWebApi = "Error acquiring a token for a downstream web API - MsalUiRequiredException message is: ";

        // Diagnostics
        public const string MethodBegin = "Begin {0}. ";
        public const string MethodEnd = "End {0}. ";
    }
}
