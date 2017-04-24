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
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace IdentityServer.Core.ResponseHandling
{
    internal class UserInfoResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IUserService _users;
        private readonly IScopeStore _scopes;

        public UserInfoResponseGenerator(IUserService users, IScopeStore scopes)
        {
            _users = users;
            _scopes = scopes;
        }

        public async Task<Dictionary<string, object>> ProcessAsync(IEnumerable<Claim> userClaims, IEnumerable<string> scopes, Client client)
        {
            Logger.Info("Creating userinfo response");
            var profileData = new Dictionary<string, object>();

            var principal = Principal.Create("UserInfo", userClaims.ToArray());

            IEnumerable<Claim> profileClaims;

            var requestedClaimTypes = await GetRequestedClaimTypesAsync(scopes);
            if (requestedClaimTypes.IncludeAllClaims)
            {
                Logger.InfoFormat("Requested claim types: all");

                var context = new ProfileDataRequestContext(
                    principal, 
                    client, 
                    Constants.ProfileDataCallers.UserInfoEndpoint);

                await _users.GetProfileDataAsync(context);
                profileClaims = context.IssuedClaims;
            }
            else
            {
                Logger.InfoFormat("Requested claim types: {0}", requestedClaimTypes.ClaimTypes.ToSpaceSeparatedString());

                var context = new ProfileDataRequestContext(
                    principal,
                    client,
                    Constants.ProfileDataCallers.UserInfoEndpoint,
                    requestedClaimTypes.ClaimTypes);

                await _users.GetProfileDataAsync(context);
                profileClaims = context.IssuedClaims;
            }
            
            if (profileClaims != null)
            {
                profileData = profileClaims.ToClaimsDictionary();
                Logger.InfoFormat("Profile service returned to the following claim types: {0}", profileClaims.Select(c => c.Type).ToSpaceSeparatedString());
            }
            else
            {
                Logger.InfoFormat("Profile service returned no claims (null)");
            }

            return profileData;
        }

        public async Task<RequestedClaimTypes> GetRequestedClaimTypesAsync(IEnumerable<string> scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                return new RequestedClaimTypes();
            }

            var scopeString = string.Join(" ", scopes);
            Logger.InfoFormat("Scopes in access token: {0}", scopeString);

            var scopeDetails = await _scopes.FindScopesAsync(scopes);
            var scopeClaims = new List<string>();

            foreach (var scope in scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);
                
                if (scopeDetail != null)
                {
                    if (scopeDetail.Type == ScopeType.Identity)
                    {
                        if (scopeDetail.IncludeAllClaimsForUser)
                        {
                            return new RequestedClaimTypes
                            {
                                IncludeAllClaims = true
                            };
                        }

                        scopeClaims.AddRange(scopeDetail.Claims.Select(c => c.Name));
                    }
                }
            }

            return new RequestedClaimTypes(scopeClaims);
        }
    }
}