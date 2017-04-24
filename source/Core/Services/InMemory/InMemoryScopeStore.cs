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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.InMemory
{
    /// <summary>
    /// In-memory scope store
    /// </summary>
    public class InMemoryScopeStore : IScopeStore
    {
        readonly IEnumerable<Scope> _scopes;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryScopeStore"/> class.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        public InMemoryScopeStore(IEnumerable<Scope> scopes)
        {
            _scopes = scopes;
        }

        /// <summary>
        /// Gets all scopes.
        /// </summary>
        /// <returns>
        /// List of scopes
        /// </returns>
        public Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException("scopeNames");
            
            var scopes = from s in _scopes
                         where scopeNames.ToList().Contains(s.Name)
                         select s;

            return Task.FromResult<IEnumerable<Scope>>(scopes.ToList());
        }


        /// <summary>
        /// Gets all defined scopes.
        /// </summary>
        /// <param name="publicOnly">if set to <c>true</c> only public scopes are returned.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            if (publicOnly)
            {
                return Task.FromResult(_scopes.Where(s => s.ShowInDiscoveryDocument));
            }

            return Task.FromResult(_scopes);
        }
    }
}