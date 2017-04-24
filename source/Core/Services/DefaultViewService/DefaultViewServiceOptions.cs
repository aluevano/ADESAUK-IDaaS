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

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Configures the assets for the default view service.
    /// </summary>
    public class DefaultViewServiceOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceOptions"/> class.
        /// </summary>
        public DefaultViewServiceOptions()
        {
            // adding default CSS here so hosting application can choose to remove it
            Stylesheets = new List<string>
            {
                "~/assets/styles.min.css"
            };
            
            Scripts = new List<string>();
            CacheViews = true;
        }

        /// <summary>
        /// Stylesheets to be rendered into the layout.
        /// </summary>
        /// <value>
        /// The stylesheets.
        /// </value>
        public IList<string> Stylesheets { get; set; }
        
        /// <summary>
        /// Scripts to be rendered into the layout.
        /// </summary>
        /// <value>
        /// The scripts.
        /// </value>
        public IList<string> Scripts { get; set; }

        /// <summary>
        /// Gets or sets the registration for the view loader.
        /// </summary>
        /// <value>
        /// The view loader.
        /// </value>
        public Registration<IViewLoader> ViewLoader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTML will be cached by the default view cache.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cache views]; otherwise, <c>false</c>.
        /// </value>
        public bool CacheViews { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the path to the filesystem directory that contain custom views. 
        /// This value is only used when the ViewLoader property has not been explicitly set.
        /// </summary>
        /// <value>
        ///  The filesystem path to the views directory.
        /// </value>
        public string CustomViewDirectory { get; set; }
    }
}
