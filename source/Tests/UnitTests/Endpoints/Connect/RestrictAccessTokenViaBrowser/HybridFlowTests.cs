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
using IdentityServer.Core.Models;
using IdentityServer.Tests.Conformance;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace IdentityServer.Tests.Endpoints.Connect.RestrictAccessTokenViaBrowser
{
    public class HybridFlowTests : IdentityServerHostTest
    {
        const string Category = "Endpoint.Authorize.HybridFlow";

        string client_id = "hybrid";
        string client_id_noBrowser = "hybrid.nobrowser";

        string redirect_uri = "https://hybrid_client/callback";
        string client_secret = "secret";
        string scope = "openid";


        protected override void PreInit()
        {
            host.Scopes.Add(StandardScopes.OpenId);

            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                Flow = Flows.Hybrid,
                AllowAccessToAllScopes = true,
                AllowAccessTokensViaBrowser = true,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });

            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id_noBrowser,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                Flow = Flows.Hybrid,
                AllowAccessToAllScopes = true,
                AllowAccessTokensViaBrowser = false,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public void Unrestricted_client_can_request_CodeIdToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "code id_token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#code=");
            result.Headers.Location.AbsoluteUri.Should().Contain("id_token=");
            result.Headers.Location.AbsoluteUri.Should().NotContain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Unrestricted_client_can_request_CodeIdTokenToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "code id_token token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#code=");
            result.Headers.Location.AbsoluteUri.Should().Contain("id_token=");
            result.Headers.Location.AbsoluteUri.Should().Contain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_client_can_request_CodeIdToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id_noBrowser,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "code id_token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#code=");
            result.Headers.Location.AbsoluteUri.Should().Contain("id_token=");
            result.Headers.Location.AbsoluteUri.Should().NotContain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_client_cannot_request_CodeIdTokenToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id_noBrowser,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "code id_token token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;

            // user error page - no protocol response
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}