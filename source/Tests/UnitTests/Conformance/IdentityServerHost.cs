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
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using IdentityServer.Core.Services.InMemory;
using Microsoft.Owin.Testing;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer.Tests.Conformance
{
    public class IdentityServerHost : IDisposable
    {
        public string Url = "https://idsvr.test/";

        public List<Scope> Scopes = new List<Scope>();
        public List<Client> Clients = new List<Client>();
        public List<InMemoryUser> Users = new List<InMemoryUser>();

        public IdentityServerOptions Options;

        public TestServer Server;
        public HttpClient Client;
        
        public DateTimeOffset Now = DateTimeOffset.MinValue;

        public IdentityServerHost()
        {
            var clientStore = new InMemoryClientStore(Clients);
            var scopeStore = new InMemoryScopeStore(Scopes);
            var userService = new InMemoryUserService(Users);

            var factory = new IdentityServerServiceFactory
            {
                ScopeStore = new Registration<IScopeStore>(scopeStore),
                ClientStore = new Registration<IClientStore>(clientStore),
                UserService = new Registration<IUserService>(userService),
            };

            Options = new IdentityServerOptions
            {
                Factory = factory,
                DataProtector = new NoDataProtector(),
                SiteName = "IdentityServer Host",
                SigningCertificate = SigningCertificate
            };
        }

        public void Dispose()
        {
            Server.Dispose();
            DateTimeOffsetHelper.UtcNowFunc = () => DateTimeOffset.UtcNow;
        }

        public void Init()
        {
            DateTimeOffsetHelper.UtcNowFunc = () => UtcNow;
            
            Server = TestServer.Create(app =>
            {
                app.UseIdentityServer(Options);
            });
            
            NewRequest();
        }

        public HttpClient NewRequest()
        {
            return Client = Server.HttpClient;
        }

        public DateTimeOffset UtcNow
        {
            get
            {
                if (Now > DateTimeOffset.MinValue) return Now;
                return DateTimeOffset.UtcNow;
            }
        }

        X509Certificate2 SigningCertificate
        {
            get
            {
                var assembly = typeof(IdentityServerHost).Assembly;
                using (var stream = assembly.GetManifestResourceStream("IdentityServer.Tests.idsrv3test.pfx"))
                {
                    return new X509Certificate2(ReadStream(stream), "idsrv3test");
                }
            }
        }

        private static byte[] ReadStream(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
