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
using IdentityServer.Core.Models;
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
    /// Implementation of RFC 7009 (http://tools.ietf.org/html/rfc7009)
    /// </summary>
    [NoCache]
    internal class RevocationEndpointController : ApiController
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        
        private readonly IEventService _events;
        private readonly ClientSecretValidator _clientValidator;
        private readonly IdentityServerOptions _options;
        private readonly TokenRevocationRequestValidator _requestValidator;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly IRefreshTokenStore _refreshTokens;

        public RevocationEndpointController(IdentityServerOptions options, ClientSecretValidator clientValidator, TokenRevocationRequestValidator requestValidator, ITokenHandleStore tokenHandles, IRefreshTokenStore refreshTokens, IEventService events)
        {
            _options = options;
            _clientValidator = clientValidator;
            _requestValidator = requestValidator;
            _tokenHandles = tokenHandles;
            _refreshTokens = refreshTokens;
            _events = events;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start token revocation request");

            // validate client credentials and client
            var clientResult = await _clientValidator.ValidateAsync();
            if (clientResult.Client == null)
            {
                return new RevocationErrorResult(Constants.TokenErrors.InvalidClient);
            }

            var form = await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync();
            var response = await ProcessAsync(clientResult.Client, form);

            if (response is RevocationErrorResult)
            {
                var details = response as RevocationErrorResult;
                await RaiseFailureEventAsync(details.Error);
            }
            else
            {
                await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.Revocation);
            }

            Logger.Info("End token revocation request");
            return response;
        }

        public async Task<IHttpActionResult> ProcessAsync(Client client, NameValueCollection parameters)
        {
            // validate the token request
            var requestResult = await _requestValidator.ValidateRequestAsync(parameters, client);

            if (requestResult.IsError)
            {
                return new RevocationErrorResult(requestResult.Error);
            }

            // revoke tokens
            if (requestResult.TokenTypeHint == Constants.TokenTypeHints.AccessToken)
            {
                await RevokeAccessTokenAsync(requestResult.Token, client);
            }
            else if (requestResult.TokenTypeHint == Constants.TokenTypeHints.RefreshToken)
            {
                await RevokeRefreshTokenAsync(requestResult.Token, client);
            }
            else
            {
                var found = await RevokeAccessTokenAsync(requestResult.Token, client);

                if (!found)
                {
                    await RevokeRefreshTokenAsync(requestResult.Token, client);
                }
            }

            return Ok();
        }

        // revoke access token only if it belongs to client doing the request
        private async Task<bool> RevokeAccessTokenAsync(string handle, Client client)
        {
            var token = await _tokenHandles.GetAsync(handle);
            
            if (token != null)
            {
                if (token.ClientId == client.ClientId)
                {
                    await _tokenHandles.RemoveAsync(handle);
                    await _events.RaiseTokenRevokedEventAsync(token.SubjectId, handle, Constants.TokenTypeHints.AccessToken);
                }
                else
                {
                    var message = string.Format("Client {0} tried to revoke an access token belonging to a different client: {1}", client.ClientId, token.ClientId);

                    Logger.Warn(message);
                    await RaiseFailureEventAsync(message);
                }

                return true;
            }

            return false;
        }

        // revoke refresh token only if it belongs to client doing the request
        private async Task<bool> RevokeRefreshTokenAsync(string handle, Client client)
        {
            var token = await _refreshTokens.GetAsync(handle);

            if (token != null)
            {
                if (token.ClientId == client.ClientId)
                {
                    await _refreshTokens.RevokeAsync(token.SubjectId, token.ClientId);
                    await _tokenHandles.RevokeAsync(token.SubjectId, token.ClientId);
                    await _events.RaiseTokenRevokedEventAsync(token.SubjectId, handle, Constants.TokenTypeHints.RefreshToken);
                }
                else
                {
                    var message = string.Format("Client {0} tried to revoke a refresh token belonging to a different client: {1}", client.ClientId, token.ClientId);
                    
                    Logger.Warn(message);
                    await RaiseFailureEventAsync(message);
                }

                return true;
            }

            return false;
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.Revocation, error);
        }
    }
}