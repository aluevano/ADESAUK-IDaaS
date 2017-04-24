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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace IdentityServer.Core.Services.Default
{
    internal class AssetManager
    {
        public const string HttpAssetsNamespace = "IdentityServer3.Core.Services.DefaultViewService.HttpAssets";
        public const string FontAssetsNamespace = HttpAssetsNamespace + ".libs.bootstrap.fonts";

        public const string PageAssetsNamespace = "IdentityServer3.Core.Services.DefaultViewService.PageAssets";
        const string PagesPrefix = PageAssetsNamespace + ".";
        const string Layout = PagesPrefix + "layout.html";
        const string FormPostResponse = PagesPrefix + "FormPostResponse.html";
        const string CheckSession = PagesPrefix + "checksession.html";
        const string SignoutFrame = PagesPrefix + "SignoutFrame.html";
        const string Welcome = PagesPrefix + "welcome.html";

        static readonly ResourceCache cache = new ResourceCache();

        const string PageNameTemplate = PagesPrefix + "{0}" + ".html";
        public static string LoadPage(string pageName)
        {
            pageName = String.Format(PageNameTemplate, pageName);
            return LoadResourceString(pageName);
        }

        public static string ApplyContentToLayout(string layout, string content)
        {
            return Format(layout, new { pageContent = content });
        }
        
        public static string LoadLayoutWithContent(string content)
        {
            if (content == null) return null;

            var layout = LoadResourceString(Layout);
            return ApplyContentToLayout(layout, content);
        }

        public static string LoadLayoutWithPage(string pageName)
        {
            var pageContent = LoadPage(pageName);
            return LoadLayoutWithContent(pageContent);
        }

        public static string LoadFormPost(string rootUrl, string redirectUri, string fields)
        {
            return LoadResourceString(FormPostResponse,
                new
                {
                    rootUrl,
                    redirect_uri = redirectUri,
                    fields
                }
            );
        }

        public static string LoadCheckSession(string rootUrl, string cookieName)
        {
            return LoadResourceString(CheckSession, new
            {
                rootUrl,
                cookieName
            });
        }

        public static string LoadSignoutFrame(IEnumerable<string> frameUrls)
        {
            string frames = null;
            if (frameUrls != null && frameUrls.Any())
            {
                frameUrls = frameUrls.Select(x => String.Format("<iframe src='{0}'></iframe>", x));
                frames = frameUrls.Aggregate((x, y) => x + Environment.NewLine + y);
            }

            return LoadResourceString(SignoutFrame, new
            {
                frames
            });
        }

        internal static string LoadWelcomePage(string applicationPath, string version)
        {
            applicationPath = applicationPath.RemoveTrailingSlash();
            return LoadResourceString(Welcome, new
            {
                applicationPath,
                version
            });
        }
        
        static string LoadResourceString(string name)
        {
            string value = cache.Read(name);
            if (value == null)
            {
                var assembly = typeof(AssetManager).Assembly;
                var s = assembly.GetManifestResourceStream(name);
                if (s != null)
                {
                    using (var sr = new StreamReader(s))
                    {
                        value = sr.ReadToEnd();
                        cache.Write(name, value);
                    }
                }
            }
            return value;
        }

        static string LoadResourceString(string name, object data)
        {
            string value = LoadResourceString(name);
            if (value == null) return null;

            value = Format(value, data);
            return value;
        }

        static string Format(string value, IDictionary<string, object> data)
        {
            if (value == null) return null;

            foreach (var key in data.Keys)
            {
                var val = data[key];
                val = val ?? String.Empty;
                value = value.Replace("{" + key + "}", val.ToString());
            }
            return value;
        }

        public static string Format(string value, object data)
        {
            return Format(value, Map(data));
        }
        
        static IDictionary<string, object> Map(object values)
        {
            var dictionary = values as IDictionary<string, object>;
            
            if (dictionary == null) 
            {
                dictionary = new Dictionary<string, object>();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    dictionary.Add(descriptor.Name, descriptor.GetValue(values));
                }
            }

            return dictionary;
        }
    }
}

