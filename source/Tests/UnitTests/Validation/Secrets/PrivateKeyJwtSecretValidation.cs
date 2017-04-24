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
using IdentityModel;
using IdentityServer.Core;
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.InMemory;
using IdentityServer.Core.Validation;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation.Secrets
{
    public class PrivateKeyJwtSecretValidation
    {
        readonly ISecretValidator _validator;
        readonly IClientStore _clients;

        public PrivateKeyJwtSecretValidation()
        {
            _validator = new PrivateKeyJwtSecretValidator(
                    new IdentityServerOptions()
                    {
                        DynamicallyCalculatedIssuerUri = "https://idsrv3.com"
                    }
                );
            _clients = new InMemoryClientStore(ClientValidationTestClients.Get());
        }

        private JwtSecurityToken CreateToken(string clientId, DateTime? nowOverride = null)
        {
            var certificate = TestCert.Load();
            var now = nowOverride ?? DateTime.Now;

            var token = new JwtSecurityToken(
                    clientId,
                    "https://idsrv3.com/connect/token",
                    new List<Claim>()
                    {
                        new Claim("jti", Guid.NewGuid().ToString()),
                        new Claim(JwtClaimTypes.Subject, clientId),
                        new Claim(JwtClaimTypes.IssuedAt, UnixTime(now).ToString(), ClaimValueTypes.Integer64)
                    },
                    now,
                    now.AddMinutes(1),
                    new X509SigningCredentials(certificate,
                                               SecurityAlgorithms.RsaSha256Signature,
                                               SecurityAlgorithms.Sha256Digest)
                );
            var rawCertificate = Convert.ToBase64String(certificate.Export(X509ContentType.Cert));
            token.Header.Add(JwtHeaderParameterNames.X5c, new[] { rawCertificate });
            return token;
        }

        private int UnixTime(DateTimeOffset dateTime)
        {
            return (int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        [Fact]
        public async Task Valid_Certificate_Thumbprint()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(CreateToken(clientId)),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Valid_Certificate_X5c_Only()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Header.Remove(JwtHeaderParameterNames.X5t);
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Valid_Certificate_X5t_Only()
        {
            var clientId = "certificate_base64_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Header.Remove(JwtHeaderParameterNames.X5c);
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Invalid_Certificate_X5t_Only_Requires_Full_Certificate()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Header.Remove(JwtHeaderParameterNames.X5c);
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Certificate_Thumbprint()
        {
            var clientId = "certificate_invalid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(CreateToken(clientId)),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Valid_Certificate_Base64()
        {
            var clientId = "certificate_base64_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(CreateToken(clientId)),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Invalid_Certificate_Base64()
        {
            var clientId = "certificate_base64_invalid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(CreateToken(clientId)),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Issuer()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Payload.Remove(JwtClaimTypes.Issuer);
            token.Payload.Add(JwtClaimTypes.Issuer, "invalid");
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Subject()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Payload.Remove(JwtClaimTypes.Subject);
            token.Payload.Add(JwtClaimTypes.Subject, "invalid");
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Expired_Token()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId, DateTime.Now.AddHours(-1));
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Unsigned_Token()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Header.Remove("alg");
            token.Header.Add("alg", "none");
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }
    }
}
