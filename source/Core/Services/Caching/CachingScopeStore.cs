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
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Caching
{
    /// <summary>
    /// <see cref="IScopeStore"/> decorator implementation that uses the provided <see cref="ICache{T}"/> for caching the scopes.
    /// </summary>
    public class CachingScopeStore : IScopeStore
    {
        const string AllScopes = "CachingScopeStore.allscopes";
        const string AllScopesPublic = AllScopes + ".public";

        readonly IScopeStore inner;
        readonly ICache<IEnumerable<Scope>> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingScopeStore"/> class.
        /// </summary>
        /// <param name="inner">The inner <see cref="IScopeStore"/>.</param>
        /// <param name="cache">The cache.</param>
        /// <exception cref="System.ArgumentNullException">
        /// inner
        /// or
        /// cache
        /// </exception>
        public CachingScopeStore(IScopeStore inner, ICache<IEnumerable<Scope>> cache)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (cache == null) throw new ArgumentNullException("cache");

            this.inner = inner;
            this.cache = cache;
        }

        /// <summary>
        /// Gets all scopes.
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns>
        /// List of scopes
        /// </returns>
        public async Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            var key = GetKey(scopeNames);
            return await cache.GetAsync(key, async () => await inner.FindScopesAsync(scopeNames));
        }

        /// <summary>
        /// Gets all defined scopes.
        /// </summary>
        /// <param name="publicOnly">if set to <c>true</c> only public scopes are returned.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            var key = GetKey(publicOnly);
            return await cache.GetAsync(key, async () => await inner.GetScopesAsync(publicOnly));
        }

        private string GetKey(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null || !scopeNames.Any()) return "";
            return scopeNames.OrderBy(x => x).Aggregate((x, y) => x + "," + y);
        }

        private string GetKey(bool publicOnly)
        {
            if (publicOnly) return AllScopesPublic;
            return AllScopes;
        }
    }
}
