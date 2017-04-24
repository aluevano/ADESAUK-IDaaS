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
using IdentityServer.Tests.Endpoints;
using System.Net;
using System.Net.Http;
using Xunit;

namespace IdentityServer.Tests.Connect.Endpoints
{
    public class AuthorizeEndpointControllerTests : IdSvrHostTestBase
    {
        HttpResponseMessage GetAuthorizePage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            ProcessXsrf(resp);
            return resp;
        }

        [Fact]
        public void GetAuthorize_AuthorizeEndpointDisabled_ReturnsNotFound()
        {
            ConfigureIdentityServerOptions = opts =>
            {
                opts.Endpoints.EnableAuthorizeEndpoint = false;
            };
            base.Init();
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetAuthorize_NoQueryStringParams_ReturnsErrorPage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            resp.AssertPage("error");
        }

        [Fact]
        public void PostConsent_JsonMediaType_ReturnsUnsupportedMediaType()
        {
            var resp = Post(Constants.RoutePaths.Oidc.Consent, (object)null);
            resp.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public void PostConsent_NoAntiCsrf_ReturnsErrorPage()
        {
            var resp = PostForm(Constants.RoutePaths.Oidc.Consent, (object)null);
            resp.AssertPage("error");
        }
        
        [Fact]
        public void PostConsent_NoBody_ReturnsErrorPage()
        {
            GetAuthorizePage();
            var resp = PostForm(Constants.RoutePaths.Oidc.Consent, (object)null);
            resp.AssertPage("error");
        }
    }
}
