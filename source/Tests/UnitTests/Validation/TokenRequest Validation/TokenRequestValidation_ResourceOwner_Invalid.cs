﻿/*
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
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation.TokenRequest
{

    public class TokenRequestValidation_ResourceOwner_Invalid
    {
        const string Category = "TokenRequest Validation - ResourceOwner - Invalid";

        IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_GrantType_For_Client()
        {
            var client = await _clients.FindClientByIdAsync("client");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Scopes()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Scope()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "unknown");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Scope_Multiple()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource unknown");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Scope()
        {
            var client = await _clients.FindClientByIdAsync("roclient_restricted");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource2");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Scope_Multiple()
        {
            var client = await _clients.FindClientByIdAsync("roclient_restricted");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource resource2");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_ResourceOwnerCredentials()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_ResourceOwner_UserName()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_ResourceOwner_Password()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.UserName, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_ResourceOwner_Credentials()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "notbob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
            result.ErrorDescription.Should().Be("Username and/or password incorrect");
        }
    }
}