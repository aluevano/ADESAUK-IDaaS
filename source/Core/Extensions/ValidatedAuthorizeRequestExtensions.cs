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
using IdentityServer.Core.Validation;
using IdentityServer.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.Core.Extensions
{
    internal static class ValidatedAuthorizeRequestExtensions
    {
        public static IEnumerable<ConsentScopeViewModel> GetIdentityScopes(this ValidatedAuthorizeRequest validatedRequest, ILocalizationService localizationService)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes.Where(x => x.Type == ScopeType.Identity);
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);
            return requestedScopes.ToConsentScopeViewModel(consentedScopeNames, localizationService);
        }

        public static IEnumerable<ConsentScopeViewModel> GetResourceScopes(this ValidatedAuthorizeRequest validatedRequest, ILocalizationService localizationService)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes.Where(x=> x.Type == ScopeType.Resource);
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);
            return requestedScopes.ToConsentScopeViewModel(consentedScopeNames, localizationService);
        }

        public static IEnumerable<ConsentScopeViewModel> ToConsentScopeViewModel(this IEnumerable<Scope> scopes, IEnumerable<string> selected, ILocalizationService localizationService)
        {
            var values =
                from s in scopes
                select new ConsentScopeViewModel
                {
                    Selected = selected.Contains(s.Name),
                    Name = s.Name,
                    DisplayName = s.DisplayName ?? localizationService.GetScopeDisplayName(s.Name),
                    Description = s.Description ?? localizationService.GetScopeDescription(s.Name),
                    Emphasize = s.Emphasize,
                    Required = s.Required
                };
            return values;
        }

        internal static bool HasIdpAcrValue(this ValidatedAuthorizeRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return request.AuthenticationContextReferenceClasses.Any(x => x.StartsWith(Constants.KnownAcrValues.HomeRealm));
        }
    }
}