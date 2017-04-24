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
using System.Security.Claims;

namespace IdentityServer.Core.Models
{
    /// <summary>
    /// Class describing the profile data request
    /// </summary>
    public class ProfileDataRequestContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileDataRequestContext"/> class.
        /// </summary>
        public ProfileDataRequestContext()
        {
            IssuedClaims = Enumerable.Empty<Claim>();
        }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all claims are requested.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all claims are requested; otherwise, <c>false</c>.
        /// </value>
        public bool AllClaimsRequested { get; set; }

        /// <summary>
        /// Gets or sets the requested claim types.
        /// </summary>
        /// <value>
        /// The requested claim types.
        /// </value>
        public IEnumerable<string> RequestedClaimTypes { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        /// <value>
        /// The client id.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets the caller.
        /// </summary>
        /// <value>
        /// The caller.
        /// </value>
        public string Caller { get; set; }

        /// <summary>
        /// Gets or sets the issued claims.
        /// </summary>
        /// <value>
        /// The issued claims.
        /// </value>
        public IEnumerable<Claim> IssuedClaims { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileDataRequestContext" /> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="requestedClaimTypes">The requested claim types.</param>
        public ProfileDataRequestContext(ClaimsPrincipal subject, Client client, string caller, IEnumerable<string> requestedClaimTypes = null)
        {
            Subject = subject;
            Client = client;
            Caller = caller;

            if (requestedClaimTypes == null)
            {
                AllClaimsRequested = true;
            }
            else
            {
                RequestedClaimTypes = requestedClaimTypes;
            }

            IssuedClaims = Enumerable.Empty<Claim>();
        }
    }
}