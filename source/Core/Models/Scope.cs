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

namespace IdentityServer.Core.Models
{
    /// <summary>
    /// Models are resource (either identity resource or web api resource)
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// Indicates if scope is enabled and can be requested. Defaults to true.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Name of the scope. This is the value a client will use to request the scope.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Display name. This value will be used e.g. on the consent screen.
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Description. This value will be used e.g. on the consent screen.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
        /// </summary>
        public bool Emphasize { get; set; }

        /// <summary>
        /// Specifies whether this scope is about identity information from the userinfo endpoint, or a resource (e.g. a Web API). Defaults to Resource.
        /// </summary>
        public ScopeType Type { get; set; }
        
        /// <summary>
        /// List of user claims that should be included in the identity (identity scope) or access token (resource scope). 
        /// </summary>
        public List<ScopeClaim> Claims { get; set; }
        
        /// <summary>
        /// If enabled, all claims for the user will be included in the token. Defaults to false.
        /// </summary>
        public bool IncludeAllClaimsForUser { get; set; }
        
        /// <summary>
        /// Rule for determining which claims should be included in the token (this is implementation specific)
        /// </summary>
        public string ClaimsRule { get; set; }

        /// <summary>
        /// Specifies whether this scope is shown in the discovery document. Defaults to true.
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; }

        /// <summary>
        /// Gets or sets the scope secrets.
        /// </summary>
        /// <value>
        /// The scope secrets.
        /// </value>
        public List<Secret> ScopeSecrets { get; set; }

        /// <summary>
        /// Specifies whether this scope is allowed to see other scopes when using the introspection endpoint
        /// </summary>
        public bool AllowUnrestrictedIntrospection { get; set; }

        /// <summary>
        /// Creates a Scope with default values
        /// </summary>
        public Scope()
        {
            Type = ScopeType.Resource;
            Claims = new List<ScopeClaim>();
            ScopeSecrets = new List<Secret>();
            IncludeAllClaimsForUser = false;
            Enabled = true;
            ShowInDiscoveryDocument = true;
            AllowUnrestrictedIntrospection = false;
        }
    }
}