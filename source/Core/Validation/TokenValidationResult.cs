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
using System.Security.Claims;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Modles the validation result of access tokens and id tokens.
    /// </summary>
    public class TokenValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IEnumerable<Claim> Claims { get; set; }
        
        /// <summary>
        /// Gets or sets the JWT.
        /// </summary>
        /// <value>
        /// The JWT.
        /// </value>
        public string Jwt { get; set; }
        
        /// <summary>
        /// Gets or sets the reference token.
        /// </summary>
        /// <value>
        /// The reference token.
        /// </value>
        public Token ReferenceToken { get; set; }
        
        /// <summary>
        /// Gets or sets the reference token identifier.
        /// </summary>
        /// <value>
        /// The reference token identifier.
        /// </value>
        public string ReferenceTokenId { get; set; }
        
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }
    }
}