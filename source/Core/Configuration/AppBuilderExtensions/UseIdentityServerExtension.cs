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
using IdentityServer.Core;
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Configuration.Hosting;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Services;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Owin
{
    /// <summary>
    /// Configuration extensions for identity server
    /// </summary>
    public static class UseIdentityServerExtension
    {
        private static readonly ILog Logger = LogProvider.GetLogger("Startup");

        /// <summary>
        /// Extension method to configure IdentityServer in the hosting application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="options">The <see cref="IdentityServer.Core.Configuration.IdentityServerOptions"/>.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// app
        /// or
        /// options
        /// </exception>
        public static IAppBuilder UseIdentityServer(this IAppBuilder app, IdentityServerOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("options");

            options.Validate();

            // turn off weird claim mappings for JWTs
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            JwtSecurityTokenHandler.OutboundClaimTypeMap = new Dictionary<string, string>();

            if (options.RequireSsl)
            {
                app.Use<RequireSslMiddleware>();
            }

            if (options.LoggingOptions.EnableKatanaLogging)
            {
                app.SetLoggerFactory(new LibLogKatanaLoggerFactory());
            }

            app.UseEmbeddedFileServer();

            app.ConfigureRequestId();
            app.ConfigureDataProtectionProvider(options);
            app.ConfigureIdentityServerBaseUrl(options.PublicOrigin);
            app.ConfigureIdentityServerIssuer(options);

            app.ConfigureRequestBodyBuffer();

            // this needs to be earlier than the autofac middleware so anything is disposed and re-initialized
            // if we send the request back into the pipeline to render the logged out page
            app.ConfigureRenderLoggedOutPage();

            var container = AutofacConfig.Configure(options);
            app.UseAutofacMiddleware(container);

            app.UseCors();
            app.ConfigureCookieAuthentication(options.AuthenticationOptions.CookieOptions, options.DataProtector);

            // this needs to be before external middleware
            app.ConfigureSignOutMessageCookie();


            if (options.PluginConfiguration != null)
            {
                options.PluginConfiguration(app, options);
            }

            if (options.AuthenticationOptions.IdentityProviders != null)
            {
                options.AuthenticationOptions.IdentityProviders(app, Constants.ExternalAuthenticationType);
            }

            app.ConfigureHttpLogging(options.LoggingOptions);

            SignatureConversions.AddConversions(app);
            
            var httpConfig = WebApiConfig.Configure(options, container);
            app.UseAutofacWebApi(httpConfig);
            app.UseWebApi(httpConfig);

            using (var child = container.CreateScopeWithEmptyOwinContext())
            {
                var eventSvc = child.Resolve<IEventService>();
                // TODO -- perhaps use AsyncHelper instead?
                DoStartupDiagnosticsAsync(options, eventSvc).Wait();
            }
            
            return app;
        }

        private static async Task DoStartupDiagnosticsAsync(IdentityServerOptions options, IEventService eventSvc)
        {
            var cert = options.SigningCertificate;
            
            if (cert == null)
            {
                Logger.Warn("No signing certificate configured.");
                await eventSvc.RaiseNoCertificateConfiguredEventAsync();

                return;
            }
            if (!cert.HasPrivateKey || !cert.IsPrivateAccessAllowed())
            {
                Logger.Error("Signing certificate has no private key or the private key is not accessible. Make sure the account running your application has access to the private key");
                await eventSvc.RaiseCertificatePrivateKeyNotAccessibleEventAsync(cert);

                return;
            }
            if (cert.PublicKey.Key.KeySize < 2048)
            {
                Logger.Error("Signing certificate key length is less than 2048 bits.");
                await eventSvc.RaiseCertificateKeyLengthTooShortEventAsync(cert);

                return;
            }

            var timeSpanToExpire = cert.NotAfter - DateTimeHelper.UtcNow;
            if (timeSpanToExpire < TimeSpan.FromDays(30))
            {
                Logger.Warn("The signing certificate will expire in the next 30 days: " + cert.NotAfter.ToString());
                await eventSvc.RaiseCertificateExpiringSoonEventAsync(cert);

                return;
            }

            await eventSvc.RaiseCertificateValidatedEventAsync(cert);
        }
    }
}
