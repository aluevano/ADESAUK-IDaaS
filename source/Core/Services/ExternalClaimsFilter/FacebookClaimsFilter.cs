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
    /// Claims filter for facebook. Converts the "urn:facebook:name" claim to the "name" claim.
    /// </summary>
    public class FacebookClaimsFilter : ClaimsFilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookClaimsFilter"/> class.
        /// </summary>
        public FacebookClaimsFilter()
            : this("Facebook")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookClaimsFilter"/> class.
        /// </summary>
        /// <param name="provider">The provider this claims filter will operate against.</param>
        public FacebookClaimsFilter(string provider)
            : base(provider)
        {
        }

        /// <summary>
        /// Transforms the claims if this provider is used.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        protected override IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims)
        {
            var nameClaim = claims.FirstOrDefault(x => x.Type == "urn:facebook:name");
            if (nameClaim != null)
            {
                var list = claims.ToList();
                list.Remove(nameClaim);
                list.RemoveAll(x => x.Type == Constants.ClaimTypes.Name);
                list.Add(new Claim(Constants.ClaimTypes.Name, nameClaim.Value));
                return list;
            }
            return claims;
        }
    }
}
