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
using IdentityServer.Core.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer.Tests.Endpoints.Connect.PoP
{
    static class Helper
    {
        public static RSACryptoServiceProvider CreateProvider(int keySize = 2048)
        {
            var csp = new CspParameters
            {
                Flags = CspProviderFlags.CreateEphemeralKey,
                KeyNumber = (int)KeyNumber.Signature
            };

            return new RSACryptoServiceProvider(keySize, csp);
        }

        public static RsaPublicKeyJwk CreateJwk()
        {
            var prov = CreateProvider();
            var pubKey = prov.ExportParameters(false);

            var jwk = new RsaPublicKeyJwk("key1")
            {
                kty = "RSA",
                n = Base64Url.Encode(pubKey.Modulus),
                e = Base64Url.Encode(pubKey.Exponent)
            };

            return jwk;
        }

        public static string CreateJwkString(RsaPublicKeyJwk jwk = null)
        {
            if (jwk == null) jwk = CreateJwk();

            var json = JsonConvert.SerializeObject(jwk);
            return Base64Url.Encode(Encoding.ASCII.GetBytes(json));
        }
    }
}