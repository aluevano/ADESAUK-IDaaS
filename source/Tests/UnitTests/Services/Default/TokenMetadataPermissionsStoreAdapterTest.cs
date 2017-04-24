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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Services.Default
{

    public class TokenMetadataPermissionsStoreAdapterTest
    {
        List<ITokenMetadata> tokens;
        Func<string, Task<IEnumerable<ITokenMetadata>>> get;
        
        Func<string, string, Task> delete;
        string subjectDeleted;
        string clientDeleted;

        TokenMetadataPermissionsStoreAdapter subject;

        
        public TokenMetadataPermissionsStoreAdapterTest()
        {
            tokens = new List<ITokenMetadata>();
            get = s => Task.FromResult(tokens.AsEnumerable());
            delete = (subject, client) =>
            {
                subjectDeleted = subject;
                clientDeleted = client;
                return Task.FromResult(0);
            };
            this.subject = new TokenMetadataPermissionsStoreAdapter(get, delete);
        }

        class TokenMeta : ITokenMetadata
        {
            public TokenMeta(string sub, string client, IEnumerable<string> scopes)
            {
                SubjectId = sub;
                ClientId = client;
                Scopes = scopes;
            }
            public string SubjectId {get; set;}
            public string ClientId {get; set;}
            public IEnumerable<string> Scopes {get; set;}
        }

        [Fact]
        public void LoadAllAsync_CallsGet_MapsResultsToConsent()
        {
            tokens.Add(new TokenMeta("sub", "client1", new string[] { "foo", "bar" }));
            tokens.Add(new TokenMeta("sub", "client2", new string[] { "baz", "quux" }));

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(2);

            var c1 = result.Single(x=>x.ClientId == "client1");
            c1.Subject.Should().Be("sub");
            c1.Scopes.ShouldAllBeEquivalentTo(new[] { "foo", "bar" });

            var c2 = result.Single(x=>x.ClientId == "client2");
            c2.Subject.Should().Be("sub");
            c2.Scopes.ShouldAllBeEquivalentTo(new[] { "baz", "quux" });
        }

        [Fact]
        public void RevokeAsync_CallsRevoke()
        {
            subject.RevokeAsync("sub34", "client12").Wait();
            subjectDeleted.Should().Be("sub34");
            clientDeleted.Should().Be("client12");
        }
    }
}
