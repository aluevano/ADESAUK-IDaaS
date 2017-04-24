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
using IdentityServer.Core.ResponseHandling;
using IdentityServer.Core.Results;
using IdentityServer.Core.Services;
using IdentityServer.Core.Validation;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Endpoints
{
    /// <summary>
    /// OAuth2/OpenID Conect token endpoint
    /// </summary>
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    internal class TokenEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly TokenResponseGenerator _generator;
        private readonly TokenRequestValidator _requestValidator;
        private readonly ClientSecretValidator _clientValidator;
        private readonly IdentityServerOptions _options;
        private readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenEndpointController" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="requestValidator">The request validator.</param>
        /// <param name="clientValidator">The client validator.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="events">The events service.</param>
        public TokenEndpointController(IdentityServerOptions options, TokenRequestValidator requestValidator, ClientSecretValidator clientValidator, TokenResponseGenerator generator, IEventService events)
        {
            _requestValidator = requestValidator;
            _clientValidator = clientValidator;
            _generator = generator;
            _options = options;
            _events = events;
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <returns>Token response</returns>
        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start token request");

            var response = await ProcessAsync(await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync());
            
            if (response is TokenErrorResult)
            {
                var details = response as TokenErrorResult;
                await RaiseFailureEventAsync(details.Error);
            }
            else
            {
                await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.Token);
            }

            Logger.Info("End token request");
            return response;
        }

        /// <summary>
        /// Processes the token request
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Token response</returns>
        public async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            // validate client credentials and client
            var clientResult = await _clientValidator.ValidateAsync();
            if (clientResult.IsError)
            {
                return this.TokenErrorResponse(Constants.TokenErrors.InvalidClient);
            }

            // validate the token request
            var requestResult = await _requestValidator.ValidateRequestAsync(parameters, clientResult.Client);

            if (requestResult.IsError)
            {
                return this.TokenErrorResponse(requestResult.Error, requestResult.ErrorDescription);
            }

            // return response
            var response = await _generator.ProcessAsync(_requestValidator.ValidatedRequest);
            return this.TokenResponse(response);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.Token, error);
        }
    }
}