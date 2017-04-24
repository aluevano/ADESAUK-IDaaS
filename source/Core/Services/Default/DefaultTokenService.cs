﻿/*
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

using IdentityModel;
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default token service
    /// </summary>
    public class DefaultTokenService : ITokenService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// The identity server options
        /// </summary>
        protected readonly IdentityServerOptions _options;

        /// <summary>
        /// The claims provider
        /// </summary>
        protected readonly IClaimsProvider _claimsProvider;

        /// <summary>
        /// The token handles
        /// </summary>
        protected readonly ITokenHandleStore _tokenHandles;

        /// <summary>
        /// The signing service
        /// </summary>
        protected readonly ITokenSigningService _signingService;

        /// <summary>
        /// The events service
        /// </summary>
        protected readonly IEventService _events;
        
        /// <summary>
        /// The OWIN environment service
        /// </summary>
        protected readonly OwinEnvironmentService _owinEnvironmentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenService" /> class. This overloaded constructor is deprecated and will be removed in 3.0.0.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="claimsProvider">The claims provider.</param>
        /// <param name="tokenHandles">The token handles.</param>
        /// <param name="signingService">The signing service.</param>
        /// <param name="events">The events service.</param>
        public DefaultTokenService(IdentityServerOptions options, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles, ITokenSigningService signingService, IEventService events)
        {
            _options = options;
            _claimsProvider = claimsProvider;
            _tokenHandles = tokenHandles;
            _signingService = signingService;
            _events = events;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenService" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="claimsProvider">The claims provider.</param>
        /// <param name="tokenHandles">The token handles.</param>
        /// <param name="signingService">The signing service.</param>
        /// <param name="events">The OWIN environment service.</param>
        /// <param name="owinEnvironmentService">The events service.</param>
        public DefaultTokenService(IdentityServerOptions options, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles, ITokenSigningService signingService, IEventService events, OwinEnvironmentService owinEnvironmentService)
        {
            _options = options;
            _claimsProvider = claimsProvider;
            _tokenHandles = tokenHandles;
            _signingService = signingService;
            _events = events;
            _owinEnvironmentService = owinEnvironmentService;
        }

        // todo: remove in 3.0.0
        private string IssuerUri
        {
            get
            {
                if (_owinEnvironmentService != null)
                {
                    return new OwinContext(_owinEnvironmentService.Environment).GetIdentityServerIssuerUri();
                }

                return _options.DynamicallyCalculatedIssuerUri;
            }
        }

        /// <summary>
        /// Creates an identity token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An identity token
        /// </returns>
        public virtual async Task<Token> CreateIdentityTokenAsync(TokenCreationRequest request)
        {
            Logger.Debug("Creating identity token");
            request.Validate();

            // host provided claims
            var claims = new List<Claim>();

            // if nonce was sent, must be mirrored in id token
            if (request.Nonce.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.Nonce, request.Nonce));
            }

            // add iat claim
            claims.Add(new Claim(Constants.ClaimTypes.IssuedAt, DateTimeOffsetHelper.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer));

            // add at_hash claim
            if (request.AccessTokenToHash.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.AccessTokenHash, HashAdditionalData(request.AccessTokenToHash)));
            }

            // add c_hash claim
            if (request.AuthorizationCodeToHash.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.AuthorizationCodeHash, HashAdditionalData(request.AuthorizationCodeToHash)));
            }

            // add sid if present
            if (request.ValidatedRequest.SessionId.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.SessionId, request.ValidatedRequest.SessionId));
            }

            claims.AddRange(await _claimsProvider.GetIdentityTokenClaimsAsync(
                request.Subject,
                request.Client,
                request.Scopes,
                request.IncludeAllIdentityClaims,
                request.ValidatedRequest));

            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = request.Client.ClientId,
                Issuer = IssuerUri,
                Lifetime = request.Client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                Client = request.Client
            };

            return token;
        }

        /// <summary>
        /// Creates an access token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An access token
        /// </returns>
        public virtual async Task<Token> CreateAccessTokenAsync(TokenCreationRequest request)
        {
            Logger.Debug("Creating access token");
            request.Validate();

            var claims = new List<Claim>();
            claims.AddRange(await _claimsProvider.GetAccessTokenClaimsAsync(
                request.Subject,
                request.Client,
                request.Scopes,
                request.ValidatedRequest));

            if (request.Client.IncludeJwtId)
            {
                claims.Add(new Claim(Constants.ClaimTypes.JwtId, CryptoRandom.CreateUniqueId()));
            }
            
            if (request.ProofKey.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.Confirmation, request.ProofKey, Constants.ClaimValueTypes.Json));
            }

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = string.Format(Constants.AccessTokenAudience, IssuerUri.EnsureTrailingSlash()),
                Issuer = IssuerUri,
                Lifetime = request.Client.AccessTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                Client = request.Client
            };

            return token;
        }

        /// <summary>
        /// Creates a serialized and protected security token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// A security token in serialized form
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Invalid token type.</exception>
        public virtual async Task<string> CreateSecurityTokenAsync(Token token)
        {
            string tokenResult;

            if (token.Type == Constants.TokenTypes.AccessToken)
            {
                if (token.Client.AccessTokenType == AccessTokenType.Jwt)
                {
                    Logger.Debug("Creating JWT access token");

                    tokenResult = await _signingService.SignTokenAsync(token);
                }
                else
                {
                    Logger.Debug("Creating reference access token");

                    var handle = CryptoRandom.CreateUniqueId();
                    await _tokenHandles.StoreAsync(handle, token);

                    tokenResult = handle;
                }
            }
            else if (token.Type == Constants.TokenTypes.IdentityToken)
            {
                Logger.Debug("Creating JWT identity token");

                tokenResult = await _signingService.SignTokenAsync(token);
            }
            else
            {
                throw new InvalidOperationException("Invalid token type.");
            }

            await _events.RaiseTokenIssuedEventAsync(token, tokenResult);
            return tokenResult;
        }

        /// <summary>
        /// Hashes an additional data (e.g. for c_hash or at_hash).
        /// </summary>
        /// <param name="tokenToHash">The token to hash.</param>
        /// <returns></returns>
        protected virtual string HashAdditionalData(string tokenToHash)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(tokenToHash));

                var leftPart = new byte[16];
                Array.Copy(hash, leftPart, 16);

                return Base64Url.Encode(leftPart);
            }
        }
    }
}