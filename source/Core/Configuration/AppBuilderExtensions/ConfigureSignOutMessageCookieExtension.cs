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

using IdentityServer.Core.Configuration.Hosting;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Models;
using System;

namespace Owin
{
    internal static class SignOutMessageCookieExtension
    {
        public static IAppBuilder ConfigureSignOutMessageCookie(this IAppBuilder app)
        {
            if (app == null) throw new ArgumentNullException("app");

            return app.Use(async (context, next) =>
            {
                // need to do the ResolveDependency here before OnSendingHeaders
                // since OnSendingHeaders might run after DI and autofac have cleaned up
                var signOutMessageCookie = context.ResolveDependency<MessageCookie<SignOutMessage>>();

                context.Response.OnSendingHeaders(state =>
                {
                    // this is needed to remove sign out message cookie if we're not redirecting to upstream IdP
                    // we don't know until we're on the way out of the pipeline after the external IdP middleware
                    // has run. if we see our flag and 200 response, then we remove the cookie normally.
                    // if we don't see 200, then we leave it and expect a post logout callback from the upstream
                    // IdP at which point we can show our logged out page and finish processing the signout message
                    // which might contain client info, post logout redirects, etc.
                    context.ProcessRemovalOfSignOutMessageCookie(signOutMessageCookie);
                }, null);

                await next();
            });
        }
    }
}