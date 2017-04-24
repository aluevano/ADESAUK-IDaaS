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
using IdentityServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Caching
{
    /// <summary>
    /// <see cref="IUserService"/> decorator implementation that uses the provided <see cref="ICache{T}"/> for caching the user profile data.
    /// </summary>
    public class CachingUserService : IUserService
    {
        readonly IUserService inner;
        readonly ICache<IEnumerable<Claim>> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingUserService"/> class.
        /// </summary>
        /// <param name="inner">The inner <see cref="IUserService"/>.</param>
        /// <param name="cache">The cache.</param>
        /// <exception cref="System.ArgumentNullException">
        /// inner
        /// or
        /// cache
        /// </exception>
        public CachingUserService(IUserService inner, ICache<IEnumerable<Claim>> cache)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (cache == null) throw new ArgumentNullException("cache");

            this.inner = inner;
            this.cache = cache;
        }

        /// <summary>
        /// This method gets called before the login page is shown. This allows you to authenticate the
        /// user somehow based on data coming from the host (e.g. client certificates or trusted headers)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            return inner.PreAuthenticateAsync(context);
        }

        /// <summary>
        /// This method gets called for local authentication (whenever the user uses the username and password dialog).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            return inner.AuthenticateLocalAsync(context);
        }

        /// <summary>
        /// This method gets called when the user uses an external identity provider to authenticate.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            return inner.AuthenticateExternalAsync(context);
        }

        /// <summary>
        /// This method is called prior to the user being issued a login cookie for IdentityServer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            return inner.PostAuthenticateAsync(context);
        }
        
        /// <summary>
        /// This method gets called when the user signs out.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task SignOutAsync(SignOutContext context)
        {
            return inner.SignOutAsync(context);
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var key = GetKey(context.Subject, context.RequestedClaimTypes);
            context.IssuedClaims = await cache.GetAsync(key, async () =>
            {
                await inner.GetProfileDataAsync(context);
                return context.IssuedClaims;
            });
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task IsActiveAsync(IsActiveContext context)
        {
            return inner.IsActiveAsync(context);
        }

        private string GetKey(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes)
        {
            var sub = subject.GetSubjectId();
            if (requestedClaimTypes == null) return sub;

            return sub + ":" + requestedClaimTypes.OrderBy(x => x).Aggregate((x, y) => x + "," + y);
        }
    }
}
