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
using Microsoft.Owin;
using System;

namespace Owin
{
    internal static class ConfigureIdentityServerIssuerExtension
    {
        // todo: remove this in 3.0.0 as it will be unnecessary. it's only being maintained now for backwards compat with 2.0 APIs.
        public static IAppBuilder ConfigureIdentityServerIssuer(this IAppBuilder app, IdentityServerOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("options");

            if (options.IssuerUri.IsPresent())
            {
                options.DynamicallyCalculatedIssuerUri = options.IssuerUri;
            }
            else
            { 
                Action<IOwinContext> op = ctx =>
                {
                    var uri = ctx.Environment.GetIdentityServerBaseUrl();
                    if (uri.EndsWith("/")) uri = uri.Substring(0, uri.Length - 1);
                    options.DynamicallyCalculatedIssuerUri = uri;
                };

                app.Use(async (ctx, next) =>
                {
                    if (op != null)
                    {
                        var tmp = op;
                        op = null;
                        tmp(ctx);
                    }

                    await next();
                });
            }

            return app;
        }
    }
}
