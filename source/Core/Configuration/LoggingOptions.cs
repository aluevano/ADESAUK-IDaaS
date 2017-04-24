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
    /// Configures logging within IdentityServer.
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingOptions"/> class.
        /// </summary>
        public LoggingOptions()
        {
            EnableWebApiDiagnostics = false;
            WebApiDiagnosticsIsVerbose = false;
            EnableHttpLogging = false;
            EnableKatanaLogging = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether web API diagnostics should be enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if web API diagnostics should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableWebApiDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether web API diagnostics logging should be set to verbose.
        /// </summary>
        /// <value>
        /// <c>true</c> if web API diagnostics logging should be verbose; otherwise, <c>false</c>.
        /// </value>
        public bool WebApiDiagnosticsIsVerbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTTP request/response logging is enabled
        /// </summary>
        /// <value>
        ///   <c>true</c> if HTTP logging is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableHttpLogging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Katana logging should be forwarded to the standard logging output.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Katana log forwarding is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableKatanaLogging { get; set; }
    }
}
