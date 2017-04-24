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

namespace IdentityServer.Core.Configuration
{
    /// <summary>
    /// Defines the input length restrictions for different values
    /// </summary>
    public class InputLengthRestrictions
    {
        private const int Default = 100;

        /// <summary>
        /// Creates InputLengthRestrictions with default values
        /// </summary>
        public InputLengthRestrictions()
        {
            ClientId = Default;
            ClientSecret = Default;
            Scope = 300;
            RedirectUri = 400;
            Nonce = 300;
            UiLocale = Default;
            LoginHint = Default;
            AcrValues = 300;
            GrantType = Default;
            UserName = Default;
            Password = Default;
            CspReport = 2000;
            IdentityProvider = Default;
            ExternalError = Default;
            AuthorizationCode = Default;
            RefreshToken = Default;
            TokenHandle = Default;
            Jwt = 51200;
            CodeChallengeMinLength = 43;
            CodeChallengeMaxLength = 128;
            CodeVerifierMinLength = 43;
            CodeVerifierMaxLength = 128;
            ProofKey = 2048;
        }

        /// <summary>
        /// Max length for client_id
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Max length for external client secrets
        /// </summary>
        public int ClientSecret { get; private set; }

        /// <summary>
        /// Max length for scope
        /// </summary>
        public int Scope { get; set; }

        /// <summary>
        /// Max length for redirect_uri
        /// </summary>
        public int RedirectUri { get; set; }

        /// <summary>
        /// Max length for nonce
        /// </summary>
        public int Nonce { get; set; }

        /// <summary>
        /// Max length for ui_locale
        /// </summary>
        public int UiLocale { get; set; }

        /// <summary>
        /// Max length for login_hint
        /// </summary>
        public int LoginHint { get; set; }

        /// <summary>
        /// Max length for acr_values
        /// </summary>
        public int AcrValues { get; set; }

        /// <summary>
        /// Max length for grant_type
        /// </summary>
        public int GrantType { get; set; }

        /// <summary>
        /// Max length for username
        /// </summary>
        public int UserName { get; set; }

        /// <summary>
        /// Max length for password
        /// </summary>
        public int Password { get; set; }

        /// <summary>
        /// Max length for CSP reports
        /// </summary>
        public int CspReport { get; set; }

        /// <summary>
        /// Max length for external identity provider name
        /// </summary>
        public int IdentityProvider { get; set; }

        /// <summary>
        /// Max length for external identity provider errors
        /// </summary>
        public int ExternalError { get; private set; }

        /// <summary>
        /// Max length for authorization codes
        /// </summary>
        public int AuthorizationCode { get; private set; }

        /// <summary>
        /// Max length for refresh tokens
        /// </summary>
        public int RefreshToken { get; private set; }

        /// <summary>
        /// Max length for token handles
        /// </summary>
        public int TokenHandle { get; private set; }

        /// <summary>
        /// Max length for JWTs
        /// </summary>
        public int Jwt { get; private set; }

        /// <summary>
        /// Min length for the code challenge
        /// </summary>
        public int CodeChallengeMinLength { get; private set; }

        /// <summary>
        /// Max length for the code challenge
        /// </summary>
        public int CodeChallengeMaxLength { get; private set; }

        /// <summary>
        /// Min length for the code verifier
        /// </summary>
        public int CodeVerifierMinLength { get; private set; }

        /// <summary>
        /// Max length for the code verifier
        /// </summary>
        public int CodeVerifierMaxLength { get; private set; }

        /// <summary>
        /// Max length for proof keys
        /// </summary>
        public int ProofKey { get; set; }
    }
}