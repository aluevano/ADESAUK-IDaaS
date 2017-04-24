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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace IdentityServer.Core.Extensions
{
    internal static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection collection)
        {
            if (collection.Count == 0)
            {
                return String.Empty;
            }

            var builder = new StringBuilder(128);
            bool first = true;
            foreach (string name in collection)
            {
                string[] values = collection.GetValues(name);
                if (values == null || values.Length == 0)
                {
                    first = AppendNameValuePair(builder, first, true, name, String.Empty);
                }
                else
                {
                    foreach (string value in values)
                    {
                        first = AppendNameValuePair(builder, first, true, name, value);
                    }
                }
            }

            return builder.ToString();
        }

        public static string ToFormPost(this NameValueCollection collection)
        {
            var builder = new StringBuilder(128);
            const string inputFieldFormat = "<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />\n";

            foreach (string name in collection)
            {
                var values = collection.GetValues(name);
                var value = values.First();
                value = Microsoft.Security.Application.Encoder.HtmlEncode(value);
                builder.AppendFormat(inputFieldFormat, name, value);
            }

            return builder.ToString();
        }


        public static Dictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            var dict = new Dictionary<string, string>();
            
            if (collection == null || collection.Count == 0)
            {
                return dict;
            }

            foreach (string name in collection)
            {
                var value = collection.Get(name);
                if (value != null)
                {
                    dict.Add(name, value);
                }
            }

            return dict;
        }

        internal static string ConvertFormUrlEncodedSpacesToUrlEncodedSpaces(string str)
        {
            if ((str != null) && (str.IndexOf('+') >= 0))
            {
                str = str.Replace("+", "%20");
            }
            return str;
        }

        private static bool AppendNameValuePair(StringBuilder builder, bool first, bool urlEncode, string name, string value)
        {
            string effectiveName = name ?? String.Empty;
            string encodedName = urlEncode ? WebUtility.UrlEncode(effectiveName) : effectiveName;

            string effectiveValue = value ?? String.Empty;
            string encodedValue = urlEncode ? WebUtility.UrlEncode(effectiveValue) : effectiveValue;
            encodedValue = ConvertFormUrlEncodedSpacesToUrlEncodedSpaces(encodedValue);

            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append("&");
            }

            builder.Append(encodedName);
            if (!String.IsNullOrEmpty(encodedValue))
            {
                builder.Append("=");
                builder.Append(encodedValue);
            }
            return first;
        }
    }
}