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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.Core.Models
{
    /// <summary>
    /// Models the identity of a user authenticating from an external identity provider.
    /// </summary>
    public class ExternalIdentity
    {
        /// <summary>
        /// Identifier of the external identity provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public string Provider { get; set; }


        /// <summary>
        /// User's unique identifier provided by the external identity provider.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        public string ProviderId { get; set; }

        /// <summary>
        /// Claims supplied for the user from the external identity provider.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IEnumerable<Claim> Claims { get; set; }

        /// <summary>
        /// Creates an ExternalIdentity and determines the Provider and ProviderId from the list of claims.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">claims</exception>
        public static ExternalIdentity FromClaims(IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException("claims");

            var subClaim = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Subject);
            if (subClaim == null)
            {
                subClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (subClaim == null)
                {
                    return null;
                }
            }

            claims = claims.Except(new[] { subClaim });
            
            return new ExternalIdentity
            {
                Provider = subClaim.Issuer,
                ProviderId = subClaim.Value,
                Claims = claims
            };
        }
    }
}