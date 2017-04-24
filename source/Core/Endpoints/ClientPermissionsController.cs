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
using IdentityServer.Core.Resources;
using IdentityServer.Core.Results;
using IdentityServer.Core.Services;
using IdentityServer.Core.ViewModels;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Endpoints
{
    [ErrorPageFilter]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    internal class ClientPermissionsController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IClientPermissionsService clientPermissionsService;
        private readonly IdentityServerOptions options;
        private readonly IViewService viewSvc;
        private readonly ILocalizationService localizationService;
        private readonly IEventService eventService;
        private readonly AntiForgeryToken antiForgeryToken;

        public ClientPermissionsController(
            IClientPermissionsService clientPermissionsService, 
            IdentityServerOptions options, 
            IViewService viewSvc, 
            ILocalizationService localizationService,
            IEventService eventService,
            AntiForgeryToken antiForgeryToken)
        {
            this.clientPermissionsService = clientPermissionsService;
            this.options = options;
            this.viewSvc = viewSvc;
            this.localizationService = localizationService;
            this.eventService = eventService;
            this.antiForgeryToken = antiForgeryToken;
        }

        [HttpGet]
        public async Task<IHttpActionResult> ShowPermissions()
        {
            Logger.Info("Permissions page requested");

            if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
            {
                Logger.Info("User not authenticated, redirecting to login");
                return RedirectToLogin();
            }

            Logger.Info("Rendering permissions page");

            return await RenderPermissionsPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> RevokePermission(RevokeClientPermission model)
        {
            Logger.Info("Revoke permissions requested");
            
            if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
            {
                Logger.Info("User not authenticated, redirecting to login");
                return RedirectToLogin();
            }

            if (model != null && String.IsNullOrWhiteSpace(model.ClientId))
            {
                Logger.Warn("No model or client id submitted");
                ModelState.AddModelError("ClientId", localizationService.GetMessage(MessageIds.ClientIdRequired));
            }

            if (model == null || ModelState.IsValid == false)
            {
                var error = ModelState.Where(x => x.Value.Errors.Any()).Select(x => x.Value.Errors.First().ErrorMessage).First();
                Logger.WarnFormat("Rendering error: {0}", error);
                return await RenderPermissionsPage(error);
            }

            Logger.InfoFormat("Revoking permissions for sub: {0}, name: {1}, clientID: {2}", User.GetSubjectId(), User.Identity.Name, model.ClientId);
            
            await this.clientPermissionsService.RevokeClientPermissionsAsync(User.GetSubjectId(), model.ClientId);

            await eventService.RaiseClientPermissionsRevokedEventAsync(User as ClaimsPrincipal, model.ClientId);

            Logger.Info("Redirecting back to permissions page");

            var url = Request.GetOwinContext().GetIdentityServerBaseUrl().EnsureTrailingSlash() +
                Constants.RoutePaths.ClientPermissions;
            return Redirect(url);
        }

        private IHttpActionResult RedirectToLogin()
        {
            var message = new SignInMessage();

            var path = Url.Route(Constants.RouteNames.ClientPermissions, null);
            var host = new Uri(Request.GetOwinEnvironment().GetIdentityServerHost());
            var url = new Uri(host, path);
            message.ReturnUrl = url.AbsoluteUri;
            return new LoginResult(Request.GetOwinContext().Environment, message);
        }

        private async Task<IHttpActionResult> RenderPermissionsPage(string error = null)
        {
            var env = Request.GetOwinEnvironment();
            var clients = await this.clientPermissionsService.GetClientPermissionsAsync(User.GetSubjectId());
            var vm = new ClientPermissionsViewModel
            {
                RequestId = env.GetRequestId(),
                SiteName = options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                CurrentUser = env.GetCurrentUserDisplayName(),
                LogoutUrl = env.GetIdentityServerLogoutUrl(),
                RevokePermissionUrl = Request.GetOwinContext().GetPermissionsPageUrl(),
                AntiForgery = antiForgeryToken.GetAntiForgeryToken(),
                Clients = clients,
                ErrorMessage = error
            };
            return new ClientPermissionsActionResult(this.viewSvc, Request.GetOwinEnvironment(), vm);
        }
    }
}
