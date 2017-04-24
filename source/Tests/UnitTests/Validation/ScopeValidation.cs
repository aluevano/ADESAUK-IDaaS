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
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.InMemory;
using IdentityServer.Core.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation
{

    public class ScopeValidation
    {
        const string Category = "Scope Validation";

        List<Scope> _allScopes = new List<Scope>
            {
                new Scope
                {
                    Name = "openid",
                    Type = ScopeType.Identity
                },
                new Scope
                {
                    Name = "email",
                    Type = ScopeType.Identity
                },
                new Scope
                {
                    Name = "resource1",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "resource2",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "disabled",
                    Enabled = false,
                    Type = ScopeType.Resource
                },
            };

        Client _unrestrictedClient = new Client
            {
                ClientId = "unrestricted",
                AllowAccessToAllScopes = true
            };

        Client _restrictedClient = new Client
            {
                ClientId = "restricted",
            
                AllowedScopes = new List<string>
                {
                    "openid",
                    "resource1",
                    "disabled"
                }
            };

        IScopeStore _store;

        public ScopeValidation()
        {
            _store = new InMemoryScopeStore(_allScopes);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Empty_Scope_List()
        {
            var scopes = ScopeValidator.ParseScopesString("");

            scopes.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Sorting()
        {
            var scopes = ScopeValidator.ParseScopesString("scope3 scope2 scope1");

            scopes.Count.Should().Be(3);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
            scopes[2].Should().Be("scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Extra_Spaces()
        {
            var scopes = ScopeValidator.ParseScopesString("   scope3     scope2     scope1   ");

            scopes.Count.Should().Be(3);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
            scopes[2].Should().Be("scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Duplicate_Scope()
        {
            var scopes = ScopeValidator.ParseScopesString("scope2 scope1 scope2");

            scopes.Count.Should().Be(2);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task All_Scopes_Valid()
        {
            var scopes = ScopeValidator.ParseScopesString("openid email resource1 resource2");
            
            var validator = new ScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Scope()
        {
            var scopes = ScopeValidator.ParseScopesString("openid email resource1 resource2 unknown");
            
            var validator = new ScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Disabled_Scope()
        {
            var scopes = ScopeValidator.ParseScopesString("openid email resource1 resource2 disabled");
            
            var validator = new ScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Allowed_For_Unrestricted_Client()
        {
            var scopes = ScopeValidator.ParseScopesString("openid email resource1 resource2");

            var validator = new ScopeValidator(_store);
            var result = validator.AreScopesAllowed(_unrestrictedClient, scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Allowed_For_Restricted_Client()
        {
            var scopes = ScopeValidator.ParseScopesString("openid resource1");

            var validator = new ScopeValidator(_store);
            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_Scopes()
        {
            var scopes = ScopeValidator.ParseScopesString("openid email resource1 resource2");

            var validator = new ScopeValidator(_store);
            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Resource_and_Identity_Scopes()
        {
            var scopes = ScopeValidator.ParseScopesString("openid email resource1 resource2");
            
            var validator = new ScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeTrue();
            validator.ContainsResourceScopes.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Resource_Scopes_Only()
        {
            var scopes = ScopeValidator.ParseScopesString("resource1 resource2");

            var validator = new ScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeFalse();
            validator.ContainsResourceScopes.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Identity_Scopes_Only()
        {
            var scopes = ScopeValidator.ParseScopesString("openid email");
            
            var validator = new ScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeTrue();
            validator.ContainsResourceScopes.Should().BeFalse();
        }
    }
}