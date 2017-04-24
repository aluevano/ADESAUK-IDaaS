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
using IdentityServer.Core.Models;
using IdentityServer.Core.Services.Default;
using IdentityServer.Core.Services.InMemory;
using System.Linq;
using Xunit;

namespace IdentityServer.Tests.Services.Default
{
    public class AggregatePermissionsStoreTests
    {
        AggregatePermissionsStore subject;
        InMemoryConsentStore store1;
        InMemoryConsentStore store2;

        public AggregatePermissionsStoreTests()
        {
            store1 = new InMemoryConsentStore();
            store2 = new InMemoryConsentStore();
            subject = new AggregatePermissionsStore(store1, store2);
        }

        [Fact]
        public void LoadAllAsync_EmptyStores_ReturnsEmptyConsentCollection()
        {
            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(0);
        }

        [Fact]
        public void LoadAllAsync_OnlyOneStoreHasConsent_ReturnsSameConsent()
        {
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new [] { "foo", "bar" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveSameConsent_ReturnsSameConsent()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new [] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new [] { "foo", "bar" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveOverlappingConsent_ReturnsCorrectUnion()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "bar", "baz" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new[] { "foo", "bar", "baz" });
        }

        [Fact]
        public void LoadAllAsync_BothStoresHaveDifferentConsent_ReturnsCorrectUnion()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "quux", "baz" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new[] { "foo", "bar", "baz", "quux" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveMultipleClientConsent_ReturnsCorrectConsent()
        {
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new[] { "foo1" } });
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "bad", Scopes = new[] { "bad" } });
            store1.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new[] { "foo1", "foo2" } });
            store1.UpdateAsync(new Consent { ClientId = "client2", Subject = "bad", Scopes = new[] { "bad" } });
            store1.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new[] { "foo1", "foo2", "foo3" } });
            store1.UpdateAsync(new Consent { ClientId = "client3", Subject = "bad", Scopes = new[] { "bad" } });
            
            store2.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new[] { "bar1" } });
            store2.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new[] { "bar1", "bar2" } });
            store2.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new[] { "bar1", "bar2", "bar3" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(3);

            var c1 = result.Single(x => x.ClientId == "client1");
            c1.Subject.Should().Be("sub");
            c1.Scopes.ShouldAllBeEquivalentTo(new[] { "foo1", "bar1" });

            var c2 = result.Single(x => x.ClientId == "client2");
            c1.Subject.Should().Be("sub");
            c2.Scopes.ShouldAllBeEquivalentTo(new[] { "foo1", "bar1", "foo2", "bar2" });
            
            var c3 = result.Single(x => x.ClientId == "client3");
            c1.Subject.Should().Be("sub");
            c3.Scopes.ShouldAllBeEquivalentTo(new[] { "foo1", "bar1", "foo2", "bar2", "foo3", "bar3" });
        }

        [Fact]
        public void RevokeAsync_DeletesInAllStores()
        {
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new string[] { "foo1" } });
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "bad", Scopes = new string[] { "bad" } });

            store2.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new string[] { "bar1" } });
            store2.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new string[] { "bar1", "bar2" } });
            store2.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new string[] { "bar1", "bar2", "bar3" } });

            subject.RevokeAsync("sub", "client1").Wait();
            store1.LoadAllAsync("sub").Result.Count().Should().Be(0);
            store2.LoadAllAsync("sub").Result.Count().Should().Be(2);
        }
    }
}
