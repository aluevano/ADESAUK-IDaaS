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


using FluentAssertions;
using IdentityServer.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;

namespace IdentityServer.Tests.Endpoints
{
    static class Extensions
    {
        public static void SetCookies(this HttpClient client, IEnumerable<string> cookies)
        {
            foreach (var c in cookies)
            {
                if (c.LooksLikeACookieDeletion())
                {
                    client.RemoveCookieByName(c);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("Cookie", c);
                }
            }
        }
        public static void SetCookies(this HttpClient client, IEnumerable<CookieState> cookies)
        {
            client.SetCookies(cookies.Select(c => c.ToString()));
        }

        public static IEnumerable<CookieState> GetCookies(this HttpResponseMessage resp)
        {
            IEnumerable<string> values;
            if (resp.Headers.TryGetValues("Set-Cookie", out values))
            {
                var cookies = new List<CookieState>();
                foreach (var value in values)
                {
                    CookieHeaderValue cookie;
                    if (CookieHeaderValue.TryParse(value, out cookie))
                    {
                        cookies.AddRange(cookie.Cookies);
                    }
                }
                return cookies;
            }
            return Enumerable.Empty<CookieState>();
        }

        public static IEnumerable<string> GetRawCookies(this HttpResponseMessage resp)
        {
            IEnumerable<string> values;
            if (resp.Headers.TryGetValues("Set-Cookie", out values))
            {
                return values;
            }
            return Enumerable.Empty<string>();
        }

        public static void AssertCookie(this HttpResponseMessage resp, string name)
        {
            var cookies = resp.GetCookies();
            var cookie = cookies.SingleOrDefault(x => x.Name == name);
            cookie.Should().NotBeNull();
        }

        public static void AssertPage(this HttpResponseMessage resp, string name)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            resp.Content.Headers.ContentType.MediaType.Should().Be("text/html");
            var html = resp.Content.ReadAsStringAsync().Result;

            var match = Regex.Match(html, "<div class='container page-(.*)' ng-cloak>");
            match.Groups[1].Value.Should().Be(name);
        }

        static T GetModel<T>(string html)
        {
            var match = "<script id='modelJson' type='application/json'>";
            var start = html.IndexOf(match);
            var end = html.IndexOf("</script>", start);
            var content = html.Substring(start + match.Length, end - start - match.Length);
            var json = HttpUtility.HtmlDecode(content);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T GetModel<T>(this HttpResponseMessage resp)
        {
            resp.IsSuccessStatusCode.Should().BeTrue();
            var html = resp.Content.ReadAsStringAsync().Result;
            return GetModel<T>(html);
        }

        public static T GetJson<T>(this HttpResponseMessage resp, Boolean successExpected = true)
        {
            if (successExpected)
            {
                resp.IsSuccessStatusCode.Should().BeTrue();
            }

            var json = resp.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static void RemoveCookieByName(this HttpClient client, string cookieString)
        {
            Conformance.Extensions.RemoveCookie(client, cookieString.Split('=').First());
        }

        private static bool LooksLikeACookieDeletion(this string cookieString)
        {
            if (!cookieString.Contains("expires"))
            {
                return false;
            }

            var parts = cookieString
                .Split(';')
                .Select(s => s.Trim())
                .ToDictionary(s => s.Split('=').First(), s => s.Split('=').Last());

            DateTime expiry;
            if (DateTime.TryParse(parts["expires"], out expiry))
            {
                return parts.First().Value == "." && expiry < DateTimeHelper.UtcNow;
            }

            return false;
        }
    }
}
