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

using IdentityModel;
using IdentityServer.Core.Extensions;
using Microsoft.Owin;
using System;
using System.ComponentModel;

#pragma warning disable 1591

namespace IdentityServer.Core.Configuration.Hosting
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SessionCookie
    {
        readonly IOwinContext context;
        readonly IdentityServerOptions identityServerOptions;

        protected internal SessionCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            this.context = ctx;
            this.identityServerOptions = options;
        }

        public virtual void IssueSessionId(bool? persistent, DateTimeOffset? expires = null)
        {
            context.Response.Cookies.Append(
                GetCookieName(), CryptoRandom.CreateUniqueId(), 
                CreateCookieOptions(persistent, expires));
        }

        private Microsoft.Owin.CookieOptions CreateCookieOptions(bool? persistent, DateTimeOffset? expires = null)
        {
            var path = context.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();
            var secure =
                identityServerOptions.AuthenticationOptions.CookieOptions.SecureMode == CookieSecureMode.Always ||
                context.Request.Scheme == Uri.UriSchemeHttps;

            var options = new Microsoft.Owin.CookieOptions
            {
                HttpOnly = false,
                Secure = secure,
                Path = path
            };

            if (persistent != false)
            {
                if (persistent == true || this.identityServerOptions.AuthenticationOptions.CookieOptions.IsPersistent)
                {
                    if (persistent == true)
                    {
                        expires = expires ?? DateTimeHelper.UtcNow.Add(this.identityServerOptions.AuthenticationOptions.CookieOptions.RememberMeDuration);
                    }
                    else
                    {
                        expires = expires ?? DateTimeHelper.UtcNow.Add(this.identityServerOptions.AuthenticationOptions.CookieOptions.ExpireTimeSpan);
                    }
                    options.Expires = expires.Value.UtcDateTime;
                }
            }

            return options;
        }

        private string GetCookieName()
        {
            return identityServerOptions.AuthenticationOptions.CookieOptions.GetSessionCookieName();
        }

        public virtual string GetSessionId()
        {
            return context.Request.Cookies[GetCookieName()];
        }

        public virtual void ClearSessionId()
        {
            var options = CreateCookieOptions(false);
            options.Expires = DateTimeHelper.UtcNow.AddYears(-1);
            
            var name = GetCookieName();
            context.Response.Cookies.Append(name, ".", options);
        }
    }
}
