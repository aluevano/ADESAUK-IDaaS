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

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer.Core.Models
{
    /// <summary>
    /// Represents the information needed to issue a login cookie.
    /// </summary>
    public class AuthenticatedLogin
    {
        /// <summary>
        /// The subject claim used to uniquely identifier the user.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// The name claim used as the display name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Claims that will be maintained in the login.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IEnumerable<Claim> Claims { get; set; }

        /// <summary>
        /// The authentication method. This should be used when 
        /// local authentication is performed as some other means other than password has been 
        /// used to authenticate the user (e.g. '2fa' for two-factor, or 'certificate' for client 
        /// certificates).
        /// </summary>
        /// <value>
        /// The authentication method.
        /// </value>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// The identity provider. This should used when an external
        /// identity provider is used and will typically match the <c>AuthenticationType</c> as configured
        /// on the Katana authentication middleware.
        /// </summary>
        /// <value>
        /// The identity provider.
        /// </value>
        public string IdentityProvider { get; set; }

        /// <summary>
        /// Gets or sets if the cookie should be persistent.
        /// </summary>
        /// <value>
        /// The persistent login.
        /// </value>
        public bool? PersistentLogin { get; set; }

        /// <summary>
        /// Gets or sets the expiration for the persistent cookie.
        /// </summary>
        /// <value>
        /// The persistent login expiration.
        /// </value>
        public DateTimeOffset? PersistentLoginExpiration { get; set; }
    }
}
