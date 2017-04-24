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

using System.Threading.Tasks;
using System.Security.Claims;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default implementation of IAuthenticationSessionValidator. Always returns true.
    /// </summary>
    public class DefaultAuthenticationSessionValidator : IAuthenticationSessionValidator
    {
        /// <summary>
        /// This method is called whenever an authentication cookie is presented to IdentityServer for the logged in user.
        /// Return true to indicate the authentication cookie should be honored, false otherwise.
        /// </summary>
        /// <param name="subject">The user.</param>
        /// <returns>true if authentication session is valid, false otherwise.</returns>
        public Task<bool> IsAuthenticationSessionValidAsync(ClaimsPrincipal subject)
        {
            return Task.FromResult(true);
        }
    }
}
