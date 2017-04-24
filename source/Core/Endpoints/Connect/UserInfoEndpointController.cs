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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Endpoints
{
    /// <summary>
    /// OpenID Connect userinfo endpoint
    /// </summary>
    [NoCache]
    internal class UserInfoEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly UserInfoResponseGenerator _generator;
        private readonly TokenValidator _tokenValidator;
        private readonly BearerTokenUsageValidator _tokenUsageValidator;
        private readonly IdentityServerOptions _options;
        private readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoEndpointController"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="tokenValidator">The token validator.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="tokenUsageValidator">The token usage validator.</param>
        /// <param name="events">The event service</param>
        public UserInfoEndpointController(IdentityServerOptions options, TokenValidator tokenValidator, UserInfoResponseGenerator generator, BearerTokenUsageValidator tokenUsageValidator, IEventService events)
        {
            _tokenValidator = tokenValidator;
            _generator = generator;
            _options = options;
            _tokenUsageValidator = tokenUsageValidator;
            _events = events;
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>userinfo response</returns>
        [HttpGet, HttpPost]
        public async Task<IHttpActionResult> GetUserInfo(HttpRequestMessage request)
        {
            Logger.Info("Start userinfo request");

            var tokenUsageResult = await _tokenUsageValidator.ValidateAsync(request.GetOwinContext());
            if (tokenUsageResult.TokenFound == false)
            {
                var error = "No token found.";

                Logger.Error(error);
                await RaiseFailureEventAsync(error);
                return Error(Constants.ProtectedResourceErrors.InvalidToken);
            }

            Logger.Info("Token found: " + tokenUsageResult.UsageType.ToString());

            var tokenResult = await _tokenValidator.ValidateAccessTokenAsync(
                tokenUsageResult.Token,
                Constants.StandardScopes.OpenId);

            if (tokenResult.IsError)
            {
                Logger.Error(tokenResult.Error);
                await RaiseFailureEventAsync(tokenResult.Error);
                return Error(tokenResult.Error);
            }

            // pass scopes/claims to profile service
            var tokenClaims = tokenResult.Claims;
            if (!tokenClaims.Any(x=>x.Type == Constants.ClaimTypes.Subject))
            {
                var error = "Token contains no sub claim";
                Logger.Error(error);
                await RaiseFailureEventAsync(error);
                return Error(Constants.ProtectedResourceErrors.InvalidToken);
            }


            var userClaims = tokenClaims.Where(x => !Constants.OidcProtocolClaimTypes.Contains(x.Type) ||
                Constants.AuthenticateResultClaimTypes.Contains(x.Type));
            var scopes = tokenResult.Claims.Where(c => c.Type == Constants.ClaimTypes.Scope).Select(c => c.Value);

            var payload = await _generator.ProcessAsync(userClaims, scopes, tokenResult.Client);

            Logger.Info("End userinfo request");
            await RaiseSuccessEventAsync();

            return new UserInfoResult(payload);
        }

        IHttpActionResult Error(string error, string description = null)
        {
            return new ProtectedResourceErrorResult(error, description);
        }

        private async Task RaiseSuccessEventAsync()
        {
            await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.UserInfo);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            if (_options.EventsOptions.RaiseFailureEvents)
            {
                await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.UserInfo, error);
            }
        }
    }
}