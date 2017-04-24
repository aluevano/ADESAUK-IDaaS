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
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.InMemory
{
    /// <summary>
    /// In-memory user service
    /// </summary>
    public class InMemoryUserService : UserServiceBase
    {
        readonly List<InMemoryUser> _users;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryUserService"/> class.
        /// </summary>
        /// <param name="users">The users.</param>
        public InMemoryUserService(List<InMemoryUser> users)
        {
            _users = users;
        }

        /// <summary>
        /// This methods gets called for local authentication (whenever the user uses the username and password dialog).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var query =
                from u in _users
                where u.Username == context.UserName && u.Password == context.Password
                select u;

            var user = query.SingleOrDefault();
            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Subject, GetDisplayName(user));
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// This method gets called when the user uses an external identity provider to authenticate.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            var query =
                from u in _users
                where
                    u.Provider == context.ExternalIdentity.Provider &&
                    u.ProviderId == context.ExternalIdentity.ProviderId
                select u;

            var user = query.SingleOrDefault();
            if (user == null)
            {
                string displayName;

                var name = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
                if (name == null)
                {
                    displayName = context.ExternalIdentity.ProviderId;
                }
                else
                {
                    displayName = name.Value;
                }

                user = new InMemoryUser
                {
                    Subject = CryptoRandom.CreateUniqueId(),
                    Provider = context.ExternalIdentity.Provider,
                    ProviderId = context.ExternalIdentity.ProviderId,
                    Username = displayName,
                    Claims = context.ExternalIdentity.Claims
                };
                _users.Add(user);
            }

            context.AuthenticateResult = new AuthenticateResult(user.Subject, GetDisplayName(user), identityProvider:context.ExternalIdentity.Provider);
            
            return Task.FromResult(0);
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var query =
                from u in _users
                where u.Subject == context.Subject.GetSubjectId()
                select u;
            var user = query.Single();

            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, user.Subject),
            };

            claims.AddRange(user.Claims);
            if (!context.AllClaimsRequested)
            {
                claims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
            }

            context.IssuedClaims = claims;

            return Task.FromResult(0);
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. during token issuance or validation)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">subject</exception>
        public override Task IsActiveAsync(IsActiveContext context)
        {
            if (context.Subject == null) throw new ArgumentNullException("subject");

            var query =
                from u in _users
                where
                    u.Subject == context.Subject.GetSubjectId()
                select u;

            var user = query.SingleOrDefault();
            
            context.IsActive = (user != null) && user.Enabled;

            return Task.FromResult(0);
        }

        /// <summary>
        /// Retrieves the display name.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual string GetDisplayName(InMemoryUser user)
        {
            var nameClaim = user.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
            if (nameClaim != null)
            {
                return nameClaim.Value;
            }

            return user.Username;
        }
    }
}