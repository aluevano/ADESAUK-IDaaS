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

using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Results
{
    internal class TokenResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly static JsonSerializer Serializer = new JsonSerializer
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly TokenResponse _response;

        public TokenResult(TokenResponse response)
        {
            _response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var dto = new TokenResponseDto
            {
                id_token = _response.IdentityToken,
                access_token = _response.AccessToken,
                refresh_token = _response.RefreshToken,
                expires_in = _response.AccessTokenLifetime,
                token_type = _response.TokenType,
                alg = _response.Algorithm
            };

            var jobject = JObject.FromObject(dto, Serializer);

            // custom entries
            if (_response.Custom != null && _response.Custom.Any())
            {
                foreach (var item in _response.Custom)
                {
                    JToken token;
                    if (jobject.TryGetValue(item.Key, out token))
                    {
                        throw new Exception("Item does already exist - cannot add it via a custom entry: " + item.Key);
                    }

                    if (item.Value.GetType().IsClass)
                    {
                        jobject.Add(new JProperty(item.Key, JToken.FromObject(item.Value)));
                    }
                    else
                    {
                        jobject.Add(new JProperty(item.Key, item.Value));
                    }
                }
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jobject.ToString(Formatting.None), Encoding.UTF8, "application/json")
            };

            Logger.Info("Returning token response.");
            return response;
        }

        internal class TokenResponseDto
        {
            public string id_token { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public string alg { get; set; }
        }    
    }
}