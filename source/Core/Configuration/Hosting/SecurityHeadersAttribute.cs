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
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;

namespace IdentityServer.Core.Configuration.Hosting
{
    internal class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public SecurityHeadersAttribute()
        {
            EnableXfo = true;
            EnableCto = true;
            EnableCsp = true;
        }

        public bool EnableXfo { get; set; }
        public bool EnableCto { get; set; }
        public bool EnableCsp { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (actionExecutedContext != null &&
                actionExecutedContext.Response != null &&
                actionExecutedContext.Response.IsSuccessStatusCode &&
                (actionExecutedContext.Response.Content == null ||
                 "text/html".Equals(actionExecutedContext.Response.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase))
            )
            {
                if (EnableCto)
                {
                    actionExecutedContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                }

                if (EnableXfo && actionExecutedContext.Request.GetSuppressXfo() == false)
                {
                    actionExecutedContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                }

                if (EnableCsp)
                {
                    var ctx = actionExecutedContext.Request.GetOwinContext();
                    var options = ctx.ResolveDependency<IdentityServerOptions>();
                    if (options.CspOptions.Enabled)
                    {
                        // img-src as * due to client logos
                        var value = "default-src 'self'; script-src 'self' {0}; style-src 'self' 'unsafe-inline' {1}; img-src {2}; ";

                        value = String.Format(value, 
                            options.CspOptions.ScriptSrc, 
                            options.CspOptions.StyleSrc, 
                            options.CspOptions.ImgSrc ?? "*");

                        if (!String.IsNullOrWhiteSpace(options.CspOptions.FontSrc))
                        {
                            value += String.Format("font-src {0};", options.CspOptions.FontSrc);
                        }

                        if (!String.IsNullOrWhiteSpace(options.CspOptions.ConnectSrc))
                        {
                            value += String.Format("connect-src {0};", options.CspOptions.ConnectSrc);
                        }

                        var iframesOrigins = actionExecutedContext.Request.GetAllowedCspFrameOrigins();
                        if (iframesOrigins.Any() || !String.IsNullOrWhiteSpace(options.CspOptions.FrameSrc))
                        {
                            var frameSrc = options.CspOptions.FrameSrc;
                            if (iframesOrigins.Any())
                            {
                                frameSrc += " ";
                                frameSrc += iframesOrigins.Aggregate((x, y) => x + " " + y);
                            }
                            value += String.Format("frame-src 'self' {0};", frameSrc);
                        }

                        if (options.Endpoints.EnableCspReportEndpoint)
                        {
                            value += " report-uri " + ctx.GetCspReportUrl();
                        }
                        
                        // once for standards compliant browsers
                        actionExecutedContext.Response.Headers.Add("Content-Security-Policy", value);
                        // and once again for IE
                        actionExecutedContext.Response.Headers.Add("X-Content-Security-Policy", value);
                    }
                }
            }
        }
    }
}