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

using IdentityServer.Core.Models;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    internal class IntrospectionRequestValidator
    {
        private readonly TokenValidator _tokenValidator;

        public IntrospectionRequestValidator(TokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        public async Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, Scope scope)
        {
            var fail = new IntrospectionRequestValidationResult { IsError = true };

            // retrieve required token
            var token = parameters.Get("token");
            if (token == null)
            {
                fail.IsActive = false;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.MissingToken;
                return fail;
            }

            // validate token
            var tokenValidationResult = await _tokenValidator.ValidateAccessTokenAsync(token);

            // invalid or unknown token
            if (tokenValidationResult.IsError)
            {
                fail.IsActive = false;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.InvalidToken;
                fail.Token = token;
                return fail;
            }

            // check expected scope
            var expectedScope = tokenValidationResult.Claims.FirstOrDefault(
                c => c.Type == Constants.ClaimTypes.Scope && c.Value == scope.Name);

            // expected scope not present
            if (expectedScope == null)
            {
                fail.IsActive = false;
                fail.IsError = true;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.InvalidScope;
                fail.Token = token;
                return fail;
            }

            // all is good
            var success = new IntrospectionRequestValidationResult
            {
                IsActive = true,
                IsError = false,
                Token = token,
                Claims = tokenValidationResult.Claims
            };

            return success;
        }
    }
}