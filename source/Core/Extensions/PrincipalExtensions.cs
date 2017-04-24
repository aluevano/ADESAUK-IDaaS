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
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace IdentityServer.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="System.Security.Principal.IPrincipal"/> and <see cref="System.Security.Principal.IIdentity"/> .
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Gets the authentication time.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DateTimeOffset GetAuthenticationTime(this IPrincipal principal)
        {
            return principal.GetAuthenticationTimeEpoch().ToDateTimeOffsetFromEpoch();
        }

        /// <summary>
        /// Gets the authentication epoch time.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationTimeEpoch();
        }

        /// <summary>
        /// Gets the authentication epoch time.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.AuthenticationTime);

            if (claim == null) throw new InvalidOperationException("auth_time is missing.");
           
            return long.Parse(claim.Value);
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IPrincipal principal)
        {
            return principal.Identity.GetSubjectId();
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">sub claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.Subject);

            if (claim == null) throw new InvalidOperationException("sub claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetName(this IPrincipal principal)
        {
            return principal.Identity.GetName();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">name claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetName(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.Name);

            if (claim == null) throw new InvalidOperationException("name claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationMethod();
        }

        /// <summary>
        /// Gets the authentication method claims.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<Claim> GetAuthenticationMethods(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationMethods();
        }

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">amr claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.AuthenticationMethod);

            if (claim == null) throw new InvalidOperationException("amr claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the authentication method claims.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<Claim> GetAuthenticationMethods(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            return id.FindAll(Constants.ClaimTypes.AuthenticationMethod);
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IPrincipal principal)
        {
            return principal.Identity.GetIdentityProvider();
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">idp claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.IdentityProvider);

            if (claim == null) throw new InvalidOperationException("idp claim is missing");
            return claim.Value;
        }
    }
}