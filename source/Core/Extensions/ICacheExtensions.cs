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

using IdentityServer.Core.Logging;
using IdentityServer.Core.Services;
using System;
using System.Threading.Tasks;

namespace IdentityServer.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IdentityServer.Core.Services.ICache{T}"/>
    /// </summary>
    public static class ICacheExtensions
    {
        private static readonly ILog Logger = LogProvider.GetLogger("Cache");

        /// <summary>
        /// Attempts to get an item from the cache. If the item is not found, the <c>get</c> function is used to 
        /// obtain the item and populate the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="get">The get function.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// cache
        /// or
        /// get
        /// </exception>
        public static async Task<T> GetAsync<T>(this ICache<T> cache, string key, Func<Task<T>> get)
            where T : class
        {
            if (cache == null) throw new ArgumentNullException("cache");
            if (get == null) throw new ArgumentNullException("get");
            if (key == null) return null;

            T item = await cache.GetAsync(key);
            
            if (item == null)
            {
                Logger.Debug("Cache miss: " + key);

                item = await get();

                if (item != null)
                {
                    await cache.SetAsync(key, item);
                }
            }
            else
            {
                Logger.Debug("Cache hit: " + key);
            }
            
            return item;
        }
    }
}