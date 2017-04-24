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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    internal class AggregatePermissionsStore : IPermissionsStore
    {
        readonly IPermissionsStore[] stores;

        public AggregatePermissionsStore(params IPermissionsStore[] stores)
        {
            if (stores == null) throw new ArgumentNullException("stores");

            this.stores = stores;
        }
        
        public async Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var result = 
                await stores
                    .Select(x => x.LoadAllAsync(subject))
                    .Aggregate(async (t1, t2) => (await t1).Union(await t2));

            var query = 
                from item in result
                group item by item.ClientId into grp
                let scopes = (from g in grp select g.Scopes).Aggregate((s1, s2)=>s1.Union(s2).Distinct())
                select new Consent
                {
                    ClientId = grp.Key,
                    Subject = subject,
                    Scopes = scopes
                };

            return query;
        }

        public async Task RevokeAsync(string subject, string client)
        {
            foreach (var store in stores)
            {
                await store.RevokeAsync(subject, client);
            }
        }
    }
}
