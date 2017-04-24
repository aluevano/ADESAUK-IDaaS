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

using IdentityModel;
using IdentityServer.Core.Configuration;
using System.Text;

namespace IdentityServer.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IdentityServer.Core.Configuration.IDataProtector"/>
    /// </summary>
    public static class IDataProtectorExtensions
    {
        /// <summary>
        /// Protects the specified data and Base64 Url encodes the response.
        /// </summary>
        /// <param name="protector">The protector.</param>
        /// <param name="data">The data.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns></returns>
        public static string Protect(this IDataProtector protector, string data, string entropy = "")
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var protectedBytes = protector.Protect(dataBytes, entropy);

            return Base64Url.Encode(protectedBytes);
        }

        /// <summary>
        /// Base64 Url decodes the input and unprotects the data.
        /// </summary>
        /// <param name="protector">The protector.</param>
        /// <param name="data">The data.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns></returns>
        public static string Unprotect(this IDataProtector protector, string data, string entropy = "")
        {
            var protectedBytes = Base64Url.Decode(data);
            var bytes = protector.Unprotect(protectedBytes, entropy);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}