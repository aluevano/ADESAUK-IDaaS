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
using IdentityServer.Core.ResponseHandling;
using IdentityServer.Core.Results;
using IdentityServer.Core.Services;
using IdentityServer.Core.Validation;
using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Endpoints
{
    /// <summary>
    /// Endpoint for token introspection - see https://tools.ietf.org/html/draft-ietf-oauth-introspection-11
    /// </summary>
    [NoCache]
    internal class IntrospectionEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions _options;
        private readonly IEventService _events;
        private readonly ScopeSecretValidator _scopeSecretValidator;
        private readonly IntrospectionRequestValidator _requestValidator;
        private readonly IntrospectionResponseGenerator _generator;

        public IntrospectionEndpointController(
            IntrospectionRequestValidator requestValidator, 
            IdentityServerOptions options, 
            IEventService events,
            ScopeSecretValidator scopeSecretValidator,
            IntrospectionResponseGenerator generator)
        {
            _requestValidator = requestValidator;
            _scopeSecretValidator = scopeSecretValidator;
            _options = options;
            _events = events;
            _generator = generator;
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <returns>Introspection response</returns>
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start introspection request");

            var scope = await _scopeSecretValidator.ValidateAsync();
            if (scope.Scope == null)
            {
                Logger.Warn("Scope unauthorized to call introspection endpoint. aborting.");
                return Unauthorized();
            }

            var parameters = await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync();
            return await ProcessRequest(parameters, scope.Scope);
        }

        internal async Task<IHttpActionResult> ProcessRequest(NameValueCollection parameters, Scope scope)
        {
            var validationResult = await _requestValidator.ValidateAsync(parameters, scope);
            var response = await _generator.ProcessAsync(validationResult, scope);

            if (validationResult.IsActive)
            {
                await RaiseSuccessEventAsync(validationResult.Token, "active", scope.Name);    
                return new IntrospectionResult(response);
            }

            if (validationResult.IsError)
            {
                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.MissingToken)
                {
                    Logger.Error("Missing token");

                    await RaiseFailureEventAsync(validationResult.ErrorDescription, validationResult.Token, scope.Name);
                    return BadRequest("missing_token");
                }

                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.InvalidToken)
                {
                    await RaiseSuccessEventAsync(validationResult.Token, "inactive", scope.Name);
                    return new IntrospectionResult(response);
                }

                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.InvalidScope)
                {
                    await RaiseFailureEventAsync("Scope not authorized to introspect token", validationResult.Token, scope.Name);
                    return new IntrospectionResult(response);
                }
            }

            throw new InvalidOperationException("Invalid token introspection outcome");
        }

        private async Task RaiseSuccessEventAsync(string token, string tokenStatus, string scopeName)
        {
            await _events.RaiseSuccessfulIntrospectionEndpointEventAsync(
                token, 
                tokenStatus, 
                scopeName);
        }

        private async Task RaiseFailureEventAsync(string error, string token, string scopeName)
        {
            await _events.RaiseFailureIntrospectionEndpointEventAsync(
                error, token, scopeName);
        }
    }
}