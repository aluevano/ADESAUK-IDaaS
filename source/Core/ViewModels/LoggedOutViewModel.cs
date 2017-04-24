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
    /// Models the data needed to render the logged out page.
    /// </summary>
    public class LoggedOutViewModel : CommonViewModel
    {
        /// <summary>
        /// A list of URLs that must be displayed in hidden iframes in the rendered page. These are
        /// needed to trigger logout of other endpoints.
        /// </summary>
        /// <value>
        /// The iframe urls.
        /// </value>
        public IEnumerable<string> IFrameUrls { get; set; }

        /// <summary>
        /// The name of the client that requested the logout.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }
        
        /// <summary>
        /// The URL to allow the user to return the the <see cref="ClientName"/>.
        /// </summary>
        /// <value>
        /// The redirect URL.
        /// </value>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic redirect to the redirect URL is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if automatic redirect is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRedirect { get; set; }
        
        /// <summary>
        /// Gets or sets the automatic redirect delay (in seconds).
        /// </summary>
        /// <value>
        /// The automatic redirect delay.
        /// </value>
        public int AutoRedirectDelay { get; set; }
    }
}
