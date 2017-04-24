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

using System.Threading.Tasks;
namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// <see cref="IViewLoader"/> decorator implementation that caches HTML templates in-memory.
    /// </summary>
    public class CachingLoader : IViewLoader
    {
        readonly ResourceCache cache;
        readonly IViewLoader inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingLoader" /> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="inner">The inner.</param>
        public CachingLoader(ResourceCache cache, IViewLoader inner)
        {
            this.cache = cache;
            this.inner = inner;
        }

        /// <summary>
        /// Loads the HTML for the named view.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public async Task<string> LoadAsync(string name)
        {
            var value = cache.Read(name);
            if (value == null)
            {
                value = await inner.LoadAsync(name);
                cache.Write(name, value);
            }
            return value;
        }
    }
}
