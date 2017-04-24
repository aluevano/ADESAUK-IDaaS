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
    public class ImplicitFlowTests : IdentityServerHostTest
    {
        const string Category = "Endpoint.Authorize.ImplicitFlow";

        string client_id = "implicit";
        string client_id_noBrowser = "implicit.nobrowser";

        string redirect_uri = "https://implicit_client/callback";
        string scope = "openid";

        protected override void PreInit()
        {
            host.Scopes.Add(StandardScopes.OpenId);

            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id,
                
                Flow = Flows.Implicit,
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
                
                Flow = Flows.Implicit,
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
        public void Unrestricted_client_can_request_IdToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "id_token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#id_token=");
            result.Headers.Location.AbsoluteUri.Should().NotContain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Unrestricted_client_can_request_IdTokenToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "id_token token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#id_token=");
            result.Headers.Location.AbsoluteUri.Should().Contain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_client_can_request_IdToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id_noBrowser,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "id_token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#id_token=");
            result.Headers.Location.AbsoluteUri.Should().NotContain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_client_cannot_request_IdTokenToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id_noBrowser,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "id_token token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;

            // user error page - no protocol response
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}