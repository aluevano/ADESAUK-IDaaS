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

using IdentityServer.Core.Extensions;
using System.IdentityModel.Tokens;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Can verify tokens that only embed "x5c" key and don't contain "x5t"
    /// </summary>
    /// <remarks>
    /// Current version implementation of <see cref="P:System.IdentityModel.Tokens.JwtHeader.SigningCredentials"/> returns NamedKeySecurityKeyIdentifierClause for "x5c" key in the token and incorrectly handles its value,
    /// which must be an array of Base64 encoded certificates according to the specification.
    /// So it is readded as X509RawDataKeyIdentifierClause, which is then correctly validated by the default JwtSecurityTokenHandler implementation
    /// </remarks>
    internal class EmbeddedCertificateJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        protected override SecurityKey ResolveIssuerSigningKey(string token, SecurityToken securityToken, SecurityKeyIdentifier keyIdentifier, TokenValidationParameters validationParameters)
        {
            var certificate = ((JwtSecurityToken)securityToken).GetCertificateFromToken();
            if (certificate != null)
            {
                keyIdentifier.Add(new X509RawDataKeyIdentifierClause(certificate));
            }
            return base.ResolveIssuerSigningKey(token, securityToken, keyIdentifier, validationParameters);
        }
    }
}
