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
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IdentityServer.Tests.Validation.Secrets
{
    public class BasicAuthenticationSecretParsing
    {
        const string Category = "Secrets - Basic Authentication Secret Parsing";
        IdentityServerOptions _options;
        BasicAuthenticationSecretParser _parser;

        public BasicAuthenticationSecretParsing()
        {
            _options = new IdentityServerOptions();
            _parser = new BasicAuthenticationSecretParser(_options);
        }

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var context = new OwinContext();
            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request()
        {
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:secret")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Type.Should().Be(Constants.ParsedSecretTypes.SharedSecret);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("secret");
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header()
        {
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic" }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request_ClientId_Too_Long()
        {
            var context = new OwinContext();

            var longClientId = "x".Repeat(_options.InputLengthRestrictions.ClientId + 1);
            var credential = string.Format("{0}:secret", longClientId);

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(credential)));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);
            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request_ClientSecret_Too_Long()
        {
            var context = new OwinContext();
            
            var longClientSecret = "x".Repeat(_options.InputLengthRestrictions.ClientSecret + 1);
            var credential = string.Format("client:{0}", longClientSecret);

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(credential)));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);
            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header_Variation()
        {
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic " }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Unknown_Scheme()
        {
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Unknown" }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_NoBase64_Encoding()
        {
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic somerandomdata" }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only()
        {
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only_With_Colon()
        {
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }
    }
}