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
using IdentityServer.Core.Models;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default token signing service
    /// </summary>
    public class DefaultTokenSigningService : ITokenSigningService
    {
        /// <summary>
        /// The identity server options
        /// </summary>
        protected readonly IdentityServerOptions _options;

        /// <summary>
        /// The signing key service
        /// </summary>
        private readonly ISigningKeyService _keyService;

        // todo: remove in next major version
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenSigningService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DefaultTokenSigningService(IdentityServerOptions options)
        {
            _options = options;
            _keyService = new DefaultSigningKeyService(options);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenSigningService"/> class.
        /// </summary>
        /// <param name="keyService">The signing key service.</param>
        public DefaultTokenSigningService(ISigningKeyService keyService)
        {
            _keyService = keyService;
        }

        /// <summary>
        /// Signs the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// A protected and serialized security token
        /// </returns>
        public virtual async Task<string> SignTokenAsync(Token token)
        {
            var credentials = await GetSigningCredentialsAsync();
            return await CreateJsonWebToken(token, credentials);
        }

        /// <summary>
        /// Retrieves the signing credential (override to load key from alternative locations)
        /// </summary>
        /// <returns>The signing credential</returns>
        protected virtual async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            return new X509SigningCredentials(await _keyService.GetSigningKeyAsync());
        }

        /// <summary>
        /// Creates the json web token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>The signed JWT</returns>
        protected virtual async Task<string> CreateJsonWebToken(Token token, SigningCredentials credentials)
        {
            var payload = CreatePayload(token);
            return await SignAsync(payload, credentials);
        }

        /// <summary>
        /// Creates the JWT payload
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The JWT payload</returns>
        protected virtual string CreatePayload(Token token)
        {
            return token.CreateJwtPayload();   
        }

        /// <summary>
        /// Creates the JWT header
        /// </summary>
        /// <param name="credential">The credentials.</param>
        /// <returns>The JWT header</returns>
        private async Task<JwtHeader> CreateHeaderAsync(SigningCredentials credential)
        {
            var header = new JwtHeader(credential);

            var x509credential = credential as X509SigningCredentials;
            if (x509credential != null)
            {
                header.Add("kid", await _keyService.GetKidAsync(x509credential.Certificate));
            }

            return header;
        }

        private async Task<string> SignAsync(string payload, SigningCredentials credentials)
        {
            var header = await CreateHeaderAsync(credentials);
            var jwtPayload = JwtPayload.Deserialize(payload);

            var token = new JwtSecurityToken(header, jwtPayload);

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }
    }
}