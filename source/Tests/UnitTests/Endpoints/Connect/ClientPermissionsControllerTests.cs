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
using IdentityServer.Core.Models;
using IdentityServer.Core.Resources;
using IdentityServer.Core.ViewModels;
using IdentityServer.Tests.Endpoints;
using System.Linq;
using System.Net;
using Xunit;

namespace IdentityServer.Tests.Connect.Endpoints
{

    public class ClientPermissionsControllerTests : IdSvrHostTestBase
    {
        string clientId;

        
        public ClientPermissionsControllerTests()
        {
            clientId = TestClients.Get().First().ClientId;
        }

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
        public void ShowPermissions_RendersPermissionPage()
        {
            Login();
            var resp = Get(Constants.RoutePaths.ClientPermissions);
            resp.AssertPage("permissions");
        }

        [Fact]
        public void ShowPermissions_EndpointDisabled_ReturnsNotFound()
        {
            ConfigureIdentityServerOptions = options =>
            {
                options.Endpoints.EnableClientPermissionsEndpoint = false;
            };
            base.Init();
            Login();
            var resp = Get(Constants.RoutePaths.ClientPermissions);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void RevokePermission_EndpointDisabled_ReturnsNotFound()
        {
            ConfigureIdentityServerOptions = options =>
            {
                options.Endpoints.EnableClientPermissionsEndpoint = false;
            };
            base.Init();
            Login();
            var resp = PostForm(Constants.RoutePaths.ClientPermissions, new { ClientId = clientId });
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void RevokePermission_JsonMediaType_ReturnsUnsupportedMediaType()
        {
            Login();
            var resp = Post(Constants.RoutePaths.Oidc.Consent, new { ClientId = clientId });
            resp.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public void RevokePermission_NoAntiCsrf_ReturnsErrorPage()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.Oidc.Consent, new { ClientId = clientId }, includeCsrf: false);
            resp.AssertPage("error");
        }
        
        [Fact]
        public void RevokePermission_NoBody_ShowsError()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.ClientPermissions, (object)null);
            var model = resp.GetModel<ClientPermissionsViewModel>();
            model.ErrorMessage.Should().Be(Messages.ClientIdRequired);
        }

        [Fact]
        public void RevokePermission_NoClient_ShowsError()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.ClientPermissions, new { ClientId = "" });
            var model = resp.GetModel<ClientPermissionsViewModel>();
            model.ErrorMessage.Should().Be(Messages.ClientIdRequired);
        }

        [Fact]
        public void ShowPermissions_Unauthenticated_ShowsLoginPage()
        {
            Login(false);
            var resp = Get(Constants.RoutePaths.ClientPermissions);
            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsoluteUri.Should().Contain(Constants.RoutePaths.Login);
        }
        
        [Fact]
        public void RevokePermissions_Unauthenticated_ShowsLoginPage()
        {
            Login(false);
            var resp = PostForm(Constants.RoutePaths.ClientPermissions, new { ClientId = clientId });
            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsoluteUri.Should().Contain(Constants.RoutePaths.Login);
        }
    }
}
