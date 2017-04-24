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
using IdentityServer.Core.Services.Default;
using IdentityServer.Core.Services.InMemory;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace IdentityServer.Tests.Services.Default
{
    public class DefaultConsentServiceTests
    {
        DefaultConsentService subject;
        InMemoryConsentStore store;
        ClaimsPrincipal user;
        Client client;
        List<string> scopes;
        
        public DefaultConsentServiceTests()
        {
            scopes = new List<string> { "read", "write" };
            client = new Client {ClientId = "client", AllowRememberConsent = true, RequireConsent = true};
            user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]{new Claim(Constants.ClaimTypes.Subject, "123")}, "password"));
            store = new InMemoryConsentStore();
            subject = new DefaultConsentService(store);
        }

        [Fact]
        public void RequiresConsentAsync_NoPriorConsentGiven_ReturnsTrue()
        {
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_ReturnsFalse()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_ScopesInDifferentOrder_ReturnsFalse()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            scopes.Reverse();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_MoreScopesRequested_ReturnsTrue()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            scopes.Add("query");
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_FewerScopesRequested_ReturnsFalse()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            scopes.RemoveAt(0);
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeFalse();
        }
        
        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_NoScopesRequested_ReturnsFalse()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            scopes.Clear();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresConsentAsync_ClientDoesNotRequireConsent_ReturnsFalse()
        {
            client.RequireConsent = false;
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresConsentAsync_ClientDoesNotAllowRememberConsent_ReturnsTrue()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            client.AllowRememberConsent = false;
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateConsentAsync_ConsentGiven_ConsentNoLongerRequired()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            var result =  subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void UpdateConsentAsync_ClientDoesNotAllowRememberConsent_ConsentStillRequired()
        {
            client.AllowRememberConsent = false;
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateConsentAsync_PriorConsentGiven_NullScopes_ConsentNowRequired()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            subject.UpdateConsentAsync(client, user, null).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateConsentAsync_PriorConsentGiven_EmptyScopeCollection_ConsentNowRequired()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            subject.UpdateConsentAsync(client, user, new string[0]).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeTrue();
        }
        
        [Fact]
        public void UpdateConsentAsync_ChangeConsent_OldConsentNotAllowed()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            var newConsent = new string[] { "foo", "bar" };
            subject.UpdateConsentAsync(client, user, newConsent).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void Offline_access_scope_always_requires_consent_if_client_consent_is_enabled()
        {
            var requested_scopes = scopes.ToList();
            requested_scopes.Add(Constants.StandardScopes.OfflineAccess);

            // update DB as if we've previosuly consented
            subject.UpdateConsentAsync(client, user, requested_scopes).Wait();

            var result = subject.RequiresConsentAsync(client, user, requested_scopes).Result;
            result.Should().BeTrue();
        }
        
        [Fact]
        public void Offline_access_scope_does_not_always_require_consent_if_client_consent_is_disabled()
        {
            client.RequireConsent = false;

            var requested_scopes = scopes.ToList();
            requested_scopes.Add(Constants.StandardScopes.OfflineAccess);

            // update DB as if we've previosuly consented
            subject.UpdateConsentAsync(client, user, requested_scopes).Wait();

            var result = subject.RequiresConsentAsync(client, user, requested_scopes).Result;
            result.Should().BeFalse();
        }
    }
}
