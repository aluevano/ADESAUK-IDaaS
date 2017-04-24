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
using IdentityServer.Core.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.ResponseHandling
{
    internal class IntrospectionResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult, Scope scope)
        {
            Logger.Info("Creating introspection response");

            var response = new Dictionary<string, object>();
            
            if (validationResult.IsActive == false)
            {
                Logger.Info("Creating introspection response for inactive token.");

                response.Add("active", false);
                return Task.FromResult(response);
            }

            if (scope.AllowUnrestrictedIntrospection)
            {
                Logger.Info("Creating unrestricted introspection response for active token.");

                response = validationResult.Claims.ToClaimsDictionary();
                response.Add("active", true);
            }
            else
            {
                Logger.Info("Creating restricted introspection response for active token.");

                response = validationResult.Claims.Where(c => c.Type != Constants.ClaimTypes.Scope).ToClaimsDictionary();
                response.Add("active", true);
                response.Add("scope", scope.Name);
            }

            return Task.FromResult(response);
        }
    }
}