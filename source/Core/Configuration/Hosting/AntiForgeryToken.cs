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
using IdentityServer.Core.ViewModels;
using Microsoft.Owin;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer.Core.Configuration.Hosting
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AntiForgeryToken
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        const string TokenName = "idsrv.xsrf";
        const string CookieEntropy = TokenName + "AntiForgeryTokenCookie";
        const string HiddenInputEntropy = TokenName + "AntiForgeryTokenHidden";

        readonly IOwinContext context;
        readonly IdentityServerOptions options;

        internal AntiForgeryToken(IOwinContext context, IdentityServerOptions options)
        {
            this.context = context;
            this.options = options;
        }

        internal AntiForgeryTokenViewModel GetAntiForgeryToken()
        {
            var tokenBytes = GetCookieToken();
            var protectedTokenBytes = options.DataProtector.Protect(tokenBytes, HiddenInputEntropy);
            var token = Base64Url.Encode(protectedTokenBytes);

            return new AntiForgeryTokenViewModel
            {
                Name = TokenName,
                Value = token
            };
        }
        
        internal async Task<bool> IsTokenValid()
        {
            if (context.GetSuppressAntiForgeryCheck())
            {
                return true;
            }

            try
            {
                var cookieToken = GetCookieToken();
                var hiddenInputToken = await GetHiddenInputTokenAsync();
                return CompareByteArrays(cookieToken, hiddenInputToken);
            }
            catch(Exception ex)
            {
                Logger.ErrorException("AntiForgeryTokenValidator validating token", ex);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        bool CompareByteArrays(byte[] cookieToken, byte[] hiddenInputToken)
        {
            if (cookieToken == null || hiddenInputToken == null) return false;
            if (cookieToken.Length != hiddenInputToken.Length) return false;
            
            bool same = true;
            for(var i = 0; i < cookieToken.Length; i++)
            {
                same &= (cookieToken[i] == hiddenInputToken[i]);
            }
            return same;
        }

        byte[] GetCookieToken()
        {
            var cookieName = options.AuthenticationOptions.CookieOptions.Prefix + TokenName;
            var cookie = context.Request.Cookies[cookieName];

            if (cookie != null)
            {
                try
                {
                    var protectedCookieBytes = Base64Url.Decode(cookie);
                    var tokenBytes = options.DataProtector.Unprotect(protectedCookieBytes, CookieEntropy);
                    return tokenBytes;
                }
                catch(Exception ex)
                {
                    // if there's an exception we fall thru the catch block to reissue a new cookie
                    Logger.WarnFormat("Problem unprotecting cookie; Issuing new cookie. Error message: {0}", ex.Message);
                }
            }

            var bytes = CryptoRandom.CreateRandomKey(16);
            var protectedTokenBytes = options.DataProtector.Protect(bytes, CookieEntropy);
            var token = Base64Url.Encode(protectedTokenBytes);
            
            var secure = 
                options.AuthenticationOptions.CookieOptions.SecureMode == CookieSecureMode.Always || 
                context.Request.Scheme == Uri.UriSchemeHttps;

            var path = context.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();
            context.Response.Cookies.Append(cookieName, token, new Microsoft.Owin.CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                Path = path
            });

            return bytes;
        }

        async Task<byte[]> GetHiddenInputTokenAsync()
        {
            var form = await context.ReadRequestFormAsync();

            var token = form[TokenName];
            if (token == null) return null;

            var tokenBytes = Base64Url.Decode(token);
            var unprotectedTokenBytes = options.DataProtector.Unprotect(tokenBytes, HiddenInputEntropy);
            
            return unprotectedTokenBytes;
        }
    }
}
