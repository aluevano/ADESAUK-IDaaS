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
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using IdentityServer.Core.Validation;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer.Core.ResponseHandling
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TokenResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IScopeStore _scopes;
        private readonly ICustomTokenResponseGenerator _customResponseGenerator;

        public TokenResponseGenerator(ITokenService tokenService, IRefreshTokenService refreshTokenService, IScopeStore scopes, ICustomTokenResponseGenerator customResponseGenerator)
        {
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _scopes = scopes;
            _customResponseGenerator = customResponseGenerator;
        }

        public async Task<TokenResponse> ProcessAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Creating token response");

            TokenResponse response;

            if (request.GrantType == Constants.GrantTypes.AuthorizationCode)
            {
                response = await ProcessAuthorizationCodeRequestAsync(request);
            }
            else if (request.GrantType == Constants.GrantTypes.RefreshToken)
            {
                response = await ProcessRefreshTokenRequestAsync(request);
            }
            else
            {
                response = await ProcessTokenRequestAsync(request);
            }

            return await _customResponseGenerator.GenerateAsync(request, response);
        }

        private async Task<TokenResponse> ProcessAuthorizationCodeRequestAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Processing authorization code request");

            //////////////////////////
            // access token
            /////////////////////////
            var accessToken = await CreateAccessTokenAsync(request);
            var response = new TokenResponse
            {
                AccessToken = accessToken.Item1,
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            if (request.RequestedTokenType == RequestedTokenTypes.PoP)
            {
                response.TokenType = Constants.ResponseTokenTypes.PoP;
                response.Algorithm = request.ProofKeyAlgorithm;
            }

            //////////////////////////
            // refresh token
            /////////////////////////
            if (accessToken.Item2.IsPresent())
            {
                response.RefreshToken = accessToken.Item2;
            }

            //////////////////////////
            // id token
            /////////////////////////
            if (request.AuthorizationCode.IsOpenId)
            {
                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Client = request.AuthorizationCode.Client,
                    Scopes = request.AuthorizationCode.RequestedScopes,
                    Nonce = request.AuthorizationCode.Nonce,

                    ValidatedRequest = request
                };

                var idToken = await _tokenService.CreateIdentityTokenAsync(tokenRequest);
                var jwt = await _tokenService.CreateSecurityTokenAsync(idToken);
                response.IdentityToken = jwt;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessTokenRequestAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Processing token request");

            var accessToken = await CreateAccessTokenAsync(request);
            var response = new TokenResponse
            {
                AccessToken = accessToken.Item1,
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            if (accessToken.Item2.IsPresent())
            {
                response.RefreshToken = accessToken.Item2;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessRefreshTokenRequestAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Processing refresh token request");

            var oldAccessToken = request.RefreshToken.AccessToken;
            string accessTokenString;

            // if pop request, claims must be updated because we need a fresh proof token
            if (request.Client.UpdateAccessTokenClaimsOnRefresh || request.RequestedTokenType == RequestedTokenTypes.PoP)
            {
                var subject = request.RefreshToken.GetOriginalSubject();

                var creationRequest = new TokenCreationRequest
                {
                    Client = request.Client,
                    Subject = subject,
                    ValidatedRequest = request,
                    Scopes = await _scopes.FindScopesAsync(oldAccessToken.Scopes),
                };

                // if pop request, embed proof token
                if (request.RequestedTokenType == RequestedTokenTypes.PoP)
                {
                    creationRequest.ProofKey = GetProofKey(request);
                }

                var newAccessToken = await _tokenService.CreateAccessTokenAsync(creationRequest);
                accessTokenString = await _tokenService.CreateSecurityTokenAsync(newAccessToken);
            }
            else
            {
                var copy = new Token(oldAccessToken);
                copy.CreationTime = DateTimeOffsetHelper.UtcNow;
                copy.Lifetime = request.Client.AccessTokenLifetime;

                accessTokenString = await _tokenService.CreateSecurityTokenAsync(copy);
            }

            var handle = await _refreshTokenService.UpdateRefreshTokenAsync(request.RefreshTokenHandle, request.RefreshToken, request.Client);

            var response = new TokenResponse
            {
                AccessToken = accessTokenString,
                AccessTokenLifetime = request.Client.AccessTokenLifetime,
                RefreshToken = handle
            };

            if (request.RequestedTokenType == RequestedTokenTypes.PoP)
            {
                response.TokenType = Constants.ResponseTokenTypes.PoP;
                response.Algorithm = request.ProofKeyAlgorithm;
            }

            response.IdentityToken = await CreateIdTokenFromRefreshTokenRequestAsync(request, accessTokenString);

            return response;
        }

        private async Task<Tuple<string, string>> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            TokenCreationRequest tokenRequest;
            bool createRefreshToken;

            if (request.AuthorizationCode != null)
            {
                createRefreshToken = request.AuthorizationCode.RequestedScopes.Select(s => s.Name).Contains(Constants.StandardScopes.OfflineAccess);

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Client = request.AuthorizationCode.Client,
                    Scopes = request.AuthorizationCode.RequestedScopes,
                    ValidatedRequest = request
                };
            }
            else
            {
                createRefreshToken = request.ValidatedScopes.ContainsOfflineAccessScope;

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.Subject,
                    Client = request.Client,
                    Scopes = request.ValidatedScopes.GrantedScopes,
                    ValidatedRequest = request
                };
            }

            // bind proof key to token if present
            if (request.RequestedTokenType == RequestedTokenTypes.PoP)
            {
                tokenRequest.ProofKey = GetProofKey(request);
            }

            Token accessToken = await _tokenService.CreateAccessTokenAsync(tokenRequest);

            string refreshToken = "";
            if (createRefreshToken)
            {
                refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(tokenRequest.Subject, accessToken, request.Client);
            }

            var securityToken = await _tokenService.CreateSecurityTokenAsync(accessToken);
            return Tuple.Create(securityToken, refreshToken);
        }

        private string GetProofKey(ValidatedTokenRequest request)
        {
            // for now we only support client generated proof keys
            return request.ProofKey;
        }

        private async Task<string> CreateIdTokenFromRefreshTokenRequestAsync(ValidatedTokenRequest request, string newAccessToken)
        {
            var oldAccessToken = request.RefreshToken.AccessToken;
            var tokenRequest = new TokenCreationRequest
            {
                Subject = request.RefreshToken.GetOriginalSubject(),
                Client = request.Client,
                Scopes = await _scopes.FindScopesAsync(oldAccessToken.Scopes),
                ValidatedRequest = request,
                AccessTokenToHash = newAccessToken
            };
            var idToken = await _tokenService.CreateIdentityTokenAsync(tokenRequest);
            return await _tokenService.CreateSecurityTokenAsync(idToken);
        }
    }
}