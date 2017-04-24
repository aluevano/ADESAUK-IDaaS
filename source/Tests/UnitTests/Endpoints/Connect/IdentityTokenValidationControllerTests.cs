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


using FluentAssertions;
using IdentityServer.Core;
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.Default;
using IdentityServer.Core.Validation;
using IdentityServer.Tests.Endpoints;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Connect.Endpoints
{
    public class IdentityTokenValidationControllerTests : IdSvrHostTestBase
    {
        const String Category = "Identity token validation endpoint";
        static readonly String TestUrl = Constants.RoutePaths.Oidc.IdentityTokenValidation;

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_IdTokenValidationEndpointDisabled_ReturnsNotFound()
        {
            ConfigureIdentityServerOptions = options =>
            {
                options.Endpoints.EnableIdentityTokenValidationEndpoint = false;
            };
            base.Init();
            var resp = Get();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_MissingTokenInQueryString_ReturnsBadRequest()
        {
            var resp = Get();
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostIdTokenValidation_MissingToken_ReturnsBadRequest()
        {
            var form = new { };

            var resp = PostForm(TestUrl, form);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_MissingClientIdInQueryString_ReturnsBadRequest()
        {
            var resp = Get(token: "token");
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostIdTokenValidation_MissingClientId_ReturnsBadRequest()
        {
            var form = new
            {
                token = "token"
            };

            var resp = PostForm(TestUrl, form);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_ValidIdToken_ReturnsClaims()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysValidIdentityTokenValidator>());
            };
            Init();

            var resp = Get("token", "client_id");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var claims = resp.GetJson<IDictionary<String, String>>();

            claims.Should().NotBeNull();
            claims.Count.Should().Be(2);

            Action<KeyValuePair<String, String>, String, String> assertClaim = (claim, claimType, claimValue) =>
            {
                claim.Should().NotBeNull();
                claim.Key.Should().Be(claimType);
                claim.Value.Should().Be(claimValue);
            };

            assertClaim(claims.ElementAt(0), Constants.ClaimTypes.Subject, "unique_subject");
            assertClaim(claims.ElementAt(1), Constants.ClaimTypes.Name, "subject name");
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostIdTokenValidation_ValidIdToken_ReturnsClaims()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysValidIdentityTokenValidator>());
            };
            Init();

            var form = new
            {
                token = "token",
                client_id = "client_id"
            };

            var resp = PostForm(TestUrl, form);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var claims = resp.GetJson<IDictionary<String, String>>();

            claims.Should().NotBeNull();
            claims.Count.Should().Be(2);

            Action<KeyValuePair<String, String>, String, String> assertClaim = (claim, claimType, claimValue) =>
            {
                claim.Should().NotBeNull();
                claim.Key.Should().Be(claimType);
                claim.Value.Should().Be(claimValue);
            };

            assertClaim(claims.ElementAt(0), Constants.ClaimTypes.Subject, "unique_subject");
            assertClaim(claims.ElementAt(1), Constants.ClaimTypes.Name, "subject name");
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_InvalidIdToken_ReturnsBadRequest()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysInvalidIdentityTokenValidator>());
            };
            Init();

            var resp = Get("token", "client_id");
            var error = resp.GetJson<IDictionary<String, String>>(successExpected: false);

            error.Should().NotBeNull();
            error.Count.Should().Be(1);
            error.First().Key.Should().Be("Message");
            error.First().Value.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostIdTokenValidation_InvalidIdToken_ReturnsBadRequest()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysInvalidIdentityTokenValidator>());
            };
            Init();

            var form = new
            {
                token = "token",
                client_id = "client_id"
            };

            var resp = PostForm(TestUrl, form);

            var error = resp.GetJson<IDictionary<String, String>>(successExpected: false);

            error.Should().NotBeNull();
            error.Count.Should().Be(1);
            error.First().Key.Should().Be("Message");
            error.First().Value.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        private HttpResponseMessage Get(String token = null, String clientId = null)
        {
            var parameters = new NameValueCollection();

            if (token.IsPresent())
            {
                parameters.Add("token", token);
            }

            if (clientId.IsPresent())
            {
                parameters.Add("client_id", clientId);
            }

            var url = TestUrl.AddQueryString(parameters.ToQueryString());
            return base.Get(url);
        }

        private class AlwaysValidIdentityTokenValidator : TokenValidator
        {
            public AlwaysValidIdentityTokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator, OwinEnvironmentService context)
                : base(options, clients, tokenHandles, customValidator, context, new DefaultSigningKeyService(options))
            {
            }

            public override Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
            {
                var result = new TokenValidationResult
                {
                    IsError = false,
                    Claims = new[]
                    {
                        new Claim(Constants.ClaimTypes.Subject, "unique_subject"),
                        new Claim(Constants.ClaimTypes.Name, "subject name")
                    }
                };

                return Task.FromResult(result);
            }
        }

        private class AlwaysInvalidIdentityTokenValidator : TokenValidator
        {
            public AlwaysInvalidIdentityTokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator, OwinEnvironmentService context)
                : base(options, clients, tokenHandles, customValidator, context, new DefaultSigningKeyService(options))
            {
            }

            public override Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
            {
                var result = new TokenValidationResult
                {
                    Error = Constants.ProtectedResourceErrors.InvalidToken
                };

                return Task.FromResult(result);
            }
        }
    }
}