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

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Claims filter for twitter. Filters out the "urn:twitter:userid" claim.
    /// </summary>
    public class TwitterClaimsFilter : ClaimsFilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterClaimsFilter"/> class.
        /// </summary>
        public TwitterClaimsFilter()
            : this("Twitter")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterClaimsFilter"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public TwitterClaimsFilter(string provider)
            : base(provider)
        {
        }

        /// <summary>
        /// Transforms the claims.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns>Transformed claims</returns>
        protected override IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims)
        {
            return claims.Where(x => x.Type != "urn:twitter:userid");
        }
    }
}
