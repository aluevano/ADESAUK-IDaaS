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
    /// Configures which endpoints are enabled or disabled.
    /// </summary>
    public class EndpointOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOptions"/> class.
        /// </summary>
        public EndpointOptions()
        {
            this.EnableAuthorizeEndpoint = true;
            this.EnableTokenEndpoint = true;
            this.EnableUserInfoEndpoint = true;
            this.EnableDiscoveryEndpoint = true;
            this.EnableAccessTokenValidationEndpoint = true;
            this.EnableIdentityTokenValidationEndpoint = true;
            this.EnableEndSessionEndpoint = true;
            this.EnableClientPermissionsEndpoint = true;
            this.EnableCspReportEndpoint = true;
            this.EnableCheckSessionEndpoint = true;
            this.EnableTokenRevocationEndpoint = true;
            this.EnableIntrospectionEndpoint = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the authorize endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the authorize endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableAuthorizeEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the token endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the token endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableTokenEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user info endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the user info endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableUserInfoEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the discovery document endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the disdovery document endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableDiscoveryEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the access token validation endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the access token validation endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableAccessTokenValidationEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the identity token validation endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the identity token validation endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableIdentityTokenValidationEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the end session endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the end session endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableEndSessionEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the client permissions endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the client permissions endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableClientPermissionsEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the CSP report endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the CSP report endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableCspReportEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the check session endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the check session endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableCheckSessionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token revocation endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the token revocation endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableTokenRevocationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the introspection endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the introspection endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableIntrospectionEndpoint { get; set; }
    }
}