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
    public class X509CertificateSecretValidation
    {
        const string Category = "Secrets - X.509 Certificate Secret Validation";

        ISecretValidator _thumbprintValidator = new X509CertificateThumbprintSecretValidator();
        IClientStore _clients = new InMemoryClientStore(ClientValidationTestClients.Get());

        [Fact]
        public async Task Valid_Certificate_Thumbprint()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = TestCert.Load(),
                Type = Constants.ParsedSecretTypes.X509Certificate
            };

            var result = await _thumbprintValidator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Invalid_Certificate_Thumbprint()
        {
            var clientId = "certificate_invalid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = TestCert.Load(),
                Type = Constants.ParsedSecretTypes.X509Certificate
            };

            var result = await _thumbprintValidator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Null_Certificate_Should_Throw()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = null,
                Type = Constants.ParsedSecretTypes.X509Certificate
            };

            Func<Task> act = () => _thumbprintValidator.ValidateAsync(client.ClientSecrets, secret);

            act.ShouldThrow<ArgumentException>();
        }
    }
}