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
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.InMemory;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation.Tokens
{
    public class AccessTokenValidation : IDisposable
    {
        const string Category = "Access token validation";

        IClientStore _clients = Factory.CreateClientStore();

        static AccessTokenValidation()
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
        }

        DateTimeOffset now;
        public DateTimeOffset UtcNow
        {
            get
            {
                if (now > DateTimeOffset.MinValue) return now;
                return DateTimeOffset.UtcNow;
            }
        }

        Func<DateTimeOffset> originalNowFunc;
        
        public AccessTokenValidation()
        {
            originalNowFunc = DateTimeOffsetHelper.UtcNowFunc;
            DateTimeOffsetHelper.UtcNowFunc = () => UtcNow;
        }

        public void Dispose()
        {
            if (originalNowFunc != null)
            {
                DateTimeOffsetHelper.UtcNowFunc = originalNowFunc;
            }
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeFalse();
            result.Claims.Count().Should().Be(8);
            result.Claims.First(c => c.Type == Constants.ClaimTypes.ClientId).Value.Should().Be("roclient");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_required_Scope()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "read");

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_missing_Scope()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "missing");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InsufficientScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var result = await validator.ValidateAccessTokenAsync("unknown");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Reference_Token_Too_Long()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);
            var options = new IdentityServerOptions();

            var longToken = "x".Repeat(options.InputLengthRestrictions.TokenHandle + 1);
            var result = await validator.ValidateAccessTokenAsync(longToken);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Reference_Token()
        {
            now = DateTimeOffset.UtcNow;

            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 2, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);
            
            now = now.AddMilliseconds(2000);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.ExpiredToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Malformed_JWT_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var result = await validator.ValidateAccessTokenAsync("unk.nown");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_JWT_Token()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write"));

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Issuer()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            token.Issuer = "invalid";
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_Too_Long()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateAccessTokenLong(new Client { ClientId = "roclient" }, "valid", 600, 1000, "read", "write"));
            
            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Audience()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            token.Audience = "invalid";
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_AccessToken_but_User_not_active()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(u => u.IsActiveAsync(It.IsAny<IsActiveContext>())).Callback<IsActiveContext>(ctx=>{
                ctx.IsActive = false;
            }).Returns(Task.FromResult(0));                        

            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(tokenStore: store, users: mock.Object);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "invalid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_AccessToken_but_Client_not_active()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "unknown" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
        }
    }
}