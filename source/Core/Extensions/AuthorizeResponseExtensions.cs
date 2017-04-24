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

using IdentityServer.Core.Models;
using System.Collections.Specialized;

namespace IdentityServer.Core.Extensions
{
    internal static class AuthorizeResponseExtensions
    {
        public static NameValueCollection ToNameValueCollection(this AuthorizeResponse response)
        {
            var collection = new NameValueCollection();

            if (response.IsError)
            {
                if (response.Error.IsPresent())
                {
                    collection.Add("error", response.Error);
                }

                if (response.ErrorDescription.IsPresent())
                {
                    collection.Add("error_description", response.ErrorDescription);
                }
            }
            else
            {
                if (response.Code.IsPresent())
                {
                    collection.Add("code", response.Code);
                }

                if (response.IdentityToken.IsPresent())
                {
                    collection.Add("id_token", response.IdentityToken);
                }

                if (response.AccessToken.IsPresent())
                {
                    collection.Add("access_token", response.AccessToken);
                    collection.Add("token_type", "Bearer");
                    collection.Add("expires_in", response.AccessTokenLifetime.ToString());
                }

                if (response.Scope.IsPresent())
                {
                    collection.Add("scope", response.Scope);
                }
            }

            if (response.State.IsPresent())
            {
                collection.Add("state", response.State);
            }
            
            if (response.SessionState.IsPresent())
            {
                collection.Add("session_state", response.SessionState);
            }

            return collection;
        }
    }
}