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

namespace IdentityServer.Core.Models
{
    /// <summary>
    /// OpenID Connect scope types.
    /// </summary>
    public enum ScopeType
    {
        /// <summary>
        /// Scope representing identity data (e.g. profile or email)
        /// </summary>
        Identity = 0,

        /// <summary>
        /// Scope representing a resource (e.g. a web api)
        /// </summary>
        Resource = 1
    }

    /// <summary>
    /// OpenID Connect flows.
    /// </summary>
    public enum Flows
    {
        /// <summary>
        /// authorization code flow
        /// </summary>
        AuthorizationCode = 0,

        /// <summary>
        /// implicit flow
        /// </summary>
        Implicit = 1,

        /// <summary>
        /// hybrid flow
        /// </summary>
        Hybrid = 2,

        /// <summary>
        /// client credentials flow
        /// </summary>
        ClientCredentials = 3,

        /// <summary>
        /// resource owner password credential flow
        /// </summary>
        ResourceOwner = 4,

        /// <summary>
        /// custom grant
        /// </summary>
        Custom = 5,

        /// <summary>
        /// authorization code with proof key flow
        /// </summary>
        AuthorizationCodeWithProofKey = 6,

        /// <summary>
        /// hybrid flow with proof key
        /// </summary>
        HybridWithProofKey = 7
    }

    /// <summary>
    /// OpenID Connect subject types.
    /// </summary>
    public enum SubjectTypes
    {
        /// <summary>
        /// global - use the native subject id
        /// </summary>
        Global = 0,

        /// <summary>
        /// ppid - scope the subject id to the client
        /// </summary>
        Ppid = 1
    };

    /// <summary>
    /// Access token types.
    /// </summary>
    public enum AccessTokenType
    {
        /// <summary>
        /// Self-contained Json Web Token
        /// </summary>
        Jwt = 0,

        /// <summary>
        /// Reference token
        /// </summary>
        Reference = 1
    }

    /// <summary>
    /// Token usage types.
    /// </summary>
    public enum TokenUsage
    {
        /// <summary>
        /// Re-use the refresh token handle
        /// </summary>
        ReUse = 0,

        /// <summary>
        /// Issue a new refresh token handle every time
        /// </summary>
        OneTimeOnly = 1
    }

    /// <summary>
    /// Token expiration types.
    /// </summary>
    public enum TokenExpiration
    {
        /// <summary>
        /// Sliding token expiration
        /// </summary>
        Sliding = 0,

        /// <summary>
        /// Absolute token expiration
        /// </summary>
        Absolute = 1
    }
}