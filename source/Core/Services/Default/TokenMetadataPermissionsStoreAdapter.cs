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
    internal class TokenMetadataPermissionsStoreAdapter : IPermissionsStore
    {
        readonly Func<string, Task<IEnumerable<ITokenMetadata>>> get;
        readonly Func<string, string, Task> delete;

        public TokenMetadataPermissionsStoreAdapter(
            Func<string, Task<IEnumerable<ITokenMetadata>>> get, 
            Func<string, string, Task> delete)
        {
            if (get == null) throw new ArgumentNullException("get");
            if (delete == null) throw new ArgumentNullException("delete");

            this.get = get;
            this.delete = delete;
        }

        public async Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var tokens = await get(subject);
            
            var query =
                from token in tokens
                select new Consent
                {
                    ClientId = token.ClientId,
                    Subject = token.SubjectId,
                    Scopes = token.Scopes
                };

            return query.ToArray();
        }

        public async Task RevokeAsync(string subject, string client)
        {
            await delete(subject, client);
        }
    }
}
