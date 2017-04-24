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
using System.IO;
using System.Text;
using Xunit;

namespace IdentityServer.Tests.Validation.Secrets
{
    public class FormPostCredentialExtraction
    {
        const string Category = "Secrets - Form Post Secret Parsing";
        IdentityServerOptions _options;
        PostBodySecretParser _parser;

        public FormPostCredentialExtraction()
        {
            _options = new IdentityServerOptions();
            _parser = new PostBodySecretParser(_options);
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
        public async void Valid_PostBody()
        {
            var context = new OwinContext();

            var body = "client_id=client&client_secret=secret";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Type.Should().Be(Constants.ParsedSecretTypes.SharedSecret);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("secret");
        }

        [Fact]
        public async void ClientId_Too_Long()
        {
            var context = new OwinContext();

            var longClientId = "x".Repeat(_options.InputLengthRestrictions.ClientId + 1);
            var body = string.Format("client_id={0}&client_secret=secret", longClientId);

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void ClientSecret_Too_Long()
        {
            var context = new OwinContext();

            var longClientSecret = "x".Repeat(_options.InputLengthRestrictions.ClientSecret + 1);
            var body = string.Format("client_id=client&client_secret={0}", longClientSecret);

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Missing_ClientId()
        {
            var context = new OwinContext();

            var body = "client_secret=secret";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Missing_ClientSecret()
        {
            var context = new OwinContext();

            var body = "client_id=client";

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