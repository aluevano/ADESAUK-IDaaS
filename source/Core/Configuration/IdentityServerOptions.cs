﻿/*
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

using IdentityServer.Core.Logging;
using Owin;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer.Core.Configuration
{
    /// <summary>
    /// The IdentityServerOptions class is the top level container for all configuration settings of IdentityServer.
    /// </summary>
    public class IdentityServerOptions
    {
        static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerOptions"/> class with default values.
        /// </summary>
        public IdentityServerOptions()
        {
            SiteName = Constants.IdentityServerName;

            this.ProtocolLogoutUrls = new List<string>();
            this.RequireSsl = true;
            this.Endpoints = new EndpointOptions();
            this.AuthenticationOptions = new AuthenticationOptions();
            this.CspOptions = new CspOptions();
            this.LoggingOptions = new LoggingOptions();
            this.EventsOptions = new EventsOptions();
            this.EnableWelcomePage = true;
            this.InputLengthRestrictions = new InputLengthRestrictions();
            this.DiscoveryOptions = new DiscoveryOptions();
        }

        internal void Validate()
        {            
            if (AuthenticationOptions == null)
            {
                throw new ArgumentException("AuthenticationOptions is missing");
            }
            if (CspOptions == null)
            {
                throw new ArgumentException("CspOptions is missing");
            }
            if (Endpoints == null)
            {
                throw new ArgumentException("Endpoints is missing");
            }
        }

        /// <summary>
        /// Gets or sets the display name of the site used in standard views.
        /// </summary>
        /// <value>
        /// Display name of the site used in standard views.
        /// </value>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the unique name of this server instance, e.g. https://myissuer.com
        /// </summary>
        /// <value>
        /// Unique name of this server instance, e.g. https://myissuer.com
        /// </value>
        public string IssuerUri { get; set; }

        // todo: remove in 3.0.0
        // added as a temporary measure since we need someplace to hold the calculated 
        // IssuerUri from the first request for the scenarios where the newer GetIdentityServerIssuerUri
        // extension method is not being used
        internal string DynamicallyCalculatedIssuerUri { get; set; }

        /// <summary>
        /// Gets or sets the X.509 certificate (and corresponding private key) for signing security tokens.
        /// </summary>
        /// <value>
        /// The signing certificate.
        /// </value>
        public X509Certificate2 SigningCertificate { get; set; }

        /// <summary>
        /// Gets or sets a secondary certificate that will appear in the discovery document. Can be used to prepare clients for certificate rollover
        /// </summary>
        /// <value>
        /// The secondary signing certificate.
        /// </value>
        public X509Certificate2 SecondarySigningCertificate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is required for IdentityServer. Defaults to `true`.
        /// </summary>
        /// <value>
        ///   <c>true</c> if SSL is required; otherwise, <c>false</c>.
        /// </value>
        public bool RequireSsl { get; set; }

        /// <summary>
        /// Gets or sets the public origin for IdentityServer (e.g. "https://yourserver:1234").
        /// </summary>
        /// <value>
        /// The name of the public origin.
        /// </value>
        public string PublicOrigin { get; set; }

        /// <summary>
        /// Gets or sets the identity server factory.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        public IdentityServerServiceFactory Factory { get; set; }

        /// <summary>
        /// Gets or sets the data protector.
        /// </summary>
        /// <value>
        /// The data protector.
        /// </value>
        public IDataProtector DataProtector { get; set; }

        /// <summary>
        /// Gets or sets the endpoint configuration.
        /// </summary>
        /// <value>
        /// The endpoints configuration.
        /// </value>
        public EndpointOptions Endpoints { get; set; }

        /// <summary>
        /// Gets or sets the discovery endpoint configuration.
        /// </summary>
        /// <value>
        /// The discovery endpoint configuration.
        /// </value>
        public DiscoveryOptions DiscoveryOptions { get; set; }

        /// <summary>
        /// Gets or sets the authentication options.
        /// </summary>
        /// <value>
        /// The authentication options.
        /// </value>
        public AuthenticationOptions AuthenticationOptions { get; set; }

        /// <summary>
        /// Gets or sets the plugin configuration.
        /// </summary>
        /// <value>
        /// The plugin configuration.
        /// </value>
        public Action<IAppBuilder, IdentityServerOptions> PluginConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the protocol logout urls.
        /// </summary>
        /// <value>
        /// The protocol logout urls.
        /// </value>
        public List<string> ProtocolLogoutUrls { get; set; }

        /// <summary>
        /// Gets or sets the CSP options.
        /// </summary>
        /// <value>
        /// The CSP options.
        /// </value>
        public CspOptions CspOptions { get; set; }

        /// <summary>
        /// Gets or sets the diagnostics options.
        /// </summary>
        /// <value>
        /// The diagnostics options.
        /// </value>
        public LoggingOptions LoggingOptions { get; set; }

        /// <summary>
        /// Gets or sets the events options.
        /// </summary>
        /// <value>
        /// The events options.
        /// </value>
        public EventsOptions EventsOptions { get; set; }

        /// <summary>
        /// Gets or sets the max input length restrictions.
        /// </summary>
        /// <value>
        /// The length restrictions.
        /// </value>
        public InputLengthRestrictions InputLengthRestrictions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the welcome page is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the welcome page is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableWelcomePage { get; set; }
        
        internal IEnumerable<X509Certificate2> PublicKeysForMetadata
        {
            get
            {
                var keys = new List<X509Certificate2>();
                
                if (SigningCertificate != null)
                {
                    keys.Add(SigningCertificate);
                }

                if (SecondarySigningCertificate != null)
                {
                    keys.Add(SecondarySigningCertificate);
                }

                return keys;
            }
        }
    }
}