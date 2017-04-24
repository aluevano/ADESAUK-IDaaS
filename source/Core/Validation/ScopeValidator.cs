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
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer.Core.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ScopeValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        private readonly IScopeStore _store;

        public bool ContainsOpenIdScopes { get; private set; }
        public bool ContainsResourceScopes { get; private set; }
        public bool ContainsOfflineAccessScope { get; set; }

        public List<Scope> RequestedScopes { get; private set; }
        public List<Scope> GrantedScopes { get; private set; }

        public ScopeValidator(IScopeStore store)
        {
            RequestedScopes = new List<Scope>();
            GrantedScopes = new List<Scope>();

            _store = store;
        }

        public static List<string> ParseScopesString(string scopes)
        {
            if (scopes.IsMissing())
            {
                Logger.Warn("Empty scopes.");
                return null;
            }

            scopes = scopes.Trim();
            var parsedScopes = scopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            if (parsedScopes.Any())
            {
                parsedScopes.Sort();
                return parsedScopes;
            }

            return null;
        }

        public void SetConsentedScopes(IEnumerable<string> consentedScopes)
        {
            consentedScopes = consentedScopes ?? Enumerable.Empty<string>();

            GrantedScopes.RemoveAll(scope => !scope.Required && !consentedScopes.Contains(scope.Name));
        }

        public async Task<bool> AreScopesValidAsync(IEnumerable<string> requestedScopes)
        {
            var availableScopes = await _store.FindScopesAsync(requestedScopes);

            foreach (var requestedScope in requestedScopes)
            {
                var scopeDetail = availableScopes.FirstOrDefault(s => s.Name == requestedScope);

                if (scopeDetail == null)
                {
                    Logger.ErrorFormat("Invalid scope: {0}", requestedScope);
                    return false;
                }

                if (scopeDetail.Enabled == false)
                {
                    Logger.ErrorFormat("Scope disabled: {0}", requestedScope);
                    return false;
                }

                if (scopeDetail.Type == ScopeType.Identity)
                {
                    ContainsOpenIdScopes = true;
                }
                else
                {
                    ContainsResourceScopes = true;
                }

                GrantedScopes.Add(scopeDetail);
            }

            if (requestedScopes.Contains(Constants.StandardScopes.OfflineAccess))
            {
                ContainsOfflineAccessScope = true;
            }

            RequestedScopes.AddRange(GrantedScopes);

            return true;
        }

        public bool AreScopesAllowed(Client client, IEnumerable<string> requestedScopes)
        {
            if (client.AllowAccessToAllScopes)
            {
                return true;
            }

            foreach (var scope in requestedScopes)
            {
                if (!client.AllowedScopes.Contains(scope))
                {
                    Logger.ErrorFormat("Requested scope not allowed: {0}", scope);
                    return false;
                }
            }

            return true;
        }

        public bool IsResponseTypeValid(string responseType)
        {
            var requirement = Constants.ResponseTypeToScopeRequirement[responseType];

            // must include identity scopes
            if (requirement == Constants.ScopeRequirement.Identity)
            {
                if (!ContainsOpenIdScopes)
                {
                    Logger.Error("Requests for id_token response type must include identity scopes");
                    return false;
                }
            }

            // must include identity scopes only
            else if (requirement == Constants.ScopeRequirement.IdentityOnly)
            {
                if (!ContainsOpenIdScopes || ContainsResourceScopes)
                {
                    Logger.Error("Requests for id_token response type only must not include resource scopes");
                    return false;
                }
            }

            // must include resource scopes only
            else if (requirement == Constants.ScopeRequirement.ResourceOnly)
            {
                if (ContainsOpenIdScopes || !ContainsResourceScopes)
                {
                    Logger.Error("Requests for token response type only must include resource scopes, but no identity scopes.");
                    return false;
                }
            }

            return true;
        }
    }
}