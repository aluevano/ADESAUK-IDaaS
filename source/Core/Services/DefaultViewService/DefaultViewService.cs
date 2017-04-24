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
using IdentityServer.Core.Models;
using IdentityServer.Core.Validation;
using IdentityServer.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default view service.
    /// </summary>
    public class DefaultViewService : IViewService
    {
        /// <summary>
        /// The login view
        /// </summary>
        public const string LoginView = "login";
        /// <summary>
        /// The logout view
        /// </summary>
        public const string LogoutView = "logout";
        /// <summary>
        /// The logged out view
        /// </summary>
        public const string LoggedOutView = "loggedOut";
        /// <summary>
        /// The consent view
        /// </summary>
        public const string ConsentView = "consent";
        /// <summary>
        /// The client permissions view
        /// </summary>
        public const string ClientPermissionsView = "permissions";
        /// <summary>
        /// The error view
        /// </summary>
        public const string ErrorView = "error";
        /// <summary>
        /// The authorize response view
        /// </summary>
        public const string AuthorizeResponseView = "authorizeresponse";

        static readonly Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings()
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// The configuration
        /// </summary>
        protected readonly DefaultViewServiceOptions config;
        
        /// <summary>
        /// The view loader
        /// </summary>
        protected readonly IViewLoader viewLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewService"/> class.
        /// </summary>
        public DefaultViewService(DefaultViewServiceOptions config, IViewLoader viewLoader)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (viewLoader == null) throw new ArgumentNullException("viewLoader");

            this.config = config;
            this.viewLoader = viewLoader;
        }

        /// <summary>
        /// Loads the HTML for the login page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Login(LoginViewModel model, SignInMessage message)
        {
            return Render(model, LoginView);
        }

        /// <summary>
        /// Loads the HTML for the logout prompt page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Logout(LogoutViewModel model, SignOutMessage message)
        {
            return Render(model, LogoutView);
        }

        /// <summary>
        /// Loads the HTML for the logged out page informing the user that they have successfully logged out.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
            return Render(model, LoggedOutView);
        }

        /// <summary>
        /// Loads the HTML for the user consent page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="authorizeRequest">The validated authorize request.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest)
        {
            return Render(model, ConsentView);
        }

        /// <summary>
        /// Loads the HTML for the client permissions page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public Task<Stream> ClientPermissions(ClientPermissionsViewModel model)
        {
            return Render(model, ClientPermissionsView);
        }

        /// <summary>
        /// Loads the HTML for the error page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Error(ErrorViewModel model)
        {
            return Render(model, ErrorView);
        }

         /// <summary>
        /// Loads the HTML for the authorize response page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual async Task<Stream> AuthorizeResponse(AuthorizeResponseViewModel model)
        {
            var newModel = new CommonViewModel
            {
                SiteName = model.SiteName,
                SiteUrl = model.SiteUrl,
                Custom = model.Custom
            };

            var scripts = new List<string>();
            scripts.AddRange(config.Scripts ?? Enumerable.Empty<string>());
            scripts.Add("~/assets/app.FormPostResponse.js");

            var data = BuildModelDictionary(newModel, AuthorizeResponseView, config.Stylesheets, scripts);
            data.Add("responseUri", model.ResponseFormUri);
            data.Add("responseFields", model.ResponseFormFields);

            string html = await LoadHtmlTemplate(AuthorizeResponseView);
            html = FormatHtmlTemplate(html, data);

            return html.ToStream();
        }

        /// <summary>
        /// Renders the specified page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        protected virtual Task<Stream> Render(CommonViewModel model, string page)
        {
            return Render(model, page, config.Stylesheets, config.Scripts);
        }

        /// <summary>
        /// Renders the specified page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="page">The page.</param>
        /// <param name="stylesheets">The stylesheets.</param>
        /// <param name="scripts">The scripts.</param>
        /// <returns></returns>
        protected virtual async Task<Stream> Render(CommonViewModel model, string page, IEnumerable<string> stylesheets, IEnumerable<string> scripts)
        {
            var data = BuildModelDictionary(model, page, stylesheets, scripts);

            string html = await LoadHtmlTemplate(page);
            if (html == null) return null;

            html = FormatHtmlTemplate(html, data);

            return html.ToStream();
        }

        /// <summary>
        /// Loads the HTML template.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        protected virtual Task<string> LoadHtmlTemplate(string page)
        {
            return this.viewLoader.LoadAsync(page);
        }

        /// <summary>
        /// Formats the specified HTML template.
        /// </summary>
        /// <param name="htmlTemplate">The HTML template.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected string FormatHtmlTemplate(string htmlTemplate, object model)
        {
            return AssetManager.Format(htmlTemplate, model);
        }

        /// <summary>
        /// Builds the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="page">The page.</param>
        /// <param name="stylesheets">The stylesheets.</param>
        /// <param name="scripts">The scripts.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// model
        /// or
        /// stylesheets
        /// or
        /// scripts
        /// </exception>
        protected object BuildModel(CommonViewModel model, string page, IEnumerable<string> stylesheets, IEnumerable<string> scripts)
        {
            return BuildModelDictionary(model, page, stylesheets, scripts);
        }

        Dictionary<string, object> BuildModelDictionary(CommonViewModel model, string page, IEnumerable<string> stylesheets, IEnumerable<string> scripts)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (stylesheets == null) throw new ArgumentNullException("stylesheets");
            if (scripts == null) throw new ArgumentNullException("scripts");

            var applicationPath = new Uri(model.SiteUrl).AbsolutePath;
            if (applicationPath.EndsWith("/")) applicationPath = applicationPath.Substring(0, applicationPath.Length - 1);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None, settings);

            var additionalStylesheets = BuildTags("<link href='{0}' rel='stylesheet'>", applicationPath, stylesheets);
            var additionalScripts = BuildTags("<script src='{0}'></script>", applicationPath, scripts);

            return new Dictionary<string, object>
            {
                { "siteName" , Microsoft.Security.Application.Encoder.HtmlEncode(model.SiteName) },
                { "applicationPath", applicationPath },
                { "model", Microsoft.Security.Application.Encoder.HtmlEncode(json) },
                { "page", page },
                { "stylesheets", additionalStylesheets },
                { "scripts", additionalScripts }
            };
        }

        string BuildTags(string tagFormat, string basePath, IEnumerable<string> values)
        {
            if (values == null || !values.Any()) return String.Empty;

            var sb = new StringBuilder();
            foreach (var value in values)
            {
                var path = value;
                if (path.StartsWith("~/"))
                {
                    path = basePath + path.Substring(1);
                }
                sb.AppendFormat(tagFormat, path);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
