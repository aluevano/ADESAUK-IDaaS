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
using IdentityServer.Core.Logging;
using Microsoft.Owin;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;


#pragma warning disable 1591

namespace IdentityServer.Core.Configuration.Hosting
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LastUserNameCookie
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        const string LastUsernameCookieName = "idsvr.username";

        readonly IOwinContext ctx;
        readonly IdentityServerOptions options;

        internal LastUserNameCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            if (options == null) throw new ArgumentNullException("options");
            
            this.ctx = ctx;
            this.options = options;
        }

        internal string GetValue()
        {
            if (options.AuthenticationOptions.RememberLastUsername)
            {
                try
                {
                    var cookieName = options.AuthenticationOptions.CookieOptions.Prefix + LastUsernameCookieName;
                    var value = ctx.Request.Cookies[cookieName];

                    var bytes = Base64Url.Decode(value);
                    try
                    {
                        bytes = options.DataProtector.Unprotect(bytes, cookieName);
                    }
                    catch(CryptographicException)
                    {
                        SetValue(null);
                        return null;
                    }
                    value = Encoding.UTF8.GetString(bytes);

                    return value;
                }
                catch
                {
                    SetValue(null);
                }
            }
            return null;
        }

        internal void SetValue(string username)
        {
            if (options.AuthenticationOptions.RememberLastUsername)
            {
                var cookieName = options.AuthenticationOptions.CookieOptions.Prefix + LastUsernameCookieName;
                var secure =
                    options.AuthenticationOptions.CookieOptions.SecureMode == CookieSecureMode.Always ||
                    ctx.Request.Scheme == Uri.UriSchemeHttps;
                var path = ctx.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();

                var cookieOptions = new Microsoft.Owin.CookieOptions
                {
                    HttpOnly = true,
                    Secure = secure,
                    Path = path
                };

                if (!String.IsNullOrWhiteSpace(username))
                {
                    var bytes = Encoding.UTF8.GetBytes(username);
                    bytes = options.DataProtector.Protect(bytes, cookieName);
                    username = Base64Url.Encode(bytes);
                    cookieOptions.Expires = DateTimeHelper.UtcNow.AddYears(1);
                }
                else
                {
                    username = ".";
                    cookieOptions.Expires = DateTimeHelper.UtcNow.AddYears(-1);
                }

                ctx.Response.Cookies.Append(cookieName, username, cookieOptions);
            }
        }
    }
}