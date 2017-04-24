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

using IdentityServer.Core.Configuration;
using IdentityServer.Core.Extensions;
using System;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Registration for the default view service.
    /// </summary>
    public class DefaultViewServiceRegistration : DefaultViewServiceRegistration<DefaultViewService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceRegistration"/> class.
        /// </summary>
        public DefaultViewServiceRegistration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceRegistration"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DefaultViewServiceRegistration(DefaultViewServiceOptions options)
            : base(options)
        {
        }
    }
    
    /// <summary>
    /// Registration for a customer view service derived from the DefaultViewService.
    /// </summary>
    public class DefaultViewServiceRegistration<T> : Registration<IViewService, T>
        where T : DefaultViewService
    {
        const string InnerRegistrationName = "DefaultViewServiceRegistration.inner";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceRegistration"/> class.
        /// </summary>
        public DefaultViewServiceRegistration()
            : this(new DefaultViewServiceOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceRegistration"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public DefaultViewServiceRegistration(DefaultViewServiceOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            AdditionalRegistrations.Add(new Registration<DefaultViewServiceOptions>(options));

            if (options.ViewLoader == null)
            {
                if (options.CustomViewDirectory.IsPresent())
                {
                    options.ViewLoader = new Registration<IViewLoader>(provider =>
                    {
                        return new FileSystemWithEmbeddedFallbackViewLoader(options.CustomViewDirectory);
                    });
                }
                else
                {
                    options.ViewLoader = new Registration<IViewLoader, FileSystemWithEmbeddedFallbackViewLoader>();
                }
            }

            if (options.CacheViews)
            {
                AdditionalRegistrations.Add(new Registration<IViewLoader>(options.ViewLoader, InnerRegistrationName));
                var cache = new ResourceCache();
                AdditionalRegistrations.Add(new Registration<IViewLoader>(
                    resolver => new CachingLoader(cache, resolver.Resolve<IViewLoader>(InnerRegistrationName))));
            }
            else
            {
                AdditionalRegistrations.Add(options.ViewLoader);
            }
        }
    }
}
