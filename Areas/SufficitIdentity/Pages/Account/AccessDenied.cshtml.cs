// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sufficit.Identity.Web.UI.Areas.SufficitIdentity.Pages.Account
{
    /// <summary>
    /// Page presenting the Access denied error.
    /// </summary>
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        /// <summary>
        /// Method handling the HTTP GET method.
        /// </summary>
        public void OnGet()
        {
        }
    }
}
