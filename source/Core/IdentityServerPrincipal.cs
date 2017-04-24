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

using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.Core
{
    /// <summary>
    /// Helps creating valid identityserver principals (contain the required claims like sub, auth_time, amr, ...)
    /// </summary>
    public static class IdentityServerPrincipal
    {
        /// <summary>
        /// Creates an identityserver principal by specifying the required claims
        /// </summary>
        /// <param name="subject">Subject ID</param>
        /// <param name="displayName">Display name</param>
        /// <param name="authenticationMethod">Authentication method</param>
        /// <param name="idp">IdP name</param>
        /// <param name="authenticationType">Authentication type</param>
        /// <param name="authenticationTime">Authentication time</param>
        /// <returns>ClaimsPrincipal</returns>
        public static ClaimsPrincipal Create(
            string subject,
            string displayName,
            string authenticationMethod = Constants.AuthenticationMethods.Password,
            string idp = Constants.BuiltInIdentityProvider,
            string authenticationType = Constants.PrimaryAuthenticationType,
            long authenticationTime = 0)
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(displayName)) throw new ArgumentNullException("displayName");
            if (String.IsNullOrWhiteSpace(authenticationMethod)) throw new ArgumentNullException("authenticationMethod");
            if (String.IsNullOrWhiteSpace(idp)) throw new ArgumentNullException("idp");
            if (String.IsNullOrWhiteSpace(authenticationType)) throw new ArgumentNullException("authenticationType");

            if (authenticationTime <= 0) authenticationTime = DateTimeOffset.UtcNow.ToEpochTime();

            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, subject),
                new Claim(Constants.ClaimTypes.Name, displayName),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, authenticationMethod),
                new Claim(Constants.ClaimTypes.IdentityProvider, idp),
                new Claim(Constants.ClaimTypes.AuthenticationTime, authenticationTime.ToString(), ClaimValueTypes.Integer)
            };

            var id = new ClaimsIdentity(claims, authenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        /// <summary>
        /// Derives an identityserver principal from another principal
        /// </summary>
        /// <param name="principal">The other principal</param>
        /// <param name="authenticationType">Authentication type</param>
        /// <returns>ClaimsPrincipal</returns>
        public static ClaimsPrincipal CreateFromPrincipal(ClaimsPrincipal principal, string authenticationType)
        {
            // we require the following claims
            var subject = principal.FindFirst(Constants.ClaimTypes.Subject);
            if (subject == null) throw new InvalidOperationException("sub claim is missing");

            var name = principal.FindFirst(Constants.ClaimTypes.Name);
            if (name == null) throw new InvalidOperationException("name claim is missing");

            var authenticationMethod = principal.FindFirst(Constants.ClaimTypes.AuthenticationMethod);
            if (authenticationMethod == null) throw new InvalidOperationException("amr claim is missing");

            var authenticationTime = principal.FindFirst(Constants.ClaimTypes.AuthenticationTime);
            if (authenticationTime == null) throw new InvalidOperationException("auth_time claim is missing");

            var idp = principal.FindFirst(Constants.ClaimTypes.IdentityProvider);
            if (idp == null) throw new InvalidOperationException("idp claim is missing");

            var id = new ClaimsIdentity(principal.Claims, authenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        /// <summary>
        /// Creates a principal from the subject id and additional claims
        /// </summary>
        /// <param name="subjectId">Subject ID</param>
        /// <param name="additionalClaims">Additional claims</param>
        /// <returns>ClaimsPrincipal</returns>
        public static ClaimsPrincipal FromSubjectId(string subjectId, IEnumerable<Claim> additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, subjectId)
            };

            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            return Principal.Create(Constants.PrimaryAuthenticationType,
                claims.Distinct(new ClaimComparer()).ToArray());
        }

        /// <summary>
        /// Creates a principal from a list of claims
        /// </summary>
        /// <param name="claims">The claims</param>
        /// <param name="allowMissing">Specifies whether required claims must be present</param>
        /// <returns>ClaimsPrincipal</returns>
        public static ClaimsPrincipal FromClaims(IEnumerable<Claim> claims, bool allowMissing = false)
        {
            var sub = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
            var amr = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.AuthenticationMethod);
            var idp = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.IdentityProvider);
            var authTime = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.AuthenticationTime);

            var id = new ClaimsIdentity(Constants.BuiltInIdentityProvider);

            if (sub != null)
            {
                id.AddClaim(sub);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("sub claim is missing");
                }
            }

            if (amr != null)
            {
                id.AddClaim(amr);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("amr claim is missing");
                }
            }

            if (idp != null)
            {
                id.AddClaim(idp);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("idp claim is missing");
                }
            }

            if (authTime != null)
            {
                id.AddClaim(authTime);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("auth_time claim is missing");
                }
            }

            return new ClaimsPrincipal(id);
        }
    }
}