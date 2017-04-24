using IdentityServer.Core.Logging;
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
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default CORS policy service.
    /// </summary>
    public class DefaultCorsPolicyService : ICorsPolicyService
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCorsPolicyService"/> class.
        /// </summary>
        public DefaultCorsPolicyService()
        {
            AllowedOrigins = new HashSet<string>();
        }

        /// <summary>
        /// The list allowed origins that are allowed.
        /// </summary>
        /// <value>
        /// The allowed origins.
        /// </value>
        public ICollection<string> AllowedOrigins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all origins are allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allow all; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAll { get; set; }

        /// <summary>
        /// Determines whether the origin allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            if (AllowAll)
            {
                Logger.InfoFormat("AllowAll true, so origin: {0} is allowed", origin);
                return Task.FromResult(true);
            }

            if (AllowedOrigins != null)
            {
                if (AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    Logger.InfoFormat("AllowedOrigins configured and origin {0} is allowed", origin);
                    return Task.FromResult(true);
                }
                else
                {
                    Logger.InfoFormat("AllowedOrigins configured and origin {0} is not allowed", origin);
                }
            }

            Logger.InfoFormat("Exiting; origin {0} is not allowed", origin);

            return Task.FromResult(false);
        }
    }
}
