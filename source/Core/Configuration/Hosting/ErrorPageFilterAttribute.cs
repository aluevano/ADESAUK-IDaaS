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
using IdentityServer.Core.Resources;
using IdentityServer.Core.Results;
using IdentityServer.Core.Services;
using IdentityServer.Core.ViewModels;
using System.Net.Http;
using System.Web.Http.Filters;

namespace IdentityServer.Core.Configuration.Hosting
{
    internal class ErrorPageFilterAttribute : ExceptionFilterAttribute
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        public override async System.Threading.Tasks.Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, System.Threading.CancellationToken cancellationToken)
        {
            var env = actionExecutedContext.ActionContext.Request.GetOwinEnvironment();
            var options = env.ResolveDependency<IdentityServerOptions>();
            var viewSvc = env.ResolveDependency<IViewService>();
            var localization = env.ResolveDependency<ILocalizationService>();
            var errorModel = new ErrorViewModel
            {
                RequestId = env.GetRequestId(),
                SiteName = options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                ErrorMessage = localization.GetMessage(MessageIds.UnexpectedError),
                CurrentUser = env.GetCurrentUserDisplayName(),
                LogoutUrl = env.GetIdentityServerLogoutUrl(),
            };
            var errorResult = new ErrorActionResult(viewSvc, errorModel);
            actionExecutedContext.Response = await errorResult.GetResponseMessage();
        }
    }
}