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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Endpoints.Connect
{
    public class AccessTokenValidationControllerTests : IdSvrHostTestBase
    {
        const String Category = "Access token validation endpoint";
        static readonly String TestUrl = Constants.RoutePaths.Oidc.AccessTokenValidation;

        [Fact]
        [Trait("Category", Category)]
        public void GetAccessTokenValidation_AccessTokenValidationEndpointDisabled_ReturnsNotFound()
        {
            ConfigureIdentityServerOptions = options =>
            {
                options.Endpoints.EnableAccessTokenValidationEndpoint = false;
            };
            base.Init();
            var resp = Get();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetAccessTokenValidation_MissingToken_ReturnsBadRequest()
        {
            var resp = Get();
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostAccessTokenValidation_MissingToken_ReturnsBadRequest()
        {
            var col = new NameValueCollection();
            var resp = PostForm(TestUrl, col);

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetAccessTokenValidation_InvalidToken_ReturnsBadRequest()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysInvalidAccessTokenValidator>());
            };
            Init();

            var resp = Get("Dummy Token");
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = resp.GetJson<IDictionary<String, String>>(successExpected: false);
            
            error.Should().NotBeNull();
            error.Count.Should().Be(1);
            error.First().Key.Should().Be("Message");
            error.First().Value.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostAccessTokenValidation_InvalidToken_ReturnsBadRequest()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysInvalidAccessTokenValidator>());
            };
            Init();

            var form = new
            {
                token = "Dummy Token"
            };

            var resp = PostForm(TestUrl, form);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = resp.GetJson<IDictionary<String, String>>(successExpected: false);

            error.Should().NotBeNull();
            error.Count.Should().Be(1);
            error.First().Key.Should().Be("Message");
            error.First().Value.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetAccessTokenValidation_ValidToken_ReturnsClaims()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysValidAccessTokenValidator>());
            };
            Init();

            var resp = Get("Dummy Token");
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
        public void PostAccessTokenValidation_ValidToken_ReturnsClaims()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysValidAccessTokenValidator>());
            };
            Init();

            var form = new
            {
                token = "Dummy Token"
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

        private new HttpResponseMessage Get(String token = null)
        {
            var parameters = new NameValueCollection();

            if (token.IsPresent())
            {
                parameters.Add("token", token);
            }

            var url = TestUrl.AddQueryString(parameters.ToQueryString());
            return base.Get(url);
        }

        private class AlwaysValidAccessTokenValidator : TokenValidator
        {
            public AlwaysValidAccessTokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator, OwinEnvironmentService context)
                : base(options, clients, tokenHandles, customValidator, context, new DefaultSigningKeyService(options))
            {
            }

            public override Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
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

        private class AlwaysInvalidAccessTokenValidator : TokenValidator
        {
            public AlwaysInvalidAccessTokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator, OwinEnvironmentService context)
                : base(options, clients, tokenHandles, customValidator, context, new DefaultSigningKeyService(options))
            {
            }

            public override Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
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
