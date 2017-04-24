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
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.Default;
using IdentityServer.Core.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Connect.ResponseHandling
{
    public class DefaultRefreshTokenServiceTests : IDisposable
    {
        const string Category = "Default RefreshToken Service";

        IRefreshTokenStore refreshTokenStore;
        DefaultRefreshTokenService service;

        Client roclient_absolute_refresh_expiration_one_time_only;
        Client roclient_sliding_refresh_expiration_one_time_only;
        Client roclient_absolute_refresh_expiration_reuse;

        ClaimsPrincipal user;

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

        public DefaultRefreshTokenServiceTests()
        {
            originalNowFunc = DateTimeOffsetHelper.UtcNowFunc;
            DateTimeOffsetHelper.UtcNowFunc = () => UtcNow;

            roclient_absolute_refresh_expiration_one_time_only = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_absolute_refresh_expiration_one_time_only",
                ClientSecrets = new List<Secret>
                { 
                    new Secret("secret".Sha256())
                },

                Flow = Flows.ResourceOwner,

                RefreshTokenExpiration = TokenExpiration.Absolute,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                AbsoluteRefreshTokenLifetime = 200
            };

            roclient_sliding_refresh_expiration_one_time_only = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_sliding_refresh_expiration_one_time_only",
                ClientSecrets = new List<Secret>
                { 
                    new Secret("secret".Sha256())
                },

                Flow = Flows.ResourceOwner,

                RefreshTokenExpiration = TokenExpiration.Sliding,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                AbsoluteRefreshTokenLifetime = 10,
                SlidingRefreshTokenLifetime = 4
            };

            roclient_absolute_refresh_expiration_reuse = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_absolute_refresh_expiration_reuse",
                ClientSecrets = new List<Secret>
                { 
                    new Secret("secret".Sha256())
                },

                Flow = Flows.ResourceOwner,

                RefreshTokenExpiration = TokenExpiration.Absolute,
                RefreshTokenUsage = TokenUsage.ReUse,
                AbsoluteRefreshTokenLifetime = 200
            };

            refreshTokenStore = new InMemoryRefreshTokenStore();
            service = new DefaultRefreshTokenService(refreshTokenStore, new DefaultEventService());

            user = IdentityServerPrincipal.Create("bob", "Bob Loblaw");
        }

        public void Dispose()
        {
            if (originalNowFunc != null)
            {
                DateTimeOffsetHelper.UtcNowFunc = originalNowFunc;
            }
        }

        Token CreateAccessToken(Client client, string subjectId, int lifetime, params string[] scopes)
        {
            var claims = new List<Claim> 
            {
                new Claim("client_id", client.ClientId),
                new Claim("sub", subjectId)
            };

            scopes.ToList().ForEach(s => claims.Add(new Claim("scope", s)));

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = "https://idsrv3.com/resources",
                Issuer = "https://idsrv3.com",
                Lifetime = lifetime,
                Claims = claims,
                Client = client
            };

            return token;
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Create_Refresh_Token_Absolute_Lifetime()
        {
            var client = roclient_absolute_refresh_expiration_one_time_only;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(user, token, client);

            // make sure a handle is returned
            string.IsNullOrWhiteSpace(handle).Should().BeFalse();

            // make sure refresh token is in store
            var refreshToken = await refreshTokenStore.GetAsync(handle);
            refreshToken.Should().NotBeNull();

            // check refresh token values
            client.ClientId.Should().Be(refreshToken.ClientId);
            client.AbsoluteRefreshTokenLifetime.Should().Be(refreshToken.LifeTime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Create_Refresh_Token_Sliding_Lifetime()
        {
            var client = roclient_sliding_refresh_expiration_one_time_only;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(user, token, client);

            // make sure a handle is returned
            string.IsNullOrWhiteSpace(handle).Should().BeFalse();

            // make sure refresh token is in store
            var refreshToken = await refreshTokenStore.GetAsync(handle);
            refreshToken.Should().NotBeNull();

            // check refresh token values
            client.ClientId.Should().Be(refreshToken.ClientId);
            client.SlidingRefreshTokenLifetime.Should().Be(refreshToken.LifeTime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Sliding_Expiration_does_not_exceed_absolute_Expiration()
        {
            now = DateTimeOffset.UtcNow;

            var client = roclient_sliding_refresh_expiration_one_time_only;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(user, token, client);
            var refreshToken = await refreshTokenStore.GetAsync(handle);
            var lifetime = refreshToken.LifeTime;

            now = now.AddSeconds(8);

            var newHandle = await service.UpdateRefreshTokenAsync(handle, refreshToken, client);
            var newRefreshToken = await refreshTokenStore.GetAsync(newHandle);
            var newLifetime = newRefreshToken.LifeTime;

            newLifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Sliding_Expiration_within_absolute_Expiration()
        {
            now = DateTimeOffset.UtcNow;

            var client = roclient_sliding_refresh_expiration_one_time_only;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(user, token, client);
            var refreshToken = await refreshTokenStore.GetAsync(handle);
            var lifetime = refreshToken.LifeTime;

            now = now.AddSeconds(1);

            var newHandle = await service.UpdateRefreshTokenAsync(handle, refreshToken, client);
            var newRefreshToken = await refreshTokenStore.GetAsync(newHandle);
            var newLifetime = newRefreshToken.LifeTime;

            (client.SlidingRefreshTokenLifetime + 1).Should().Be(newLifetime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ReUse_Handle_reuses_Handle()
        {
            var client = roclient_absolute_refresh_expiration_reuse;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(user, token, client);
            var newHandle = await service.UpdateRefreshTokenAsync(handle, await refreshTokenStore.GetAsync(handle), client);

            newHandle.Should().Be(handle);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OneTime_Handle_creates_new_Handle()
        {
            var client = roclient_absolute_refresh_expiration_one_time_only;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(user, token, client);
            var newHandle = await service.UpdateRefreshTokenAsync(handle, await refreshTokenStore.GetAsync(handle), client);

            newHandle.Should().NotBe(handle);
        }
    }
}