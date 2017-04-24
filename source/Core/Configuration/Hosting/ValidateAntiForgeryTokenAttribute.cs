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
using IdentityServer.Core.Results;
using IdentityServer.Core.Services;
using IdentityServer.Core.ViewModels;
using Microsoft.Owin;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace IdentityServer.Core.Configuration.Hosting
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    internal class ValidateAntiForgeryTokenAttribute : PreventUnsupportedRequestMediaTypesAttribute
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public ValidateAntiForgeryTokenAttribute()
            : base(allowFormUrlEncoded:true)
        {
        }

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // first check for 415
            await base.OnAuthorizationAsync(actionContext, cancellationToken);

            if (actionContext.Response == null)
            {
                await ValidateTokens(actionContext);
            }
        }

        private static async Task ValidateTokens(HttpActionContext actionContext)
        {
            var env = actionContext.Request.GetOwinEnvironment();

            var success = actionContext.Request.Method == HttpMethod.Post &&
                          actionContext.Request.Content.IsFormData();
            if (success)
            {
                // ReadAsByteArrayAsync buffers the request body stream
                // so Web API will re-use that later for model binding
                // unfortunately the stream pointer is at the end, but 
                // in our anti-forgery logic we use our internal ReadRequestFormAsync
                // API to read the body, which has the side effect of resetting
                // the stream pointer to the begining. subsequet calls to 
                // read the form body will then succeed (e.g. via OwinContext)
                // this is all rather unfortunate that web api prevents others
                // from re-reading the form, but this sequence of code allow it. #lame
                var bytes = await actionContext.Request.Content.ReadAsByteArrayAsync();

                var antiForgeryToken = env.ResolveDependency<AntiForgeryToken>();
                success = await antiForgeryToken.IsTokenValid();
            }

            if (!success)
            {
                Logger.ErrorFormat("AntiForgery validation failed -- returning error page");

                var options = env.ResolveDependency<IdentityServerOptions>();
                var viewSvc = env.ResolveDependency<IViewService>();
                var localization = env.ResolveDependency<ILocalizationService>();

                var errorModel = new ErrorViewModel
                {
                    RequestId = env.GetRequestId(),
                    SiteName = options.SiteName,
                    SiteUrl = env.GetIdentityServerBaseUrl(),
                    ErrorMessage = localization.GetMessage(Resources.MessageIds.UnexpectedError),
                    CurrentUser = env.GetCurrentUserDisplayName(),
                    LogoutUrl = env.GetIdentityServerLogoutUrl(),
                };
                var errorResult = new ErrorActionResult(viewSvc, errorModel);
                actionContext.Response = await errorResult.GetResponseMessage();
            }
        }
    }
}
