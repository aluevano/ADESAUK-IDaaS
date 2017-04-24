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
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Models;
using IdentityServer.Core.ResponseHandling;
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.Default;
using IdentityServer.Core.Validation;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Connect.ResponseHandling
{

    public class AuthorizeInteractionResponseGeneratorTests_Login
    {
        IdentityServerOptions options;

        public AuthorizeInteractionResponseGeneratorTests_Login()
        {
            options = new IdentityServerOptions();
        }

        [Fact]
        public async Task Anonymous_User_must_SignIn()
        {
            var generator = new AuthorizeInteractionResponseGenerator(options, null, null, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo"
            };

            var result = await generator.ProcessLoginAsync(request, Principal.Anonymous);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client()
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_allowed_current_Idp_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client 
                {
                    IdentityProviderRestrictions = new List<string> 
                    {
                        Constants.BuiltInIdentityProvider
                    }
                }
            };

            var result = await generator.ProcessClientLoginAsync(request);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_restricted_current_Idp_must_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    IdentityProviderRestrictions = new List<string> 
                    {
                        "some_idp"
                    }
                }
            };

            var result = await generator.ProcessClientLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_allowed_requested_Idp_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client(),
                 AuthenticationContextReferenceClasses = new List<string>{
                    "idp:" + Constants.BuiltInIdentityProvider
                }
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_different_requested_Idp_must_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client(),
                AuthenticationContextReferenceClasses = new List<string>{
                    "idp:some_idp"
                },
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_local_Idp_must_SignIn_when_global_options_does_not_allow_local_logins()
        {
            options.AuthenticationOptions.EnableLocalLogin = false;

            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    ClientId = "foo",
                    EnableLocalLogin = true
                }
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessClientLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_local_Idp_must_SignIn_when_client_options_does_not_allow_local_logins()
        {
            options.AuthenticationOptions.EnableLocalLogin = true;

            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    ClientId = "foo",
                    EnableLocalLogin = false
                }
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessClientLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }
    }
}
