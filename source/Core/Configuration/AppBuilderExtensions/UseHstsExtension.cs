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

using System;

namespace Owin
{
    /// <summary>
    /// Configure extensions for HSTS support
    /// </summary>
    public static class UseHstsExtension
    {
        /// <summary>
        /// Enables HTTP Strict Transport Security (HSTS) for the hosting application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="duration">The duration the HSTS header should be cached in the client browser. <c>TimeSpan.Zero</c> will clear the cached value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        /// <exception cref="System.ArgumentException">duration cannot be below zero</exception>
        public static IAppBuilder UseHsts(this IAppBuilder app, TimeSpan duration)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (duration < TimeSpan.Zero) throw new ArgumentException("duration cannot be below zero");

            if (duration >= TimeSpan.Zero)
            {
                var seconds = (int)duration.TotalSeconds;
                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.IsSecure)
                    {
                        ctx.Response.Headers.Append("Strict-Transport-Security", "max-age=" + seconds);
                    }
                    await next();
                });
            }

            return app;
        }

        /// <summary>
        /// Enables HTTP Strict Transport Security (HSTS) for the hosting application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="days">The number of days the HSTS header should be cached in the client browser. A value of zero will clear the cached value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">days cannot be below zero</exception>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder UseHsts(this IAppBuilder app, int days = 30)
        {
            if (days < 0) throw new ArgumentException("days cannot be below zero");

            return app.UseHsts(TimeSpan.FromDays(days));
        }
    }
}