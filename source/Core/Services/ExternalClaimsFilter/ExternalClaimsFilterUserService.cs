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
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    internal class ExternalClaimsFilterUserService : IUserService
    {
        readonly IExternalClaimsFilter filter;
        readonly IUserService inner;

        public ExternalClaimsFilterUserService(IExternalClaimsFilter filter, IUserService inner)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            if (inner == null) throw new ArgumentNullException("inner");

            this.filter = filter;
            this.inner = inner;
        }

        public Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            return inner.PreAuthenticateAsync(context);
        }

        public Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            return inner.AuthenticateLocalAsync(context);
        }

        public Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            context.ExternalIdentity.Claims = filter.Filter(context.ExternalIdentity.Provider, context.ExternalIdentity.Claims);
            return inner.AuthenticateExternalAsync(context);
        }

        public Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            return inner.PostAuthenticateAsync(context);
        }
        
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            return inner.GetProfileDataAsync(context);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return inner.IsActiveAsync(context);
        }

        public Task SignOutAsync(SignOutContext context)
        {
            return inner.SignOutAsync(context);
        }
    }
}