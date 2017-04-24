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

using IdentityServer.Core.Configuration;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Services.Default;
using System.Net.Http;

namespace IdentityServer.Core.Results
{
    internal class CheckSessionResult : HtmlActionResult
    {
        private readonly IdentityServerOptions options;
        private readonly HttpRequestMessage request;

        public CheckSessionResult(IdentityServerOptions options, HttpRequestMessage request)
        {
            this.options = options;
            this.request = request;
        }

        protected override string GetHtml()
        {
            var root = request.GetIdentityServerBaseUrl();
            if (root.EndsWith("/")) root = root.Substring(0, root.Length - 1);

            return AssetManager.LoadCheckSession(root, options.AuthenticationOptions.CookieOptions.GetSessionCookieName());
        }
    }
}