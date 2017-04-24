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
using System.Linq;

namespace IdentityServer.Core.ViewModels
{
    /// <summary>
    /// Models the data submitted from the conset page.
    /// </summary>
    public class UserConsent
    {
        /// <summary>
        /// Gets or sets the button that was clicked (either "yes" or "no").
        /// </summary>
        /// <value>
        /// The button.
        /// </value>
        public string Button { get; set; }
        /// <summary>
        /// Gets or sets the scopes consented to.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wishes the consent to be remembered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent is to be remembered; otherwise, <c>false</c>.
        /// </value>
        public bool RememberConsent { get; set; }

        internal bool WasConsentGranted
        {
            get
            {
                return Button == "yes";
            }
        }

        internal IEnumerable<string> ScopedConsented
        {
            get
            {
                if (WasConsentGranted && Scopes != null)
                {
                    return Scopes;
                }

                return Enumerable.Empty<string>();
            }
        }
    }
}
