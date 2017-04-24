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

namespace IdentityServer.Core.Configuration
{
    /// <summary>
    /// Configured how cookies are managed by IdentityServer.
    /// </summary>
    public class CookieOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CookieOptions"/> class.
        /// </summary>
        public CookieOptions()
        {
            ExpireTimeSpan = Constants.DefaultCookieTimeSpan;
            SlidingExpiration = false;
            AllowRememberMe = true;
            RememberMeDuration = Constants.DefaultRememberMeDuration;
            SecureMode = CookieSecureMode.SameAsRequest;
        }

        /// <summary>
        /// Allows setting a prefix on cookies to avoid potential conflicts with other cookies with the same names.
        /// </summary>
        /// <value>
        /// The prefix.
        /// </value>
        public string Prefix { get; set; }
        
        /// <summary>
        /// The expiration duration of the authentication cookie. Defaults to 10 hours.
        /// </summary>
        /// <value>
        /// The expire time span.
        /// </value>
        public TimeSpan ExpireTimeSpan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the authentication cookie is marked as persistent. Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if persistent; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersistent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the authentication cookie is sliding, which means it auto renews as the user is active. Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if sliding; otherwise, <c>false</c>.
        /// </value>
        public bool SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets the cookie path.
        /// </summary>
        /// <value>
        /// The cookie path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "remember me" option is presented to users on the login page. 
        /// If selected this option will issue a persistent authentication cookie. Defaults to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowRememberMe { get; set; }

        /// <summary>
        /// Gets or sets the duration of the persistent cookie issued by the "remember me" option on the login page.
        /// Defaults to 30 days.
        /// </summary>
        /// <value>
        /// The duration of the "remember me" persistent cookie.
        /// </value>
        public TimeSpan RememberMeDuration { get; set; }

        /// <summary>
        /// Gets or sets the mode for issuing the secure flag on the cookies issued. Defaults to SameAsRequest.
        /// </summary>
        /// <value>
        /// The secure.
        /// </value>
        public CookieSecureMode SecureMode { get; set; }

        /// <summary>
        /// An optional container in which to store the identity across requests. When used, only a session identifier is sent
        /// to the client. This can be used to mitigate potential problems with very large identities.
        /// </summary>
        public IAuthenticationSessionStoreProvider SessionStoreProvider { get; set; }



    }
}
