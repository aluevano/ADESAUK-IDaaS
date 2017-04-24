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
using IdentityServer.Core.Configuration.Hosting;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.Default;
using IdentityServer.Core.Services.InMemory;
using IdentityServer.Core.Validation;
using Microsoft.Owin;
using Moq;
using System.Collections.Generic;

namespace IdentityServer.Tests.Validation
{
    static class Factory
    {
        public static IClientStore CreateClientStore()
        {
            return new InMemoryClientStore(TestClients.Get());
        }

        public static DefaultTokenSigningService CreateDefaultTokenSigningService()
        {
            return new DefaultTokenSigningService(new DefaultSigningKeyService(TestIdentityServerOptions.Create()));
        }

        //public static ClientValidator CreateClientValidator(
        //    IClientStore clients = null,
        //    IClientSecretValidator secretValidator = null)
        //{
        //    if (clients == null)
        //    {
        //        clients = new InMemoryClientStore(ClientValidationTestClients.Get());
        //    }

        //    if (secretValidator == null)
        //    {
        //        secretValidator = new HashedClientSecretValidator();
        //    }

        //    var owin = new OwinEnvironmentService(new OwinContext());

        //    return new ClientValidator(clients, secretValidator, owin);
        //}

        public static TokenRequestValidator CreateTokenRequestValidator(
            IdentityServerOptions options = null,
            IScopeStore scopes = null,
            IAuthorizationCodeStore authorizationCodeStore = null,
            IRefreshTokenStore refreshTokens = null,
            IUserService userService = null,
            IEnumerable<ICustomGrantValidator> customGrantValidators = null,
            ICustomRequestValidator customRequestValidator = null,
            ScopeValidator scopeValidator = null)
        {
            if (options == null)
            {
                options = TestIdentityServerOptions.Create();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeStore(TestScopes.Get());
            }

            if (userService == null)
            {
                userService = new TestUserService();
            }

            if (customRequestValidator == null)
            {
                customRequestValidator = new DefaultCustomRequestValidator();
            }

            CustomGrantValidator aggregateCustomValidator;
            if (customGrantValidators == null)
            {
                aggregateCustomValidator = new CustomGrantValidator(new [] { new TestGrantValidator() });
            }
            else
            {
                aggregateCustomValidator = new CustomGrantValidator(customGrantValidators);
            }
                
            if (refreshTokens == null)
            {
                refreshTokens = new InMemoryRefreshTokenStore();
            }

            if (scopeValidator == null)
            {
                scopeValidator = new ScopeValidator(scopes);
            }

            return new TokenRequestValidator(
                options, 
                authorizationCodeStore, 
                refreshTokens, 
                userService, 
                aggregateCustomValidator, 
                customRequestValidator, 
                scopeValidator, 
                new DefaultEventService());
        }

        public static AuthorizeRequestValidator CreateAuthorizeRequestValidator(
            IdentityServerOptions options = null,
            IScopeStore scopes = null,
            IClientStore clients = null,
            IUserService users = null,
            ICustomRequestValidator customValidator = null,
            IRedirectUriValidator uriValidator = null,
            ScopeValidator scopeValidator = null,
            IDictionary<string, object> environment = null)
        {
            if (options == null)
            {
                options = TestIdentityServerOptions.Create();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeStore(TestScopes.Get());
            }

            if (clients == null)
            {
                clients = new InMemoryClientStore(TestClients.Get());
            }

            if (customValidator == null)
            {
                customValidator = new DefaultCustomRequestValidator();
            }

            if (uriValidator == null)
            {
                uriValidator = new DefaultRedirectUriValidator();
            }

            if (scopeValidator == null)
            {
                scopeValidator = new ScopeValidator(scopes);
            }

            var mockSessionCookie = new Mock<SessionCookie>((IOwinContext)null, (IdentityServerOptions)null);
            mockSessionCookie.CallBase = false;
            mockSessionCookie.Setup(x => x.GetSessionId()).Returns((string)null);

            return new AuthorizeRequestValidator(options, clients, customValidator, uriValidator, scopeValidator, mockSessionCookie.Object);

        }

        public static TokenValidator CreateTokenValidator(ITokenHandleStore tokenStore = null, IUserService users = null)
        {
            if (users == null)
            {
                users = new TestUserService();
            }

            var clients = CreateClientStore();
            var options = TestIdentityServerOptions.Create();
            options.Factory = new IdentityServerServiceFactory();
            var context = CreateOwinContext(options, clients, users);

            var validator = new TokenValidator(
                options: options,
                clients: clients,
                tokenHandles: tokenStore,
                customValidator: new DefaultCustomTokenValidator(
                    users: users,
                    clients: clients),
                owinEnvironment: new OwinEnvironmentService(context),
                keyService: new DefaultSigningKeyService(options));

            return validator;
        }

        public static IOwinContext CreateOwinContext(IdentityServerOptions options, IClientStore clients, IUserService users)
        {
            options.Factory = options.Factory ?? new IdentityServerServiceFactory();
            if (users != null)
            {
                options.Factory.UserService = new Registration<IUserService>(users);
            }
            if (options.Factory.UserService == null)
            {
                options.Factory.UseInMemoryUsers(new List<InMemoryUser>());
            }

            if (clients != null)
            {
                options.Factory.ClientStore = new Registration<IClientStore>(clients);
            }
            if (options.Factory.ClientStore == null)
            {
                options.Factory.UseInMemoryClients(new List<Client>());
            }

            if (options.Factory.ScopeStore == null)
            {
                options.Factory.UseInMemoryScopes(new List<Scope>());
            }

            var container = AutofacConfig.Configure(options);

            var context = new OwinContext();
            context.Set(Autofac.Integration.Owin.Constants.OwinLifetimeScopeKey, container);

            return context;
        }
    }
}