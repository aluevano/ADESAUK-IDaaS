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
using IdentityServer.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.Core.Extensions
{
    internal static class ClaimListExtensions
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public static Dictionary<string, object> ToClaimsDictionary(this IEnumerable<Claim> claims)
        {
            var d = new Dictionary<string, object>();

            if (claims == null)
            {
                return d;
            }

            var distinctClaims = claims.Distinct(new ClaimComparer());

            foreach (var claim in distinctClaims)
            {
                if (!d.ContainsKey(claim.Type))
                {
                    d.Add(claim.Type, GetValue(claim));
                }
                else
                {
                    var value = d[claim.Type];

                    var list = value as List<object>;
                    if (list != null)
                    {
                        list.Add(GetValue(claim));
                    }
                    else
                    {
                        d.Remove(claim.Type);
                        d.Add(claim.Type, new List<object> { value, GetValue(claim) });
                    }
                }
            }

            return d;
        }

        private static object GetValue(Claim claim)
        {
            if (claim.ValueType == ClaimValueTypes.Integer ||
                claim.ValueType == ClaimValueTypes.Integer32)
            {
                Int32 value;
                if (Int32.TryParse(claim.Value, out value))
                {
                    return value;
                }
            }

            if (claim.ValueType == ClaimValueTypes.Integer64)
            {
                Int64 value;
                if (Int64.TryParse(claim.Value, out value))
                {
                    return value;
                }
            }

            if (claim.ValueType == ClaimValueTypes.Boolean)
            {
                bool value;
                if (bool.TryParse(claim.Value, out value))
                {
                    return value;
                }
            }

            if (claim.ValueType == Constants.ClaimValueTypes.Json)
            {
                try
                {
                    return JsonConvert.DeserializeObject(claim.Value);
                }
                catch { }
                
            }

            return claim.Value;
        }
    }
}