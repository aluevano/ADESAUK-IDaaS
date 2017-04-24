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
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.Caching;
using IdentityServer.Core.Services.Default;
using IdentityServer.Core.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer.Core.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="IdentityServer.Core.Configuration.IdentityServerServiceFactory"/>
    /// </summary>
    public static class IdentityServerServiceFactoryExtensions
    {
        const string CachingRegistrationName = "IdentityServerServiceFactoryExtensions.cache";
        const string InnerRegistrationName = "IdentityServerServiceFactoryExtensions.inner";

        /// <summary>
        /// Configures the provided cache in the dependency injection system as a decorator for the scope store.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="cacheRegistration">The cache registration.</param>
        /// <exception cref="System.ArgumentNullException">
        /// factory
        /// or
        /// cacheRegistration
        /// or
        /// ScopeStore needs to be configured on the factory
        /// </exception>
        public static void ConfigureScopeStoreCache(this IdentityServerServiceFactory factory,
            Registration<ICache<IEnumerable<Scope>>> cacheRegistration)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (cacheRegistration == null) throw new ArgumentNullException("cacheRegistration");
            if (factory.ScopeStore == null) throw new ArgumentNullException("ScopeStore needs to be configured on the factory");

            factory.Register(new Registration<ICache<IEnumerable<Scope>>>(cacheRegistration, CachingRegistrationName));
            factory.Register(new Registration<IScopeStore>(factory.ScopeStore, InnerRegistrationName));

            factory.ScopeStore = new Registration<IScopeStore>(resolver =>
            {
                var inner = resolver.Resolve<IScopeStore>(InnerRegistrationName);
                var cache = resolver.Resolve<ICache<IEnumerable<Scope>>>(CachingRegistrationName);
                return new CachingScopeStore(inner, cache);
            });
        }

        /// <summary>
        /// Configures an in-memory, time-based cache for the scope store.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="cacheDuration">Duration of the cache.</param>
        public static void ConfigureScopeStoreCache(this IdentityServerServiceFactory factory, TimeSpan cacheDuration)
        {
            var cache = new DefaultCache<IEnumerable<Scope>>(cacheDuration);
            var cacheRegistration = new Registration<ICache<IEnumerable<Scope>>>(cache);
            factory.ConfigureScopeStoreCache(cacheRegistration);
        }

        /// <summary>
        /// Configures the default cache for the scope store.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public static void ConfigureScopeStoreCache(this IdentityServerServiceFactory factory)
        {
            var cache = new DefaultCache<IEnumerable<Scope>>();
            var cacheRegistration = new Registration<ICache<IEnumerable<Scope>>>(cache);
            factory.ConfigureScopeStoreCache(cacheRegistration);
        }

        /// <summary>
        /// Configures the provided cache in the dependency injection system as a decorator for the client store.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="cacheRegistration">The cache registration.</param>
        /// <exception cref="System.ArgumentNullException">
        /// factory
        /// or
        /// cacheRegistration
        /// or
        /// ClientStore needs to be configured on the factory
        /// </exception>
        public static void ConfigureClientStoreCache(this IdentityServerServiceFactory factory,
            Registration<ICache<Client>> cacheRegistration)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (cacheRegistration == null) throw new ArgumentNullException("cacheRegistration");
            if (factory.ClientStore == null) throw new ArgumentNullException("ClientStore needs to be configured on the factory");

            factory.Register(new Registration<ICache<Client>>(cacheRegistration, CachingRegistrationName));
            factory.Register(new Registration<IClientStore>(factory.ClientStore, InnerRegistrationName));
            
            factory.ClientStore = new Registration<IClientStore>(resolver =>
            {
                var inner = resolver.Resolve<IClientStore>(InnerRegistrationName);
                var cache = resolver.Resolve<ICache<Client>>(CachingRegistrationName);
                return new CachingClientStore(inner, cache);
            });
        }

        /// <summary>
        /// Configures an in-memory, time-based cache for the client store.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="cacheDuration">Duration of the cache.</param>
        public static void ConfigureClientStoreCache(this IdentityServerServiceFactory factory, TimeSpan cacheDuration)
        {
            var cache = new DefaultCache<Client>(cacheDuration);
            var cacheRegistration = new Registration<ICache<Client>>(cache);
            factory.ConfigureClientStoreCache(cacheRegistration);
        }

        /// <summary>
        /// Configures the default cache for the client store.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public static void ConfigureClientStoreCache(this IdentityServerServiceFactory factory)
        {
            var cache = new DefaultCache<Client>();
            var cacheRegistration = new Registration<ICache<Client>>(cache);
            factory.ConfigureClientStoreCache(cacheRegistration);
        }

        /// <summary>
        /// Configures the provided cache in the dependency injection system as a decorator for the user service.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="cacheRegistration">The cache registration.</param>
        /// <exception cref="System.ArgumentNullException">
        /// factory
        /// or
        /// cacheRegistration
        /// or
        /// UserService needs to be configured on the factory
        /// </exception>
        public static void ConfigureUserServiceCache(this IdentityServerServiceFactory factory,
           Registration<ICache<IEnumerable<Claim>>> cacheRegistration)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (cacheRegistration == null) throw new ArgumentNullException("cacheRegistration");
            if (factory.UserService == null) throw new ArgumentNullException("UserService needs to be configured on the factory");

            factory.Register(new Registration<ICache<IEnumerable<Claim>>>(cacheRegistration, CachingRegistrationName));
            factory.Register(new Registration<IUserService>(factory.UserService, InnerRegistrationName));

            factory.UserService = new Registration<IUserService>(resolver =>
            {
                var inner = resolver.Resolve<IUserService>(InnerRegistrationName);
                var cache = resolver.Resolve<ICache<IEnumerable<Claim>>>(CachingRegistrationName);
                return new CachingUserService(inner, cache);
            });
        }

        /// <summary>
        /// Configures an in-memory, time-based cache for the user service store.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="cacheDuration">Duration of the cache.</param>
        public static void ConfigureUserServiceCache(this IdentityServerServiceFactory factory, TimeSpan cacheDuration)
        {
            var cache = new DefaultCache<IEnumerable<Claim>>(cacheDuration);
            var cacheRegistration = new Registration<ICache<IEnumerable<Claim>>>(cache);
            factory.ConfigureUserServiceCache(cacheRegistration);
        }

        /// <summary>
        /// Configures the default cache for the user service store.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public static void ConfigureUserServiceCache(this IdentityServerServiceFactory factory)
        {
            var cache = new DefaultCache<IEnumerable<Claim>>();
            var cacheRegistration = new Registration<ICache<IEnumerable<Claim>>>(cache);
            factory.ConfigureUserServiceCache(cacheRegistration);
        }

        /// <summary>
        /// Configures the default view service.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="options">The default view service options.</param>
        /// <exception cref="System.ArgumentNullException">
        /// factory
        /// or
        /// options
        /// </exception>
        /// <exception cref="System.InvalidOperationException">ViewService is already configured</exception>
        public static void ConfigureDefaultViewService(this IdentityServerServiceFactory factory, 
            DefaultViewServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");
            
            if (factory.ViewService != null) throw new InvalidOperationException("A ViewService is already configured");

            factory.ViewService = new DefaultViewServiceRegistration(options);
        }

        /// <summary>
        /// Configures a derived default view service.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="options">The default view service options.</param>
        /// <exception cref="System.ArgumentNullException">
        /// factory
        /// or
        /// options
        /// </exception>
        /// <exception cref="System.InvalidOperationException">ViewService is already configured</exception>
        public static void ConfigureDefaultViewService<T>(this IdentityServerServiceFactory factory,
            DefaultViewServiceOptions options)
            where T : DefaultViewService
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            if (factory.ViewService != null) throw new InvalidOperationException("A ViewService is already configured");

            factory.ViewService = new DefaultViewServiceRegistration<T>(options);
        }

        /// <summary>
        /// Configures the factory to use in-memory users.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        public static IdentityServerServiceFactory UseInMemoryUsers(this IdentityServerServiceFactory factory, List<InMemoryUser> users)
        {
            factory.Register(new Registration<List<InMemoryUser>>(users));
            factory.UserService = new Registration<IUserService, InMemoryUserService>();

            return factory;
        }

        /// <summary>
        /// Configures the factory to use in-memory clients.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="clients">The clients.</param>
        /// <returns></returns>
        public static IdentityServerServiceFactory UseInMemoryClients(this IdentityServerServiceFactory factory, IEnumerable<Client> clients)
        {
            factory.Register(new Registration<IEnumerable<Client>>(clients));
            factory.ClientStore = new Registration<IClientStore>(typeof(InMemoryClientStore));
            factory.CorsPolicyService = new Registration<ICorsPolicyService>(new InMemoryCorsPolicyService(clients));

            return factory;
        }

        /// <summary>
        /// Configures the factory to use in-memory scopes.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        public static IdentityServerServiceFactory UseInMemoryScopes(this IdentityServerServiceFactory factory, IEnumerable<Scope> scopes)
        {
            factory.Register(new Registration<IEnumerable<Scope>>(scopes));
            factory.ScopeStore = new Registration<IScopeStore>(typeof(InMemoryScopeStore));

            return factory;
        }
    }
}