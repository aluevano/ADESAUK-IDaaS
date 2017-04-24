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
using IdentityServer.Core.Configuration.Hosting;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Results
{
    internal class LogoutResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignOutMessage message;
        private readonly IDictionary<string, object> env;
        private readonly IdentityServerOptions options;

        public static string GetRedirectUrl(SignOutMessage message, IDictionary<string, object> env, IdentityServerOptions options)
        {
            var result = new LogoutResult(message, env, options);
            var response = result.Execute();

            return response.Headers.Location.AbsoluteUri;
        }

        public LogoutResult(SignOutMessage message, IDictionary<string, object> env, IdentityServerOptions options)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (options == null) throw new ArgumentNullException("options");

            this.env = env;
            this.options = options;

            this.message = message;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            Logger.Info("Redirecting to logout page");

            var url = env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Logout;

            if (message != null)
            {
                var cookie = new MessageCookie<SignOutMessage>(this.env, this.options);
                var id = cookie.Write(this.message);

                url = url.AddQueryString("id=" + id);
            }

            var uri = new Uri(url);

            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = uri;
            return response;
        }
    }
}