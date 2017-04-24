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

using IdentityServer.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.InMemory
{
    /// <summary>
    /// In-memory consent store
    /// </summary>
    public class InMemoryConsentStore : IConsentStore
    {
        private readonly List<Consent> _consents = new List<Consent>();

        /// <summary>
        /// Loads all permissions the subject has granted to all clients.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>The permissions.</returns>
        public Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var query =
                from c in _consents
                where c.Subject == subject
                select c;
            return Task.FromResult<IEnumerable<Consent>>(query.ToArray());
        }

        /// <summary>
        /// Loads the subject's prior consent for the client.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns>The persisted consent.</returns>
        public Task<Consent> LoadAsync(string subject, string client)
        {
            var query =
                from c in _consents
                where c.Subject == subject && c.ClientId == client
                select c;
            return Task.FromResult(query.SingleOrDefault());
        }


        /// <summary>
        /// Persists the subject's consent.
        /// </summary>
        /// <param name="consent">The consent.</param>
        /// <returns></returns>
        public Task UpdateAsync(Consent consent)
        {
            // makes a snapshot as a DB would
            consent.Scopes = consent.Scopes.ToArray();

            var query =
                from c in _consents
                where c.Subject == consent.Subject && c.ClientId == consent.ClientId
                select c;
            var item = query.SingleOrDefault();
            if (item != null)
            {
                item.Scopes = consent.Scopes;
            }
            else
            {
                _consents.Add(consent);
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Revokes all permissions the subject has given to a client.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public Task RevokeAsync(string subject, string client)
        {
            var query =
                from c in _consents
                where c.Subject == subject && c.ClientId == client
                select c;
            var item = query.SingleOrDefault();
            if (item != null)
            {
                _consents.Remove(item);
            }
            return Task.FromResult(0);
        }
    }
}