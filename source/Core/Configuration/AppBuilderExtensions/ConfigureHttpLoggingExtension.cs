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
using IdentityServer.Core.Logging;
using Microsoft.Owin;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Owin
{
    static class ConfigureHttpLoggingExtension
    {
        static readonly ILog Logger = LogProvider.GetLogger("HTTP Logging");

        public static IAppBuilder ConfigureHttpLogging(this IAppBuilder app, LoggingOptions options)
        {
            if (options.EnableHttpLogging)
            {
                app.Use(async (ctx, next) =>
                {
                    await LogRequest(ctx.Request);

                    var oldStream = ctx.Response.Body;
                    var ms = ctx.Response.Body = new MemoryStream();

                    try
                    {
                        await next();
                        await LogResponse(ctx.Response);

                        ctx.Response.Body = oldStream;
                        await ms.CopyToAsync(oldStream);
                    }
                    catch(Exception ex)
                    {
                        Logger.DebugException("HTTP Response Exception", ex);
                        throw;
                    }
                });
            }

            return app;
        }

        private static async Task LogRequest(IOwinRequest request)
        {
            var reqLog = new
            {
                Method = request.Method,
                Url = request.Uri.AbsoluteUri,
                Headers = request.Headers,
                Body = await request.ReadBodyAsStringAsync()
            };

            Logger.Debug("HTTP Request" + Environment.NewLine + LogSerializer.Serialize(reqLog));
        }

        private static async Task LogResponse(IOwinResponse response)
        {
            var respLog = new
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers,
                Body = await response.ReadBodyAsStringAsync()
            };

            Logger.Debug("HTTP Response" + Environment.NewLine + LogSerializer.Serialize(respLog));
        }
    }
}
