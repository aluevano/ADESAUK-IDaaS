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

namespace IdentityServer.Core.Configuration
{
    /// <summary>
    /// Configures Content Security Policy (CSP) for HTML pages rendered by IdentityServer.
    /// </summary>
    public class CspOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CspOptions"/> class.
        /// </summary>
        public CspOptions()
        {
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether CSP is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Allows additional script sources to be indicated.
        /// </summary>
        /// <value>
        /// The script source.
        /// </value>
        public string ScriptSrc { get; set; }

        /// <summary>
        /// Allows additional style sources to be indicated.
        /// </summary>
        /// <value>
        /// The style source.
        /// </value>
        public string StyleSrc { get; set; }

        /// <summary>
        /// Allows additional font sources to be indicated.
        /// </summary>
        /// <value>
        /// The font source.
        /// </value>
        public string FontSrc { get; set; }

        /// <summary>
        /// Allows additional connect sources to be indicated.
        /// </summary>
        /// <value>
        /// The connect source.
        /// </value>
        public string ConnectSrc { get; set; }

        /// <summary>
        /// Allows additional image sources to be indicated.
        /// </summary>
        /// <value>
        /// The connect source.
        /// </value>
        public string ImgSrc { get; set; }
        
        /// <summary>
        /// Allows additional iframe sources to be indicated.
        /// </summary>
        /// <value>
        /// The connect source.
        /// </value>
        public string FrameSrc { get; set; }
    }
}
