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
using IdentityServer.Core.Services.InMemory;
using System;
using System.Security.Claims;

namespace IdentityServer.Tests.Conformance
{
    public class IdentityServerHostTest : IDisposable
    {
        protected IdentityServerHost host = new IdentityServerHost();
        
        public IdentityServerHostTest()
        {
            host.Users.Add(new InMemoryUser{
                Subject = "818727", Username = "bob", Password = "bob", 
                Claims = new Claim[]
                {
                    new Claim(Constants.ClaimTypes.Name, "Bob Loblaw"),
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Loblaw"),
                    new Claim(Constants.ClaimTypes.Email, "bob@email.com"),
                    new Claim(Constants.ClaimTypes.Role, "Admin"),
                    new Claim(Constants.ClaimTypes.Role, "Geek"),
                    new Claim(Constants.ClaimTypes.WebSite, "http://bob.com"),
                    new Claim(Constants.ClaimTypes.Address, "{ \"street_address\": \"One Hacker Way\", \"locality\": \"Heidelberg\", \"postal_code\": 69118, \"country\": \"Germany\" }")
                }
            });

            Init();
        }

        protected virtual void Init()
        {
            PreInit();
            host.Init();
            PostInit();
        }

        protected virtual void PreInit()
        {
        }

        protected virtual void PostInit()
        {
        }

        public void Dispose()
        {
            host.Dispose();
        }
    }
}
