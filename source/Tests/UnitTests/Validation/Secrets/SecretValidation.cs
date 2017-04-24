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
using IdentityServer.Core.Validation;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation.Secrets
{
    public class SecretValidation
    {
        const string Category = "Secrets - Secret Validator";

        ISecretValidator _hashedSecretValidator = new HashedSharedSecretValidator();
        IClientStore _clients = new InMemoryClientStore(ClientValidationTestClients.Get());
        SecretValidator _validator;

        public SecretValidation()
        {
            _validator = new SecretValidator(new[]
            {
                _hashedSecretValidator
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Single_Secret()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);

            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Credential_Type()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = "invalid"
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Multiple_Secrets()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeTrue();

            secret.Credential = "foobar";
            result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeTrue();

            secret.Credential = "quux";
            result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeTrue();

            secret.Credential = "notexpired";
            result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Single_Secret()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "invalid",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Secret()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "expired",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Multiple_Secrets()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "invalid",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_with_no_Secret_Should_Throw()
        {
            var clientId = "no_secret_client";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Type = Constants.ParsedSecretTypes.SharedSecret
            };
            
            Func<Task> act = () => _validator.ValidateAsync(secret, client.ClientSecrets);

            act.ShouldThrow<ArgumentException>();
        }
    }
}