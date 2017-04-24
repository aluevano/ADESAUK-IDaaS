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

using IdentityServer.Core.Extensions;
using Microsoft.Owin;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    internal class BearerTokenUsageValidator
    {
        public async Task<BearerTokenUsageValidationResult> ValidateAsync(IOwinContext context)
        {
            var result = ValidateAuthorizationHeader(context);
            if (result.TokenFound)
            {
                return result;
            }

            if (context.Request.IsFormData())
            {
                result = await ValidatePostBodyAsync(context);
                if (result.TokenFound)
                {
                    return result;
                }
            }

            return new BearerTokenUsageValidationResult();
        }

        public BearerTokenUsageValidationResult ValidateAuthorizationHeader(IOwinContext context)
        {
            var authorizationHeaders = context.Request.Headers.GetValues("Authorization");
            if (authorizationHeaders != null)
            {
                var header = authorizationHeaders.First().Trim();
                if (header.StartsWith(Constants.AuthenticationSchemes.BearerAuthorizationHeader))
                {
                    var value = header.Substring(Constants.AuthenticationSchemes.BearerAuthorizationHeader.Length).Trim();
                    if (value != null && value.Length > 0)
                    {
                        return new BearerTokenUsageValidationResult
                        {
                            TokenFound = true,
                            Token = value,
                            UsageType = BearerTokenUsageType.AuthorizationHeader
                        };
                    }
                }
            }

            return new BearerTokenUsageValidationResult();
        }

        public async Task<BearerTokenUsageValidationResult> ValidatePostBodyAsync(IOwinContext context)
        {
            var form = await context.ReadRequestFormAsNameValueCollectionAsync();

            var token = form.Get(Constants.AuthenticationSchemes.BearerFormPost);
            if (token.IsPresent())
            {
                return new BearerTokenUsageValidationResult
                {
                    TokenFound = true,
                    Token = token,
                    UsageType = BearerTokenUsageType.PostBody
                };
            }

            return new BearerTokenUsageValidationResult();
        }
    }
}