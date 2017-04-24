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
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using Microsoft.Owin;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Parses the environment for an X509 client certificate
    /// </summary>
    public class X509CertificateSecretParser : ISecretParser
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger(); 

        /// <summary>
        /// Tries to find a secret on the environment that can be used for authentication
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        public async Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            Logger.Debug("Start parsing for X.509 certificate");

            var context = new OwinContext(environment);
            var body = await context.ReadRequestFormAsync();

            if (body == null)
            {
                return null;
            }

            var id = body.Get("client_id");
            if (id.IsMissing())
            {
                Logger.Debug("client_id is not found in post body");
                return null;
            }

            var cert = context.Get<X509Certificate2>("ssl.ClientCertificate");

            if (cert != null)
            {
                return new ParsedSecret
                {
                    Id = id,
                    Credential = cert,
                    Type = Constants.ParsedSecretTypes.X509Certificate
                };
            }

            Logger.Debug("X.509 certificate not found.");
            return null;
        }
    }
}