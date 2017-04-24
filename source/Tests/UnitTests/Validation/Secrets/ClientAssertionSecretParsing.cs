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
using IdentityServer.Core.Validation;
using Microsoft.Owin;
using System.IdentityModel.Tokens;
using System.IO;
using System.Text;
using Xunit;

namespace IdentityServer.Tests.Validation.Secrets
{
    public class ClientAssertionSecretParsing
    {
        IdentityServerOptions _options;
        ClientAssertionSecretParser _parser;

        public ClientAssertionSecretParsing()
        {
            _options = new IdentityServerOptions();
            _parser = new ClientAssertionSecretParser();
        }

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var context = new OwinContext();
            context.Request.Body = new MemoryStream();

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Valid_ClientAssertion()
        {
            var context = new OwinContext();

            var body = "client_id=client&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion=token";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().NotBeNull();
            secret.Type.Should().Be(Constants.ParsedSecretTypes.JwtBearer);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("token");
        }

        [Fact]
        public async void Valid_ClientAssertion_ImplicitClientId()
        {
            var context = new OwinContext();

            var token = new JwtSecurityToken(issuer: "client");
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var body = "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion=" + tokenString;

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().NotBeNull();
            secret.Type.Should().Be(Constants.ParsedSecretTypes.JwtBearer);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be(tokenString);
        }

        [Fact]
        public async void Missing_ClientAssertionType()
        {
            var context = new OwinContext();

            var body = "client_id=client&client_assertion=token";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Missing_ClientAssertion()
        {
            var context = new OwinContext();

            var body = "client_id=client&client_assertion_type=urn:ietf:params:oauth:grant-type:jwt-bearer";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Malformed_PostBody()
        {
            var context = new OwinContext();

            var body = "malformed";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }
    }
}
