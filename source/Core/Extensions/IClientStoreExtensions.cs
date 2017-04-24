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
using IdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.Extensions
{
    internal static class IClientStoreExtensions
    {
        internal static async Task<IEnumerable<string>> GetIdentityProviderRestrictionsAsync(this IClientStore store, string clientId)
        {
            if (store == null) throw new ArgumentNullException("store");

            if (clientId.IsPresent())
            {
                var client = await store.FindClientByIdAsync(clientId);
                if (client != null &&
                    client.IdentityProviderRestrictions != null &&
                    client.IdentityProviderRestrictions.Any())
                {
                    return client.IdentityProviderRestrictions;
                }
            }

            return Enumerable.Empty<string>();
        }

        internal static async Task<bool> IsValidIdentityProviderAsync(this IClientStore store, string clientId, string provider)
        {
            var restrictions = await store.GetIdentityProviderRestrictionsAsync(clientId);
            
            if (restrictions.Any())
            {
                return restrictions.Contains(provider);
            }

            return true;
        }

        internal static async Task<string> GetClientName(this IClientStore store, SignOutMessage signOutMessage)
        {
            if (store == null) throw new ArgumentNullException("store");

            if (signOutMessage != null && signOutMessage.ClientId.IsPresent())
            {
                var client = await store.FindClientByIdAsync(signOutMessage.ClientId);
                if (client != null)
                {
                    return client.ClientName;
                }
            }

            return null;
        }
    }
}
