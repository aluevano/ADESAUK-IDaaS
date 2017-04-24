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
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Services;
using IdentityServer.Core.Validation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation
{
    public class CustomGrantValidation
    {
        const string Category = "Validation - Custom Grant Validation";

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Custom_Grant_Validator_Throws_Exception()
        {
            var validatorThrowingException = new Mock<ICustomGrantValidator>();
            validatorThrowingException.Setup(y => y.ValidateAsync(It.IsAny<ValidatedTokenRequest>())).Throws(new Exception("Random validation error"));
            validatorThrowingException.Setup(y => y.GrantType).Returns("custom_grant");
            var validator = new CustomGrantValidator(new[] { validatorThrowingException.Object});
            var request = new ValidatedTokenRequest
            {
                GrantType = validator.GetAvailableGrantTypes().Single()
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be("Grant validation error");
            result.Principal.Should().BeNull();
            
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Custom_Grant_Single_Validator()
        {
            var validator = new CustomGrantValidator(new[] { new TestGrantValidator() });
            var request = new ValidatedTokenRequest
            {
                GrantType = "custom_grant"
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeFalse();
            result.Principal.Should().NotBeNull();
            result.Principal.GetSubjectId().Should().Be("bob");
            result.Principal.GetAuthenticationMethod().Should().Be("CustomGrant");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Custom_Grant_Multiple_Validator()
        {
            var validator = new CustomGrantValidator(new List<ICustomGrantValidator> 
            { 
                new TestGrantValidator(), 
                new TestGrantValidator2() 
            });

            var request = new ValidatedTokenRequest
            {
                GrantType = "custom_grant"
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeFalse();
            result.Principal.Should().NotBeNull();
            result.Principal.GetSubjectId().Should().Be("bob");
            result.Principal.GetAuthenticationMethod().Should().Be("CustomGrant");

            request.GrantType = "custom_grant2";
            result = await validator.ValidateAsync(request);

            result.IsError.Should().BeFalse();
            result.Principal.Should().NotBeNull();
            result.Principal.GetSubjectId().Should().Be("alice");
            result.Principal.GetAuthenticationMethod().Should().Be("CustomGrant2");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Custom_Grant_Multiple_Validator()
        {
            var validator = new CustomGrantValidator(new List<ICustomGrantValidator> 
            { 
                new TestGrantValidator(), 
                new TestGrantValidator2() 
            });

            var request = new ValidatedTokenRequest
            {
                GrantType = "unknown"
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Validator_List()
        {
            var validator = new CustomGrantValidator(new List<ICustomGrantValidator>());

            var request = new ValidatedTokenRequest
            {
                GrantType = "something"
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetAvailable_Should_Return_Expected_GrantTypes()
        {
            var validator = new CustomGrantValidator(new List<ICustomGrantValidator> 
            { 
                new TestGrantValidator(), 
                new TestGrantValidator2() 
            });

            var available = validator.GetAvailableGrantTypes();

            available.Count().Should().Be(2);
            available.First().Should().Be("custom_grant");
            available.Skip(1).First().Should().Be("custom_grant2");
        }
    }
}