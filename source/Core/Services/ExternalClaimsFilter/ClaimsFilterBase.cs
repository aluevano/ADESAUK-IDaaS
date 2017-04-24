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
using System.Security.Claims;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Base external claims filter implementation. Will only execute for the configured provider and 
    /// provides a single virtual method to override to transform claims. 
    /// </summary>
    public abstract class ClaimsFilterBase : IExternalClaimsFilter
    {
        readonly string provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsFilterBase"/> class.
        /// </summary>
        /// <param name="provider">The provider this claims filter will operate against.</param>
        protected ClaimsFilterBase(string provider)
        {
            this.provider = provider;
        }

        /// <summary>
        /// Filters the specified claims from an external identity provider.
        /// </summary>
        /// <param name="provider">The identifier for the external identity provider.</param>
        /// <param name="claims">The incoming claims.</param>
        /// <returns>
        /// The transformed claims.
        /// </returns>
        public IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims)
        {
            if (this.provider == provider)
            {
                claims = TransformClaims(claims);
            }

            return claims;
        }

        /// <summary>
        /// Transforms the claims if this provider is used.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        protected abstract IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims);
    }
}
