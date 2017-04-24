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
using IdentityServer.Core.Models;
using IdentityServer.Core.Validation;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.Core.Logging
{
    internal class AuthorizeRequestValidationLog
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string RedirectUri { get; set; }
        public IEnumerable<string> AllowedRedirectUris { get; set; }
        public string SubjectId { get; set; }

        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
        public Flows Flow { get; set; }
        public string RequestedScopes { get; set; }
        
        public string State { get; set; }
        public string UiLocales { get; set; }
        public string Nonce { get; set; }
        public IEnumerable<string> AuthenticationContextReferenceClasses { get; set; }
        public string DisplayMode { get; set; }
        public string PromptMode { get; set; }
        public int? MaxAge { get; set; }
        public string LoginHint { get; set; }
        public string SessionId { get; set; }
        public string CodeChallengeMethod { get; set; }

        public Dictionary<string, string> Raw { get; set; }

        public AuthorizeRequestValidationLog(ValidatedAuthorizeRequest request)
        {
            Raw = request.Raw.ToDictionary();

            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;

                AllowedRedirectUris = request.Client.RedirectUris;
            }

            if (request.Subject != null)
            {
                var subjectClaim = request.Subject.FindFirst(Constants.ClaimTypes.Subject);
                if (subjectClaim != null)
                {
                    SubjectId = subjectClaim.Value;
                }
                else
                {
                    SubjectId = "unknown";
                }
            }

            if (request.AuthenticationContextReferenceClasses.Any())
            {
                AuthenticationContextReferenceClasses = request.AuthenticationContextReferenceClasses;
            }
                
            RedirectUri = request.RedirectUri;
            ResponseType = request.ResponseType;
            ResponseMode = request.ResponseMode;
            Flow = request.Flow;
            RequestedScopes = request.RequestedScopes.ToSpaceSeparatedString();
            State = request.State;
            UiLocales = request.UiLocales;
            Nonce = request.Nonce;
            
            DisplayMode = request.DisplayMode;
            PromptMode = request.PromptMode;
            LoginHint = request.LoginHint;
            MaxAge = request.MaxAge;
            SessionId = request.SessionId;
            CodeChallengeMethod = request.CodeChallengeMethod;
        }
    }
}