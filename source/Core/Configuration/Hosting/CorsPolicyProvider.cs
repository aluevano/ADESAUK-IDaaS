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

using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Services;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.Configuration.Hosting
{
    internal class CorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        readonly string[] paths;

        public CorsPolicyProvider(IEnumerable<string> allowedPaths)
        {
            if (allowedPaths == null) throw new ArgumentNullException("allowedPaths");

            this.paths = allowedPaths.Select(Normalize).ToArray();
        }

        public async Task<System.Web.Cors.CorsPolicy> GetCorsPolicyAsync(IOwinRequest request)
        {
            var path = request.Path.ToString();
            var origin = request.Headers["Origin"];

            // see if the Origin is different than this server's origin. if so
            // that indicates a proper CORS request
            var ctx = new OwinContext(request.Environment);
            // using GetIdentityServerHost takes into account a configured PublicOrigin
            var thisOrigin = ctx.GetIdentityServerHost();
            if (origin != null && origin != thisOrigin)
            {
                if (IsPathAllowed(request))
                {
                    Logger.InfoFormat("CORS request made for path: {0} from origin: {1}", path, origin);

                    if (await IsOriginAllowed(origin, request.Environment))
                    {
                        Logger.Info("CorsPolicyService allowed origin");
                        return Allow(origin);
                    }
                    else
                    {
                        Logger.Info("CorsPolicyService did not allow origin");
                    }
                }
                else
                {
                    Logger.InfoFormat("CORS request made for path: {0} from origin: {1} but rejected because invalid CORS path", path, origin);
                }
            }

            return null;
        }

        protected virtual async Task<bool> IsOriginAllowed(string origin, IDictionary<string, object> env)
        {
            var corsPolicy = env.ResolveDependency<ICorsPolicyService>();
            return await corsPolicy.IsOriginAllowedAsync(origin);
        }

        private bool IsPathAllowed(IOwinRequest request)
        {
            var requestPath = Normalize(request.Path.Value);
            return paths.Any(path => requestPath.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        private string Normalize(string path)
        {
            if (String.IsNullOrWhiteSpace(path) || path == "/")
            {
                path = "/";
            }
            else
            {
                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                if (path.EndsWith("/"))
                {
                    path = path.Substring(0, path.Length - 1);
                }
            }
            
            return path;
        }

        System.Web.Cors.CorsPolicy Allow(string origin)
        {
            var p = new System.Web.Cors.CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
            };

            p.Origins.Add(origin);
            return p;
        }
    }
}
