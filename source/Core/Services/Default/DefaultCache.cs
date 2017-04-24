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
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// In-memory, time based implementation of <see cref="ICache{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultCache<T> : ICache<T>
        where T : class
    {
        readonly MemoryCache cache;
        readonly TimeSpan duration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCache{T}"/> class.
        /// </summary>
        public DefaultCache()
            : this(Constants.DefaultCacheDuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCache{T}"/> class.
        /// </summary>
        /// <param name="duration">The duration to cache items.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">duration;Duration must be greater than zero</exception>
        public DefaultCache(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException("duration", "Duration must be greater than zero");

            this.cache = new MemoryCache("cache");
            this.duration = duration;
        }

        /// <summary>
        /// Gets the cached data based upon a key index.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The cached item, or <c>null</c> if no item matches the key.
        /// </returns>
        public Task<T> GetAsync(string key)
        {
            return Task.FromResult((T)cache.Get(key));
        }

        /// <summary>
        /// Caches the data based upon a key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public Task SetAsync(string key, T item)
        {
            var expiration = UtcNow.Add(this.duration);
            cache.Set(key, item, expiration);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets the UTC now.
        /// </summary>
        /// <value>
        /// The UTC now.
        /// </value>
        protected virtual DateTimeOffset UtcNow
        {
            get { return DateTimeOffsetHelper.UtcNow; }
        }
    }
}
