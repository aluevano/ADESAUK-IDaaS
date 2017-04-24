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
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.InMemory;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation.TokenRequest
{
    public class TokenRequestValidation_General_Invalid
    {
        IClientStore _clients = new InMemoryClientStore(TestClients.Get());

        [Fact]
        
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public void Parameters_Null()
        {
            var store = new InMemoryAuthorizationCodeStore();
            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            Func<Task> act = () => validator.ValidateRequestAsync(null, null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public void Client_Null()
        {
            var store = new InMemoryAuthorizationCodeStore();
            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            Func<Task> act = () => validator.ValidateRequestAsync(parameters, null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public async Task Unknown_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "unknown");
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UnsupportedGrantType);
        }

        [Fact]
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public async Task Missing_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UnsupportedGrantType);
        }
    }
}