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
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Parses a POST body for secrets
    /// </summary>
    public class ClientAssertionSecretParser : ISecretParser
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Tries to find a JWT client assertion token on the environment that can be used for authentication
        /// Used for "private_key_jwt" client authentication method as defined in http://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        public async Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            Logger.Debug("Start parsing for JWT client assertion in post body");

            var context = new OwinContext(environment);
            var body = await context.ReadRequestFormAsync();

            if (body != null)
            {
                var clientId = body.Get(Constants.TokenRequest.ClientId);
                var clientAssertionType = body.Get(Constants.TokenRequest.ClientAssertionType);
                var clientAssertion = body.Get(Constants.TokenRequest.ClientAssertion);

                if (clientAssertion.IsPresent()
                    && clientAssertionType == Constants.ClientAssertionTypes.JwtBearer)
                {
                    if (!clientId.IsPresent())
                    {
                        // at least some clients (i.e. java com.nimbusds/oauth2-oidc-sdk) do not send client_id, but assume that token is enough (and it actually is)
                        clientId = GetClientIdFromToken(clientAssertion);
                        if (!clientId.IsPresent())
                        {
                            return null;
                        }
                    }
                    var parsedSecret = new ParsedSecret
                    {
                        Id = clientId,
                        Credential = clientAssertion,
                        Type = Constants.ParsedSecretTypes.JwtBearer
                    };

                    return parsedSecret;
                }
            }

            Logger.Debug("No JWT client assertion found in post body");
            return null;
        }

        private string GetClientIdFromToken(string token)
        {
            try
            {
                var jwt = new JwtSecurityToken(token);
                return jwt.Issuer;
            }
            catch (Exception e)
            {
                Logger.WarnException("Could not parse client assertion", e);
                return null;
            }
        }
    }
}
