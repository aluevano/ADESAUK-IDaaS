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
using IdentityServer.Core.Services.InMemory;
using IdentityServer.Core.Validation;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation
{
    public class RevocationRequestValidation
    {
        const string Category = "Revocation Request Validation Tests";

        TokenRevocationRequestValidator _validator;
        IRefreshTokenStore _refreshTokens;
        ITokenHandleStore _tokenHandles;
        IClientStore _clients;

        public RevocationRequestValidation()
        {
            _refreshTokens = new InMemoryRefreshTokenStore();
            _tokenHandles = new InMemoryTokenHandleStore();
            _clients = new InMemoryClientStore(TestClients.Get());

            _validator = new TokenRevocationRequestValidator();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Parameters()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var parameters = new NameValueCollection();

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_Token_Valid_Hint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            
            var parameters = new NameValueCollection
            {
                { "token_type_hint", "access_token" }
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_And_AccessTokenHint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");

            var parameters = new NameValueCollection
            {
                { "token", "foo" },
                { "token_type_hint", "access_token" }
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
            result.Token.Should().Be("foo");
            result.TokenTypeHint.Should().Be("access_token");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_and_RefreshTokenHint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");

            var parameters = new NameValueCollection
            {
                { "token", "foo" },
                { "token_type_hint", "refresh_token" }
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
            result.Token.Should().Be("foo");
            result.TokenTypeHint.Should().Be("refresh_token");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_And_Missing_Hint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");

            var parameters = new NameValueCollection
            {
                { "token", "foo" },
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
            result.Token.Should().Be("foo");
            result.TokenTypeHint.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_And_Invalid_Hint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");

            var parameters = new NameValueCollection
            {
                { "token", "foo" },
                { "token_type_hint", "invalid" }
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.RevocationErrors.UnsupportedTokenType);
        }
    }
}