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

using IdentityModel;
using IdentityServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    internal class KeyHashingRefreshTokenStore : KeyHashingTransientDataRepository<RefreshToken>, IRefreshTokenStore
    {
        public KeyHashingRefreshTokenStore(IRefreshTokenStore inner)
            : base(inner)
        {
        }
    }
    
    internal class KeyHashingAuthorizationCodeStore : KeyHashingTransientDataRepository<AuthorizationCode>, IAuthorizationCodeStore
    {
        public KeyHashingAuthorizationCodeStore(IAuthorizationCodeStore inner)
            : base(inner)
        {
        }
    }
    
    internal class KeyHashingTokenHandleStore : KeyHashingTransientDataRepository<Token>, ITokenHandleStore
    {
        public KeyHashingTokenHandleStore(ITokenHandleStore inner)
            : base(inner)
        {
        }
    }

    internal class KeyHashingTransientDataRepository<T> : ITransientDataRepository<T>
        where T : ITokenMetadata
    {
        readonly string hashName;
        readonly ITransientDataRepository<T> inner;

        public KeyHashingTransientDataRepository(ITransientDataRepository<T> inner)
            : this(Constants.DefaultHashAlgorithm, inner)
        {
        }

        public KeyHashingTransientDataRepository(string hashName, ITransientDataRepository<T> inner)
        {
            if (String.IsNullOrWhiteSpace(hashName)) throw new ArgumentNullException("hashName");
            if (inner == null) throw new ArgumentNullException("inner");

            this.hashName = hashName;
            this.inner = inner;
        }

        protected string Hash(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");

            using (var hash = HashAlgorithm.Create(hashName))
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var hashedBytes = hash.ComputeHash(bytes);
                var hashedString = Base64Url.Encode(hashedBytes);
                return hashedString;
            }
        }

        public Task StoreAsync(string key, T value)
        {
            return inner.StoreAsync(Hash(key), value);
        }

        public Task<T> GetAsync(string key)
        {
            return inner.GetAsync(Hash(key));
        }

        public Task RemoveAsync(string key)
        {
            return inner.RemoveAsync(Hash(key));
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            return inner.GetAllAsync(subject);
        }

        public Task RevokeAsync(string subject, string client)
        {
            return inner.RevokeAsync(subject, client);
        }
    }
}
