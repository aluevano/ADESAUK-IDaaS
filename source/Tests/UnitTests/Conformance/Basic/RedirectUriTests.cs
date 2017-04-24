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
using IdentityServer.Core.Resources;
using IdentityServer.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Xunit;


namespace IdentityServer.Tests.Conformance.Basic
{
    public class RedirectUriTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.RedirectUriTests";

        Client client;
        string client_id = "code_client";
        string redirect_uri = "https://code_client/callback";
        string client_secret = "secret";

        protected override void PreInit()
        {
            host.Scopes.Add(StandardScopes.OpenId);
            host.Clients.Add(client = new Client
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
        public void Reject_redirect_uri_not_matching_registered_redirect_uri()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = host.GetAuthorizeUrl(client_id, "http://bad", "openid", "code", state, nonce);

            var result = host.Client.GetAsync(url).Result;

            result.AssertPage("error");
            var model = result.GetPageModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.unauthorized_client);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Reject_request_without_redirect_uri_when_multiple_registered()
        {
			host.Login();

			var nonce = Guid.NewGuid().ToString();
			var state = Guid.NewGuid().ToString();
			var url = host.GetAuthorizeUrl(client_id, /*redirect_uri*/ null, "openid", "code", state, nonce);

			var result = host.Client.GetAsync(url).Result;

			result.AssertPage("error");
			var model = result.GetPageModel<ErrorViewModel>();
			model.ErrorMessage.Should().Be(Messages.invalid_request);
		}
        
        [Fact]
        [Trait("Category", Category)]
        public void Preserves_query_parameters_in_redirect_uri()
        {
            var query_redirect_uri = redirect_uri + "?foo=bar&baz=quux";
            client.RedirectUris.Add(query_redirect_uri);

            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = host.GetAuthorizeUrl(client_id, query_redirect_uri, "openid", "code", state, nonce);

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Redirect);
            result.Headers.Location.AbsoluteUri.Should().StartWith("https://code_client/callback");
            result.Headers.Location.AbsolutePath.Should().Be("/callback");
            var query = result.Headers.Location.ParseQueryString();
            query.AllKeys.Should().Contain("foo");
            query["foo"].ToString().Should().Be("bar");
            query.AllKeys.Should().Contain("baz");
            query["baz"].ToString().Should().Be("quux");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Rejects_redirect_uri_when_query_parameter_does_not_match()
        {
            var query_redirect_uri = redirect_uri + "?foo=bar&baz=quux";
            client.RedirectUris.Add(query_redirect_uri);
            query_redirect_uri = redirect_uri + "?baz=quux&foo=bar";

            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = host.GetAuthorizeUrl(client_id, query_redirect_uri, "openid", "code", state, nonce);

            var result = host.Client.GetAsync(url).Result;
            
            result.AssertPage("error");
            var model = result.GetPageModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.unauthorized_client);
        }
    }
}
