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
using IdentityServer.Core.Services;
using IdentityServer.Core.Validation;
using Moq;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation.TokenRequest
{

    public class TokenRequestValidation_CustomGrants_Invalid
    {
        const string Category = "TokenRequest Validation - AssertionFlow - Invalid";

        IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Custom_Grant_Type_For_Client_Credentials_Client()
        {
            var client = await _clients.FindClientByIdAsync("client");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "customGrant");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UnsupportedGrantType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Custom_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "unknown_grant_type");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UnsupportedGrantType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Failing_Custom_Grant_Validator()
        {
            var grantType = "custom_grant";
            var client = await _clients.FindClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator(customGrantValidators: new[] { AlwaysFailingCustomGrantValidator(grantType).Object });

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, grantType);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be("Grant validation error");
        }

        private static Mock<ICustomGrantValidator> AlwaysFailingCustomGrantValidator(string grantType)
        {
            var alwaysFailingCustomGrantValidator = new Mock<ICustomGrantValidator>();
            alwaysFailingCustomGrantValidator.Setup(y => y.ValidateAsync(It.IsAny<ValidatedTokenRequest>()))
                .Throws<NotSupportedException>();
            alwaysFailingCustomGrantValidator.SetupGet(y => y.GrantType).Returns(grantType);
            return alwaysFailingCustomGrantValidator;
        }

    }
}