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

using Autofac;
using Autofac.Integration.WebApi;
using Autofac.Util;
using IdentityServer.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;

namespace IdentityServer.Core.Configuration.Hosting
{
    internal static class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityServerOptions options, ILifetimeScope container)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            config.SuppressDefaultHostAuthentication();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            config.Services.Add(typeof(IExceptionLogger), new LogProviderExceptionLogger());
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new HttpControllerTypeResolver());
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;

            if (options.LoggingOptions.EnableWebApiDiagnostics)
            {
                var liblog = new TraceSource("LibLog");
                liblog.Switch.Level = SourceLevels.All;
                liblog.Listeners.Add(new LibLogTraceListener());

                var diag = config.EnableSystemDiagnosticsTracing();
                diag.IsVerbose = options.LoggingOptions.WebApiDiagnosticsIsVerbose;
                diag.TraceSource = liblog;
            }

            ConfigureRoutes(options, config);

            return config;
        }

        private static void ConfigureRoutes(IdentityServerOptions options, HttpConfiguration config)
        {
            if (options.EnableWelcomePage)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Welcome, 
                    Constants.RoutePaths.Welcome, 
                    new { controller = "Welcome", action = "Get" });
            }

            if (options.Endpoints.EnableAccessTokenValidationEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.AccessTokenValidation,
                    Constants.RoutePaths.Oidc.AccessTokenValidation,
                    new { controller = "AccessTokenValidation" });
            }

            if (options.Endpoints.EnableIntrospectionEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Introspection,
                    Constants.RoutePaths.Oidc.Introspection,
                    new { controller = "IntrospectionEndpoint" });
            }

            if (options.Endpoints.EnableAuthorizeEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Authorize,
                    Constants.RoutePaths.Oidc.Authorize,
                    new { controller = "AuthorizeEndpoint", action = "Get" });
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Consent,
                    Constants.RoutePaths.Oidc.Consent,
                    new { controller = "AuthorizeEndpoint", action = "PostConsent" });
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.SwitchUser,
                    Constants.RoutePaths.Oidc.SwitchUser,
                    new { controller = "AuthorizeEndpoint", action = "LoginAsDifferentUser" });
            }

            if (options.Endpoints.EnableCheckSessionEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.CheckSession,
                    Constants.RoutePaths.Oidc.CheckSession,
                    new { controller = "CheckSessionEndpoint" });
            }

            if (options.Endpoints.EnableClientPermissionsEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.ClientPermissions,
                    Constants.RoutePaths.ClientPermissions,
                    new { controller = "ClientPermissions" });
            }

            if (options.Endpoints.EnableCspReportEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.CspReport,
                    Constants.RoutePaths.CspReport,
                    new { controller = "CspReport" });
            }

            if (options.Endpoints.EnableDiscoveryEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.DiscoveryConfiguration,
                    Constants.RoutePaths.Oidc.DiscoveryConfiguration,
                    new { controller = "DiscoveryEndpoint", action = "GetConfiguration" });
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.DiscoveryWebKeys,
                    Constants.RoutePaths.Oidc.DiscoveryWebKeys,
                    new { controller = "DiscoveryEndpoint", action= "GetKeyData" });
            }

            if (options.Endpoints.EnableEndSessionEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.EndSession,
                    Constants.RoutePaths.Oidc.EndSession,
                    new { controller = "EndSession", action = "Logout" });
            }
            
            // this one is always enabled/allowed (for use by our logout page)
            config.Routes.MapHttpRoute(
                Constants.RouteNames.Oidc.EndSessionCallback,
                Constants.RoutePaths.Oidc.EndSessionCallback,
                new { controller = "EndSession", action = "LogoutCallback" });

            if (options.Endpoints.EnableIdentityTokenValidationEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.IdentityTokenValidation,
                    Constants.RoutePaths.Oidc.IdentityTokenValidation,
                    new { controller = "IdentityTokenValidation" });
            }

            if (options.Endpoints.EnableTokenEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Token,
                    Constants.RoutePaths.Oidc.Token,
                    new { controller = "TokenEndpoint", action= "Post" });
            }

            if (options.Endpoints.EnableTokenRevocationEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Revocation,
                    Constants.RoutePaths.Oidc.Revocation,
                    new { controller = "RevocationEndpoint", action = "Post" });
            }

            if (options.Endpoints.EnableUserInfoEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.UserInfo,
                    Constants.RoutePaths.Oidc.UserInfo,
                    new { controller = "UserInfoEndpoint" });
            }
        }

        private class HttpControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver _)
            {
                var httpControllerType = typeof (IHttpController);
                return typeof (WebApiConfig)
                    .Assembly
                    .GetLoadableTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && httpControllerType.IsAssignableFrom(t))
                    .ToList();
            }
        }

    }
}
