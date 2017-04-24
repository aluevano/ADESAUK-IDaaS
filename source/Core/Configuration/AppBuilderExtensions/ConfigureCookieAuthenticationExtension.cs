/*
*	Copyright (c) 2017 Antonio Luevano. All rights reserved. 
* 
* This code is licensed under the MIT License (MIT). 
*
* Permission is hereby granted, free of charge, to any person obtaining a copy 
* of this software and associated documentation files (the "Software"), to deal 
* in the Software without restriction, including without limitation the rights 
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
* of the Software, and to permit persons to whom the Software is furnished to do 
* so, subject to the following conditions: 

* The above copyright notice and this permission notice shall be included in all 
* copies or substantial portions of the Software. 
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
* THE SOFTWARE. 
*/

using IdentityServer.Core;
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Configuration.Hosting;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Services;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Owin
{
    internal static class UseCookieAuthenticationExtension
    {
        public static IAppBuilder ConfigureCookieAuthentication(this IAppBuilder app, CookieOptions options, IDataProtector dataProtector)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (dataProtector == null) throw new ArgumentNullException("dataProtector");

            if (options.Prefix.IsPresent())
            {
                options.Prefix += ".";
            }

            var primary = new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PrimaryAuthenticationType,
                CookieName = options.Prefix + Constants.PrimaryAuthenticationType,
                ExpireTimeSpan = options.ExpireTimeSpan,
                SlidingExpiration = options.SlidingExpiration,
                CookieSecure = GetCookieSecure(options.SecureMode),
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.PrimaryAuthenticationType)),
                SessionStore = GetSessionStore(options.SessionStoreProvider),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = async cookieCtx =>
                    {
                        var validator = cookieCtx.OwinContext.Environment.ResolveDependency<IAuthenticationSessionValidator>();
                        var isValid = await validator.IsAuthenticationSessionValidAsync(new ClaimsPrincipal(cookieCtx.Identity));
                        if (isValid == false)
                        {
                            cookieCtx.RejectIdentity();
                        }
                    }
                }
            };
            app.UseCookieAuthentication(primary);

            var external = new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.ExternalAuthenticationType,
                CookieName = options.Prefix + Constants.ExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ExpireTimeSpan = Constants.ExternalCookieTimeSpan,
                SlidingExpiration = false,
                CookieSecure = GetCookieSecure(options.SecureMode),
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.ExternalAuthenticationType))
            };
            app.UseCookieAuthentication(external);

            var partial = new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PartialSignInAuthenticationType,
                CookieName = options.Prefix + Constants.PartialSignInAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ExpireTimeSpan = options.ExpireTimeSpan,
                SlidingExpiration = options.SlidingExpiration,
                CookieSecure = GetCookieSecure(options.SecureMode),
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.PartialSignInAuthenticationType))
            };
            app.UseCookieAuthentication(partial);

            Action<string> setCookiePath = path =>
            {
                if (!String.IsNullOrWhiteSpace(path))
                {
                    primary.CookiePath = external.CookiePath = path;
                    partial.CookiePath = path;
                }
            };
            
            if (String.IsNullOrWhiteSpace(options.Path))
            {
                app.Use(async (ctx, next) =>
                {
                    // we only want this to run once, so assign to null once called 
                    // (and yes, it's possible that many callers hit this at same time, 
                    // but the set is idempotent)
                    if (setCookiePath != null)
                    {
                        setCookiePath(ctx.Request.PathBase.Value);
                        setCookiePath = null;
                    }
                    await next();
                });
            }
            else
            {
                setCookiePath(options.Path);
            }

            return app;
        }

        private static CookieSecureOption GetCookieSecure(CookieSecureMode cookieSecureMode)
        {
            switch (cookieSecureMode)
            {
                case CookieSecureMode.Always:
                    return CookieSecureOption.Always;
                case CookieSecureMode.SameAsRequest:
                    return CookieSecureOption.SameAsRequest;
                default:
                    throw new InvalidOperationException("Invalid CookieSecureMode");
            }
        }

        private static IAuthenticationSessionStore GetSessionStore(IAuthenticationSessionStoreProvider provider)
        {
            return provider != null ? new AuthenticationSessionStoreWrapper(provider) : null;
        }
    }
}