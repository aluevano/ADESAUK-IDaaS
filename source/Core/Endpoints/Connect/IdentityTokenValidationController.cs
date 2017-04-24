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
using IdentityServer.Core.Events;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Resources;
using IdentityServer.Core.Services;
using IdentityServer.Core.Validation;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Endpoints
{
    /// <summary>
    /// Endpoint for validating identity tokens
    /// </summary>
    [NoCache]
    internal class IdentityTokenValidationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly TokenValidator _validator;
        private readonly IdentityServerOptions _options;
        private readonly ILocalizationService _localizationService;
        private readonly IEventService _events;

        public IdentityTokenValidationController(TokenValidator validator, IdentityServerOptions options, ILocalizationService localizationService, IEventService events)
        {
            _validator = validator;
            _options = options;
            _localizationService = localizationService;
            _events = events;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <returns>Claims if token is valid</returns>
        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            Logger.Info("Start identity token validation request");

            var parameters = Request.RequestUri.ParseQueryString();
            return await ProcessAsync(parameters);
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <returns>Claims if token is valid</returns>
        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start identity token validation request");

            var parameters = await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync();
            return await ProcessAsync(parameters);
        }

        internal async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            var token = parameters.Get("token");
            if (token.IsMissing())
            {
                var error = "token is missing.";
                Logger.Error(error);
                await RaiseFailureEventAsync(error);

                return BadRequest(_localizationService.GetMessage(MessageIds.MissingToken));
            }

            var clientId = parameters.Get("client_id");
            if (clientId.IsMissing())
            {
                var error = "client_id is missing.";
                Logger.Error(error);
                await RaiseFailureEventAsync(error);

                return BadRequest(_localizationService.GetMessage(MessageIds.MissingClientId));
            }

            var result = await _validator.ValidateIdentityTokenAsync(token, clientId);

            if (result.IsError)
            {
                Logger.Info("Returning error: " + result.Error);
                await RaiseFailureEventAsync(result.Error);

                return BadRequest(result.Error);
            }

            var response = result.Claims.ToClaimsDictionary();

            Logger.Info("End identity token validation request");
            await RaiseSuccessEventAsync();

            return Json(response);
        }

        private async Task RaiseSuccessEventAsync()
        {
            await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.IdentityTokenValidation);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.IdentityTokenValidation, error);
        }
    }
}