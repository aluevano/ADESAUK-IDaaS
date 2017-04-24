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
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Parses a Basic Authentication header
    /// </summary>
    public class BasicAuthenticationSecretParser : ISecretParser
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Creates the parser with a reference to identity server options
        /// </summary>
        /// <param name="options">IdentityServer options</param>
        public BasicAuthenticationSecretParser(IdentityServerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Tries to find a secret on the environment that can be used for authentication
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        public Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            Logger.Debug("Start parsing Basic Authentication secret");

            var notfound = Task.FromResult<ParsedSecret>(null);
            var context = new OwinContext(environment);
            var authorizationHeader = context.Request.Headers.Get("Authorization");

            if (authorizationHeader == null)
            {
                return notfound;
            }

            if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return notfound;
            }

            var parameter = authorizationHeader.Substring("Basic ".Length);

            string pair;
            try
            {
                pair = Encoding.UTF8.GetString(
                    Convert.FromBase64String(parameter));
            }
            catch (FormatException)
            {
                Logger.Debug("Malformed Basic Authentication credential.");
                return notfound;
            }
            catch (ArgumentException)
            {
                Logger.Debug("Malformed Basic Authentication credential.");
                return notfound;
            }

            var ix = pair.IndexOf(':');
            if (ix == -1)
            {
                Logger.Debug("Malformed Basic Authentication credential.");
                return notfound;
            }

            var clientId = pair.Substring(0, ix);
            var secret = pair.Substring(ix + 1);

            if (clientId.IsPresent() && secret.IsPresent())
            {
                if (clientId.Length > _options.InputLengthRestrictions.ClientId ||
                    secret.Length > _options.InputLengthRestrictions.ClientSecret)
                {
                    Logger.Error("Client ID or secret exceeds allowed length.");
                    return notfound;
                }

                var parsedSecret = new ParsedSecret
                {
                    Id = clientId,
                    Credential = secret,
                    Type = Constants.ParsedSecretTypes.SharedSecret
                };

                return Task.FromResult(parsedSecret);
            }

            Logger.Debug("No Basic Authentication secret found");
            return notfound;
        }
    }
}