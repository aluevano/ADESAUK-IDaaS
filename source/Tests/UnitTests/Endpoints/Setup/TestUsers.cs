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
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer.Tests.Endpoints
{
    public class TestUsers
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
                {
                    new InMemoryUser{Subject = "818727", Username = "alice", Password = "alice", 
                        Claims = new Claim[]
                        {
                            new Claim(Constants.ClaimTypes.GivenName, "Alice"),
                            new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                            new Claim(Constants.ClaimTypes.Email, "AliceSmith@email.com"),
                        }, 
                        Provider = "Google", 
                        ProviderId = "123"
                    },
                    new InMemoryUser{Subject = "88421113", Username = "bob", Password = "bob", 
                        Claims = new Claim[]
                        {
                            new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                            new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                            new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                        }
                    },
                    new InMemoryUser{Subject = "999", Username = "sam", Password = "sam",
                        Claims = new Claim[]
                        {
                            new Claim(Constants.ClaimTypes.GivenName, "Sam"),
                            new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                            new Claim(Constants.ClaimTypes.Email, "SamSmith@email.com"),
                        },
                        Provider = "Google2", ProviderId = "999"
                    },
                };
        }
    }
}
