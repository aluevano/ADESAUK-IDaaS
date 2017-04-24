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

using System.Collections.Generic;

namespace IdentityServer.Core.ViewModels
{
    /// <summary>
    /// Models the data needed to render the consent page.
    /// </summary>
    public class ConsentViewModel : ErrorViewModel
    {
        /// <summary>
        /// The URL to allow a user to login as a different user.
        /// </summary>
        /// <value>
        /// The login with different account URL.
        /// </value>
        public string LoginWithDifferentAccountUrl { get; set; }

        /// <summary>
        /// The URL to POST the user's consent. <see cref="UserConsent"/> for the model for the submitted data.
        /// </summary>
        /// <value>
        /// The consent URL.
        /// </value>
        public string ConsentUrl { get; set; }

        /// <summary>
        /// The anti forgery values.
        /// </summary>
        /// <value>
        /// The anti forgery.
        /// </value>
        public AntiForgeryTokenViewModel AntiForgery { get; set; }

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

        /// <summary>
        /// Indicates if the "allow remember consent" is disabled and should not be displayed to the user.
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow remember consent]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowRememberConsent { get; set; }

        /// <summary>
        /// Value to populate the "remember my choice" checkbox.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remember consent]; otherwise, <c>false</c>.
        /// </value>
        public bool RememberConsent { get; set; }

        /// <summary>
        /// List of identity scopes being requested.
        /// </summary>
        /// <value>
        /// The identity scopes.
        /// </value>
        public IEnumerable<ConsentScopeViewModel> IdentityScopes { get; set; }

        /// <summary>
        /// List of resource scopes being requested.
        /// </summary>
        /// <value>
        /// The resource scopes.
        /// </value>
        public IEnumerable<ConsentScopeViewModel> ResourceScopes { get; set; }
    }
}
