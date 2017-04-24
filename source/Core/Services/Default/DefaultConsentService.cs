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

using IdentityServer.Core.Extensions;
using IdentityServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default consent service
    /// </summary>
    public class DefaultConsentService : IConsentService
    {
        /// <summary>
        /// The consent store
        /// </summary>
        protected readonly IConsentStore _store;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConsentService"/> class.
        /// </summary>
        /// <param name="store">The consent store.</param>
        /// <exception cref="System.ArgumentNullException">store</exception>
        public DefaultConsentService(IConsentStore store)
        {
            if (store == null) throw new ArgumentNullException("store");

            this._store = store;
        }

        /// <summary>
        /// Checks if consent is required.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subject">The user.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns>Boolean if consent is required.</returns>
        public virtual async Task<bool> RequiresConsentAsync(Client client, ClaimsPrincipal subject, IEnumerable<string> scopes)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (subject == null) throw new ArgumentNullException("subject");

            if (!client.RequireConsent)
            {
                return false;
            }

            // TODO: validate that this is a correct statement
            if (!client.AllowRememberConsent)
            {
                return true;
            }

            if (scopes == null || !scopes.Any())
            {
                return false;
            }

            // we always require consent for offline access if
            // the client has not disabled RequireConsent 
            if (scopes.Contains(Constants.StandardScopes.OfflineAccess))
            {
                return true;
            }
            
            var consent = await _store.LoadAsync(subject.GetSubjectId(), client.ClientId);
            if (consent != null && consent.Scopes != null)
            {
                var intersect = scopes.Intersect(consent.Scopes);
                return !(scopes.Count() == intersect.Count());
            }

            return true;
        }

        /// <summary>
        /// Updates the consent.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        public virtual async Task UpdateConsentAsync(Client client, ClaimsPrincipal subject, IEnumerable<string> scopes)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (subject == null) throw new ArgumentNullException("subject");

            if (client.AllowRememberConsent)
            {
                var subjectId = subject.GetSubjectId();
                var clientId = client.ClientId;

                if (scopes != null && scopes.Any())
                {
                    var consent = new Consent
                    {
                        Subject = subjectId,
                        ClientId = clientId,
                        Scopes = scopes
                    };
                    await _store.UpdateAsync(consent);
                }
                else
                {
                    await _store.RevokeAsync(subjectId, clientId);
                }
            }
        }
    }
}