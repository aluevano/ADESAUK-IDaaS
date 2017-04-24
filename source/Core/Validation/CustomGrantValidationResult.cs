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

using IdentityModel;
using IdentityServer.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Models the result of custom grant validation.
    /// </summary>
    public class CustomGrantValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the principal which represents the result of the authentication.
        /// </summary>
        /// <value>
        /// The principal.
        /// </value>
        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGrantValidationResult"/> class with an error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public CustomGrantValidationResult(string errorMessage)
        {
            Error = errorMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGrantValidationResult"/> class with no error and no user.
        /// </summary>
        public CustomGrantValidationResult()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGrantValidationResult"/> class.
        /// </summary>
        /// <param name="subject">The subject claim used to uniquely identifier the user.</param>
        /// <param name="authenticationMethod">The authentication method which describes the custom grant type.</param>
        /// <param name="claims">Additional claims that will be maintained in the principal.</param>
        /// <param name="identityProvider">The identity provider.</param>
        public CustomGrantValidationResult(
            string subject, 
            string authenticationMethod,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider)
        {
            var resultClaims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, subject),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, authenticationMethod),
                new Claim(Constants.ClaimTypes.IdentityProvider, identityProvider),
                new Claim(Constants.ClaimTypes.AuthenticationTime, DateTimeOffsetHelper.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer)
            };

            if (claims != null && claims.Any())
            {
                resultClaims.AddRange(claims.Where(x => !Constants.OidcProtocolClaimTypes.Contains(x.Type)));
            }

            var id = new ClaimsIdentity(authenticationMethod);
            id.AddClaims(resultClaims.Distinct(new ClaimComparer()));

            Principal = new ClaimsPrincipal(id);

            IsError = false;
        }
    }
}