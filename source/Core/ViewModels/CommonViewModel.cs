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

namespace IdentityServer.Core.ViewModels
{
    /// <summary>
    /// Models common data needed to render pages in IdentityServer.
    /// </summary>
    public class CommonViewModel
    {
        /// <summary>
        /// The site URL.
        /// </summary>
        /// <value>
        /// The site URL.
        /// </value>
        public string SiteUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the site.
        /// </summary>
        /// <value>
        /// The name of the site.
        /// </value>
        public string SiteName { get; set; }
        
        /// <summary>
        /// The current logged in display name.
        /// </summary>
        /// <value>
        /// The current user.
        /// </value>
        public string CurrentUser { get; set; }

        /// <summary>
        /// The URL to allow a user to logout.
        /// </summary>
        /// <value>
        /// The logout URL.
        /// </value>
        public string LogoutUrl { get; set; }

        /// <summary>
        /// Gets or sets the custom data for the model.
        /// </summary>
        /// <value>
        /// The custom data.
        /// </value>
        public object Custom { get; set; }
    }
}
