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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Xunit;

namespace IdentityServer.Tests.Conformance.Basic
{
    public class ResponseTypeResponseModeTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.ResponseTypeResponseModeTests";

        string client_id = "code_client";
        string redirect_uri = "https://code_client/callback";
        string client_secret = "secret";

        protected override void PreInit()
        {
            host.Scopes.Add(StandardScopes.OpenId);
            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret)
                },

                Flow = Flows.AuthorizationCode,
                AllowAccessToAllScopes = true,
                
                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public void Request_with_response_type_code_supported()
        {
            host.Login();
            var cert = host.GetSigningCertificate();

            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();

            var url = host.GetAuthorizeUrl(client_id, redirect_uri, "openid", "code", state, nonce);
            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseQueryString();
            query.AllKeys.Should().Contain("code");
            query.AllKeys.Should().Contain("state");
            query["state"].Should().Be(state);
        }

		// todo : update to new behavior
		//[Fact]
		//[Trait("Category", Category)]
		//public void Request_missing_response_type_rejected()
		//{
		//    host.Login();

		//    var state = Guid.NewGuid().ToString();
		//    var nonce = Guid.NewGuid().ToString();
		//    var url = host.GetAuthorizeUrl(client_id, redirect_uri, "openid", /*response_type*/ null, state, nonce);

		//    var result = host.Client.GetAsync(url).Result;
		//    result.StatusCode.Should().Be(HttpStatusCode.Found);
		//    result.Headers.Location.AbsoluteUri.Should().Contain("#");

		//    var query = result.Headers.Location.ParseHashFragment();
		//    query.AllKeys.Should().Contain("state");
		//    query["state"].Should().Be(state);
		//    query.AllKeys.Should().Contain("error");
		//    query["error"].Should().Be("unsupported_response_type");
		//}
	}
}
