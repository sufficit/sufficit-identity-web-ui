// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace Sufficit.Identity.Web.UI
{
    /// <summary>
    /// Extension method on <see cref="IMvcBuilder"/> to add UI
    /// for Microsoft.Identity.Web.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a controller and Razor pages for the accounts management.
        /// </summary>
        /// <param name="builder">MVC builder.</param>
        /// <returns>MVC builder for chaining.</returns>
        public static IMvcBuilder AddSufficitIdentityUI(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureApplicationPartManager(apm =>
            {
                apm.FeatureProviders.Add(new SufficitIdentityAccountControllerFeatureProvider());
            });

            builder.Services.ConfigureAll<CookieAuthenticationOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.AccessDeniedPath))
                {
                    options.AccessDeniedPath = new PathString("/SufficitIdentity/Account/AccessDenied");
                }
            });

            return builder;
        }

        public static void AddSufficitAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {                    
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);                    
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Events.OnSigningOut = async e =>
                    {
                        await e.HttpContext.RevokeUserRefreshTokenAsync();
                    };
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = "https://identity.sufficit.com.br:5001/";
                    options.ClientId = "SufficitMVC";
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.Scope.Add("read"); // necessário para conectar a api
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                    };
                });

            services.AddAccessTokenManagement();
        }
    }
}
