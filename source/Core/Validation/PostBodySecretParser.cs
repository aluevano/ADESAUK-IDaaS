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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Parses a POST body for secrets
    /// </summary>
    public class PostBodySecretParser : ISecretParser
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Creates the parser with options
        /// </summary>
        /// <param name="options">IdentityServer options</param>
        public PostBodySecretParser(IdentityServerOptions options)
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
        public async Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            Logger.Debug("Start parsing for secret in post body");

            var context = new OwinContext(environment);
            var body = await context.ReadRequestFormAsync();

            if (body != null)
            {
                var id = body.Get("client_id");
                var secret = body.Get("client_secret");

                if (id.IsPresent() && secret.IsPresent())
                {
                    if (id.Length > _options.InputLengthRestrictions.ClientId ||
                        secret.Length > _options.InputLengthRestrictions.ClientSecret)
                    {
                        Logger.Debug("Client ID or secret exceeds maximum lenght.");
                        return null;
                    }

                    var parsedSecret = new ParsedSecret
                    {
                        Id = id,
                        Credential = secret,
                        Type = Constants.ParsedSecretTypes.SharedSecret
                    };

                    return parsedSecret;
                }
            }

            Logger.Debug("No secret in post body found");
            return null;
        }
    }
}
