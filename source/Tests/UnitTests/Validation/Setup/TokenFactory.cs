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

using IdentityServer.Core;
using IdentityServer.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.Tests.Validation
{
    static class TokenFactory
    {
        public static Token CreateAccessToken(Client client, string subjectId, int lifetime, params string[] scopes)
        {
            var claims = new List<Claim> 
            {
                new Claim("client_id", client.ClientId),
                new Claim("sub", subjectId)
            };

            scopes.ToList().ForEach(s => claims.Add(new Claim("scope", s)));

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = "https://idsrv3.com/resources",
                Issuer = "https://idsrv3.com",
                Lifetime = lifetime,
                Claims = claims,
                Client = client
            };

            return token;
        }

        public static Token CreateAccessTokenLong(Client client, string subjectId, int lifetime, int count, params string[] scopes)
        {
            var claims = new List<Claim>
            {
                new Claim("client_id", client.ClientId),
                new Claim("sub", subjectId)
            };

            for (int i = 0; i < count; i++)
            {
                claims.Add(new Claim("junk", "x".Repeat(100)));
            }

            scopes.ToList().ForEach(s => claims.Add(new Claim("scope", s)));

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = "https://idsrv3.com/resources",
                Issuer = "https://idsrv3.com",
                Lifetime = lifetime,
                Claims = claims,
                Client = client
            };

            return token;
        }

        public static Token CreateIdentityToken(string clientId, string subjectId)
        {
            var clients = Factory.CreateClientStore();

            var claims = new List<Claim> 
            {
                new Claim("sub", subjectId)
            };

            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = clientId,
                Client = clients.FindClientByIdAsync(clientId).Result,
                Issuer = "https://idsrv3.com",
                Lifetime = 600,
                Claims = claims
            };

            return token;
        }

        public static Token CreateIdentityTokenLong(string clientId, string subjectId, int count)
        {
            var clients = Factory.CreateClientStore();

            var claims = new List<Claim>
            {
                new Claim("sub", subjectId)
            };

            for (int i = 0; i < count; i++)
            {
                claims.Add(new Claim("junk", "x".Repeat(100)));
            }

            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = clientId,
                Client = clients.FindClientByIdAsync(clientId).Result,
                Issuer = "https://idsrv3.com",
                Lifetime = 600,
                Claims = claims
            };

            return token;
        }
    }
}