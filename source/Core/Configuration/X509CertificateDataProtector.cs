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

using System.IdentityModel;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer.Core.Configuration
{
    /// <summary>
    /// X.509 certificate based data protector
    /// </summary>
    public class X509CertificateDataProtector : IDataProtector
    {
        readonly CookieTransform _encrypt;
        readonly CookieTransform _sign;

        /// <summary>
        /// Initializes a new instance of the <see cref="X509CertificateDataProtector"/> class.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        public X509CertificateDataProtector(X509Certificate2 certificate)
        {
            _encrypt = new RsaEncryptionCookieTransform(certificate);
            _sign = new RsaSignatureCookieTransform(certificate);
        }

        /// <summary>
        /// Protects the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns></returns>
        public byte[] Protect(byte[] data, string entropy = "")
        {
            var encrypted = _encrypt.Encode(data);
            return _sign.Encode(encrypted);
        }

        /// <summary>
        /// Unprotects the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns></returns>
        public byte[] Unprotect(byte[] data, string entropy = "")
        {
            var validated = _sign.Decode(data);
            return _encrypt.Decode(validated);
        }
    }
}