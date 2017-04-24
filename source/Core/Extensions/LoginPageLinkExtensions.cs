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

using IdentityServer.Core.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.Core.Extensions
{
    internal static class LoginPageLinkExtensions
    {
        internal static IEnumerable<LoginPageLink> Render(this IEnumerable<LoginPageLink> links, string baseUrl, string signinId)
        {
            if (links == null || !links.Any()) return null;

            var result = new List<LoginPageLink>();
            foreach (var link in links)
            {
                var url = link.Href;
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(2);
                    url = baseUrl + url;
                }

                url = url.AddQueryString("signin=" + signinId);

                result.Add(new LoginPageLink
                {
                    Type = link.Type,
                    Text = link.Text,
                    Href = url
                });
            }
            return result;
        }
    }
}
