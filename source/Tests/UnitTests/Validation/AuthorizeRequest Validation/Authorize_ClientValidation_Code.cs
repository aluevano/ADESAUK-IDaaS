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
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Validation.AuthorizeRequest
{
    public class Authorize_ClientValidation_Code
    {
        const string Category = "AuthorizeRequest Client Validation - Code";
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", Category)]
        public async Task Code_Request_Unknown_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "unknown");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OpenId_Code_Request_Invalid_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://invalid");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OpenId_Code_Request_Invalid_IdToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OpenId_Code_Request_Invalid_IdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OpenId_Code_Request_With_Unknown_Client()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "unknown");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OpenId_Code_Request_With_Restricted_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient_restricted");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OpenId_CodeIdTokenToken_with_NoTokenViaBrowser_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "hybridclient.nobrowser");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.CodeIdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "nonce");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().Be(true);
        }


        [Theory]
        [InlineData("codewithproofkeyclient", Constants.ResponseTypes.Code)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeIdToken)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeToken)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeIdTokenToken)]
        [Trait("Category", Category)]
        public async Task ProofKey_Request_No_CodeChallenge(string clientId, string responseType)
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, clientId);
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, responseType);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
            result.ErrorDescription.Should().Be("code challenge required");
        }

        [Theory]
        [InlineData("codewithproofkeyclient", Constants.ResponseTypes.Code)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeIdToken)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeToken)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeIdTokenToken)]
        [Trait("Category", Category)]
        public async Task ProofKey_Request_CodeChallenge_Too_Short(string clientId, string responseType)
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, clientId);
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, responseType);
            parameters.Add(Constants.AuthorizeRequest.CodeChallenge, "a".Repeat(_options.InputLengthRestrictions.CodeChallengeMinLength - 1));

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Theory]
        [InlineData("codewithproofkeyclient", Constants.ResponseTypes.Code)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeIdToken)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeToken)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeIdTokenToken)]
        [Trait("Category", Category)]
        public async Task ProofKey_Request_CodeChallenge_Too_Long(string clientId, string responseType)
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, clientId);
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, responseType);
            parameters.Add(Constants.AuthorizeRequest.CodeChallenge, "a".Repeat(_options.InputLengthRestrictions.CodeChallengeMaxLength + 1));

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Theory]
        [InlineData("codewithproofkeyclient", Constants.ResponseTypes.Code)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeIdToken)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeToken)]
        [InlineData("hybridwithproofkeyclient", Constants.ResponseTypes.CodeIdTokenToken)]
        [Trait("Category", Category)]
        public async Task ProofKey_Request_Unsupported_CodeChallengeMethod(string clientId, string responseType)
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, clientId);
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, responseType);
            parameters.Add(Constants.AuthorizeRequest.CodeChallenge, "a".Repeat(_options.InputLengthRestrictions.CodeChallengeMinLength));
            parameters.Add(Constants.AuthorizeRequest.CodeChallengeMethod, "unknown");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
            result.ErrorDescription.Should().Be("transform algorithm not supported");
        }
    }
}