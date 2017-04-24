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
using System.Collections.Generic;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Models a validated request to the token endpoint.
    /// </summary>
    public class ValidatedTokenRequest : ValidatedRequest
    {
        /// <summary>
        /// Initializes the validated request with default values.
        /// </summary>
        public ValidatedTokenRequest()
        {
            RequestedTokenType = RequestedTokenTypes.Bearer;
        }

        /// <summary>
        /// Gets or sets the requested token type.
        /// </summary>
        /// <value>
        /// The requested token type.
        /// </value>
        public RequestedTokenTypes RequestedTokenType { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the grant.
        /// </summary>
        /// <value>
        /// The type of the grant.
        /// </value>
        public string GrantType { get; set; }
        
        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the username used in the request.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        
        /// <summary>
        /// Gets or sets the sign in message.
        /// </summary>
        /// <value>
        /// The sign in message.
        /// </value>
        public SignInMessage SignInMessage { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        public RefreshToken RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the refresh token handle.
        /// </summary>
        /// <value>
        /// The refresh token handle.
        /// </value>
        public string RefreshTokenHandle { get; set; }

        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        /// <value>
        /// The authorization code.
        /// </value>
        public AuthorizationCode AuthorizationCode { get; set; }

        /// <summary>
        /// Gets or sets the authorization code handle.
        /// </summary>
        /// <value>
        /// The authorization code handle.
        /// </value>
        public string AuthorizationCodeHandle { get; set; }

        /// <summary>
        /// Gets or sets the code verifier.
        /// </summary>
        /// <value>
        /// The code verifier.
        /// </value>
        public string CodeVerifier { get; set; }

        /// <summary>
        /// Gets or sets the algorithm used for the proof key
        /// </summary>
        /// <value>
        /// The algorithm name.
        /// </value>
        public string ProofKeyAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the proof key
        /// </summary>
        /// <value>
        /// The proof key.
        /// </value>
        public string ProofKey { get; set; }
    }
}