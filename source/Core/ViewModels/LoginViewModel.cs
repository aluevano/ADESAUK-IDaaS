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
using System.Collections.Generic;

namespace IdentityServer.Core.ViewModels
{
    /// <summary>
    /// Models that data needed to render the login page.
    /// </summary>
    public class LoginViewModel : ErrorViewModel
    {
        /// <summary>
        /// The URL to POST credentials to for local logins. Will be <c>null</c> if local login is disabled.
        /// <see cref="LoginCredentials"/> for the model for the submitted data.
        /// </summary>
        /// <value>
        /// The login URL.
        /// </value>
        public string LoginUrl { get; set; }

        /// <summary>
        /// The anti forgery values.
        /// </summary>
        /// <value>
        /// The anti forgery.
        /// </value>
        public AntiForgeryTokenViewModel AntiForgery { get; set; }

        /// <summary>
        /// Indicates if "remember me" has been disabled and should not be displayed to the user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow remember me]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowRememberMe { get; set; }

        /// <summary>
        /// The value to populate the "remember me" field.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remember me]; otherwise, <c>false</c>.
        /// </value>
        public bool RememberMe { get; set; }

        /// <summary>
        /// The value to populate the username field.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string Username { get; set; }

        /// <summary>
        /// List of external providers to display for home realm discover (HRD). 
        /// </summary>
        /// <value>
        /// The external providers.
        /// </value>
        public IEnumerable<LoginPageLink> ExternalProviders { get; set; }

        /// <summary>
        /// List of additional links configured to be displayed on the login page (e.g. as registration, or forgot password links).
        /// </summary>
        /// <value>
        /// The additional links.
        /// </value>
        public IEnumerable<LoginPageLink> AdditionalLinks { get; set; }

        /// <summary>
        /// The display name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }

        /// <summary>
        /// The URL for more information about the client.
        /// </summary>
        /// <value>
        /// The client URL.
        /// </value>
        public string ClientUrl { get; set; }

        /// <summary>
        /// The URL for the client's logo image.
        /// </summary>
        /// <value>
        /// The client logo URL.
        /// </value>
        public string ClientLogoUrl { get; set; }
    }
}
