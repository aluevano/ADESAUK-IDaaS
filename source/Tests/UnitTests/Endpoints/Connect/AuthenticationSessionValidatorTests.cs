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
using IdentityServer.Core.Services;
using IdentityServer.Tests.Endpoints;
using System.Net;
using System.Net.Http;
using Xunit;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Models;
using IdentityServer.Core.ViewModels;

namespace IdentityServer.Tests.Connect.Endpoints
{
    public class AuthenticationSessionValidatorTests : IdSvrHostTestBase
    {
        public class StubValidator : IAuthenticationSessionValidator
        {
            public bool Response { get; set; }
            public Task<bool> IsAuthenticationSessionValidAsync(ClaimsPrincipal subject)
            {
                return Task.FromResult(Response);
            }
        }

        HttpResponseMessage GetAuthorizePage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            ProcessXsrf(resp);
            return resp;
        }

        public AuthenticationSessionValidatorTests()
        {
            ConfigureIdentityServerOptions = opts =>
            {
                opts.Factory.AuthenticationSessionValidator = new Registration<IAuthenticationSessionValidator>(_stubValidator);
            };
            base.Init();
        }

        StubValidator _stubValidator = new StubValidator();

        void Login(bool setCookie = true)
        {
            var msg = new SignInMessage() { ReturnUrl = Url("authorize") };
            var signInId = WriteMessageToCookie(msg);
            var url = Constants.RoutePaths.Login + "?signin=" + signInId;
            var resp = Get(url);
            ProcessXsrf(resp);

            if (setCookie)
            {
                resp = PostForm(url, new LoginCredentials { Username = "alice", Password = "alice" });
                client.SetCookies(resp.GetCookies());
            }
        }

        [Fact]
        public void GetAuthorize_UserLoggedIn_ValidatorReturnsTrue_ReturnsConsentPage()
        {
            _stubValidator.Response = true;
            Login();

            var resp = Get(Constants.RoutePaths.Oidc.Authorize + "?client_id=implicitclient&redirect_uri=http://localhost:21575/index.html&response_type=id_token&scope=openid&nonce=123");

            resp.AssertPage("consent");
        }

        [Fact]
        public void GetAuthorize_UserLoggedIn_ValidatorReturnsFalse_ReturnsLogin()
        {
            _stubValidator.Response = false;
            Login();

            var resp = Get(Constants.RoutePaths.Oidc.Authorize + "?client_id=implicitclient&redirect_uri=http://localhost:21575/index.html&response_type=id_token&scope=openid&nonce=123");

            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsolutePath.Should().Be("/" + Constants.RoutePaths.Login);
        }
    }
}
