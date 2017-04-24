﻿/*
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
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdentityServer.Core.Configuration.Hosting
{
    internal class ClientListCookie
    {
        const string ClientListCookieName = "idsvr.clients";

        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        readonly IOwinContext ctx;
        readonly IdentityServerOptions options;

        public ClientListCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            this.ctx = ctx;
            this.options = options;
        }

        public void Clear()
        {
            SetClients(null);
        }

        public void AddClient(string clientId)
        {
            if (options.Endpoints.EnableEndSessionEndpoint)
            {
                var clients = GetClients();
                if (!clients.Contains(clientId))
                {
                    var update = clients.ToList();
                    update.Add(clientId);
                    SetClients(update);
                }
            }
        }

        public IEnumerable<string> GetClients()
        {
            var value = GetCookie();
            if (String.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<string>();
            }

            return JsonConvert.DeserializeObject<string[]>(value, settings);
        }

        void SetClients(IEnumerable<string> clients)
        {
            string value = null;
            if (clients != null && clients.Any())
            {
                value = JsonConvert.SerializeObject(clients);
            }

            SetCookie(value);
        }

        string CookieName
        {
            get
            {
                return options.AuthenticationOptions.CookieOptions.GetCookieName(ClientListCookieName);
            }
        }

        string CookiePath
        {
            get
            {
                return ctx.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();
            }
        }

        private bool Secure
        {
            get
            {
                return
                    options.AuthenticationOptions.CookieOptions.SecureMode == CookieSecureMode.Always ||
                    ctx.Request.Scheme == Uri.UriSchemeHttps;
            }
        }

        void SetCookie(string value)
        {
            DateTime? expires = null;
            if (String.IsNullOrWhiteSpace(value))
            {
                var existingValue = GetCookie();
                if (existingValue == null)
                {
                    // no need to write cookie to clear if we don't already have one
                    return;
                }

                value = ".";
                expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                // encode the value
                var bytes = Encoding.UTF8.GetBytes(value);
                value = Base64Url.Encode(bytes);
            }

            var opts = new Microsoft.Owin.CookieOptions
            {
                HttpOnly = true,
                Secure = Secure,
                Path = CookiePath,
                Expires = expires
            };

            this.ctx.Response.Cookies.Append(CookieName, value, opts);
        }

        string GetCookie()
        {
            var value = this.ctx.Request.Cookies[CookieName];

            // the check here is to allow for handling cookies prior to manually encoding
            if (!String.IsNullOrWhiteSpace(value) && !value.StartsWith("["))
            {
                try
                {
                    var bytes = Base64Url.Decode(value);
                    value = Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                }

                if (!value.StartsWith("["))
                {
                    // deal with double encoding or just invalid values
                    value = "";
                }
            }

            return value;
        }
    }
}