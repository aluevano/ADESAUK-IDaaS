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
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.Default;
using IdentityServer.Core.ViewModels;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer.Core.Results
{
    internal class AuthorizeFormPostResult : HtmlActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly AuthorizeResponse _response;
        private readonly HttpRequestMessage _request;

        public AuthorizeFormPostResult(AuthorizeResponse response, HttpRequestMessage request)
        {
            _response = response;
            _request = request;
        }

        protected override string GetHtml()
        {
            var root = _request.GetIdentityServerBaseUrl();
            if (root.EndsWith("/")) root = root.Substring(0, root.Length - 1);
            var fields = _response.ToNameValueCollection().ToFormPost();
            var redirect = _response.RedirectUri;

            return AssetManager.LoadFormPost(root, redirect, fields);
        }

        public override async Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            _request.SetSuppressXfo();

            Logger.Info("Posting to " + _response.RedirectUri);

            // see if we have a DefaultViewService for the IViewService 
            // to allow for customization of the authorize response page
            var ctx = _request.GetOwinContext();
            var defaultViewSvc = ctx.ResolveDependency<IViewService>() as DefaultViewService;
            if (defaultViewSvc != null)
            {
                Logger.Debug("Using DefaultViewService to render authorization response HTML");

                var vm = new AuthorizeResponseViewModel
                {
                    SiteName = _response.Request.Options.SiteName,
                    SiteUrl = _request.GetIdentityServerBaseUrl(),
                    ResponseFormUri = _response.RedirectUri,
                    ResponseFormFields = _response.ToNameValueCollection().ToFormPost()
                };

                var result = new HtmlStreamActionResult(() => defaultViewSvc.AuthorizeResponse(vm));
                return await result.ExecuteAsync(cancellationToken);
            }

            Logger.Debug("Using AssetManager to render authorization response HTML");
            return await base.ExecuteAsync(cancellationToken);
        }
    }
}