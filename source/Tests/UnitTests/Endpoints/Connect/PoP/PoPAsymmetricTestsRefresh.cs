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
using IdentityModel;
using IdentityServer.Core;
using IdentityServer.Core.Models;
using IdentityServer.Tests.Conformance;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace IdentityServer.Tests.Endpoints.Connect.PoP
{
    public class PoP_Asymmetrc_Tests_Refresh : IdentityServerHostTest
    {
        const string Category = "Endpoints.PoP.Asymmetric.RefreshToken";

        string client_id = "code_client";
        string client_id_reference = "code_client_reference";
        string redirect_uri = "https://code_client/callback";
        string client_secret = "secret";

        protected override void PreInit()
        {
            var api = new Scope
            {
                Name = "api",
                Type = ScopeType.Resource,

                ScopeSecrets = new List<Secret>
                {
                    new Secret("secret".Sha256())
                }
            };

            host.Scopes.Add(StandardScopes.OpenId);
            host.Scopes.Add(StandardScopes.OfflineAccess);
            host.Scopes.Add(api);

            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                Flow = Flows.AuthorizationCode,
                AllowAccessToAllScopes = true,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });

            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id_reference,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                Flow = Flows.AuthorizationCode,
                AllowAccessToAllScopes = true,
                AccessTokenType = AccessTokenType.Reference,
                UpdateAccessTokenClaimsOnRefresh = false,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public void Valid_Asymmetric_Key()
        {
            host.Login();

            // request code
            var nonce = Guid.NewGuid().ToString();
            var query = host.RequestAuthorizationCode(client_id, redirect_uri, "openid api offline_access", nonce);
            var code = query["code"];

            host.NewRequest();
            host.Client.SetBasicAuthentication(client_id, client_secret);

            // request tokens using code
            var jwk = Helper.CreateJwk();
            var key = Helper.CreateJwkString(jwk);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri,
                    token_type = "pop",
                    alg = "RS256",
                    key
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.CacheControl.NoCache.Should().BeTrue();
            result.Headers.CacheControl.NoStore.Should().BeTrue();

            var data = result.ReadJsonObject();

            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("pop");

            data["alg"].ToString().Should().NotBeNull();
            data["alg"].ToString().Should().Be("RS256");

            data["access_token"].Should().NotBeNull();
            data["refresh_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();
            data["id_token"].Should().NotBeNull();

            var payload = data["access_token"].ToString().Split('.')[1];
            var json = Encoding.UTF8.GetString(Base64Url.Decode(payload));
            var claims = JObject.Parse(json);

            claims["cnf"].Should().NotBeNull();
            var jjwk = claims["cnf"]["jwk"];

            jjwk["kty"].ToString().Should().Be("RSA");
            jjwk["e"].ToString().Should().Be(jwk.e);
            jjwk["n"].ToString().Should().Be(jwk.n);
            jjwk["alg"].ToString().Should().Be("RS256");


            // request new token using refresh token
            var refresh_token = data["refresh_token"].ToString();

            jwk = Helper.CreateJwk();
            key = Helper.CreateJwkString(jwk);

            host.NewRequest();
            host.Client.SetBasicAuthentication(client_id, client_secret);

            result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "refresh_token",
                    refresh_token,
                    token_type = "pop",
                    alg = "RS256",
                    key
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.CacheControl.NoCache.Should().BeTrue();
            result.Headers.CacheControl.NoStore.Should().BeTrue();

            data = result.ReadJsonObject();

            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("pop");

            data["alg"].ToString().Should().NotBeNull();
            data["alg"].ToString().Should().Be("RS256");

            data["access_token"].Should().NotBeNull();
            data["refresh_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();
            
            payload = data["access_token"].ToString().Split('.')[1];
            json = Encoding.UTF8.GetString(Base64Url.Decode(payload));
            claims = JObject.Parse(json);

            claims["cnf"].Should().NotBeNull();
            jjwk = claims["cnf"]["jwk"];

            jjwk["kty"].ToString().Should().Be("RSA");
            jjwk["e"].ToString().Should().Be(jwk.e);
            jjwk["n"].ToString().Should().Be(jwk.n);
            jjwk["alg"].ToString().Should().Be("RS256");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Valid_Asymmetric_Key_Reference()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var query = host.RequestAuthorizationCode(client_id_reference, redirect_uri, "openid api offline_access", nonce);
            var code = query["code"];

            host.NewRequest();
            host.Client.SetBasicAuthentication(client_id_reference, client_secret);

            var jwk = Helper.CreateJwk();
            var key = Helper.CreateJwkString(jwk);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri,
                    token_type = "pop",
                    alg = "RS256",
                    key
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.CacheControl.NoCache.Should().BeTrue();
            result.Headers.CacheControl.NoStore.Should().BeTrue();

            var data = result.ReadJsonObject();
            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("pop");

            data["alg"].ToString().Should().NotBeNull();
            data["alg"].ToString().Should().Be("RS256");

            data["access_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();
            data["id_token"].Should().NotBeNull();
            data["refresh_token"].ToString().Should().NotBeNull();

            var refresh_token = data["refresh_token"].ToString();
            var referenceToken = data["access_token"].ToString();

            host.NewRequest();
            var introspectionResponse = host.Introspect("api", "secret", referenceToken);

            introspectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            data = introspectionResponse.ReadJsonObject();

            data["cnf"].Should().NotBeNull();
            var jjwk = data["cnf"]["jwk"];

            jjwk["kty"].ToString().Should().Be("RSA");
            jjwk["e"].ToString().Should().Be(jwk.e);
            jjwk["n"].ToString().Should().Be(jwk.n);
            jjwk["alg"].ToString().Should().Be("RS256");

            // request new token using refresh token

            jwk = Helper.CreateJwk();
            key = Helper.CreateJwkString(jwk);

            host.NewRequest();
            host.Client.SetBasicAuthentication(client_id_reference, client_secret);

            result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "refresh_token",
                    refresh_token,
                    token_type = "pop",
                    alg = "RS256",
                    key
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.CacheControl.NoCache.Should().BeTrue();
            result.Headers.CacheControl.NoStore.Should().BeTrue();

            data = result.ReadJsonObject();

            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("pop");

            data["alg"].ToString().Should().NotBeNull();
            data["alg"].ToString().Should().Be("RS256");

            data["access_token"].Should().NotBeNull();
            data["refresh_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();

            referenceToken = data["access_token"].ToString();

            host.NewRequest();
            introspectionResponse = host.Introspect("api", "secret", referenceToken);

            introspectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            data = introspectionResponse.ReadJsonObject();

            data["cnf"].Should().NotBeNull();
            jjwk = data["cnf"]["jwk"];

            jjwk["kty"].ToString().Should().Be("RSA");
            jjwk["e"].ToString().Should().Be(jwk.e);
            jjwk["n"].ToString().Should().Be(jwk.n);
            jjwk["alg"].ToString().Should().Be("RS256");
        }
    }
}