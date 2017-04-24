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
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    /// <summary>
    /// Validates a secret based on RS256 signed JWT token
    /// </summary>
    public class PrivateKeyJwtSecretValidator : ISecretValidator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly string audienceUri;

        /// <summary>
        /// Instantiates an instance of private_key_jwt secret validator
        /// </summary>
        /// <param name="options">IdentityServer options</param>
        public PrivateKeyJwtSecretValidator(IdentityServerOptions options)
        {
            audienceUri = string.Concat(options.DynamicallyCalculatedIssuerUri.EnsureTrailingSlash(), Constants.RoutePaths.Oidc.Token);
        }

        /// <summary>
        /// Validates a secret
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>
        /// A validation result
        /// </returns>
        /// <exception cref="System.ArgumentException">ParsedSecret.Credential is not a JWT token</exception>
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });
            var success = Task.FromResult(new SecretValidationResult { Success = true });

            if (parsedSecret.Type != Constants.ParsedSecretTypes.JwtBearer)
            {
                return fail;
            }

            var jwtTokenString = parsedSecret.Credential as string;

            if (jwtTokenString == null)
            {
                throw new ArgumentException("ParsedSecret.Credential is not a string.");
            }

            var enumeratedSecrets = secrets.ToList().AsReadOnly();

            var trustedKeys = GetTrustedKeys(enumeratedSecrets, jwtTokenString);

            if (!trustedKeys.Any())
            {
                Logger.Warn("There are no certificates available to validate client assertion.");
                return fail;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKeys = trustedKeys,
                ValidateIssuerSigningKey = true,

                ValidIssuer = parsedSecret.Id,
                ValidateIssuer = true,

                ValidAudience = audienceUri,
                ValidateAudience = true,

                RequireSignedTokens = true,
                RequireExpirationTime = true
            };
            try
            {
                SecurityToken token;
                var handler = new EmbeddedCertificateJwtSecurityTokenHandler();
                handler.ValidateToken(jwtTokenString, tokenValidationParameters, out token);

                var jwtToken = (JwtSecurityToken)token;

                if (jwtToken.Subject != jwtToken.Issuer)
                {
                    Logger.Warn("Both 'sub' and 'iss' in the client assertion token must have a value of client_id.");
                    return fail;
                }

                return success;
            }
            catch (Exception e)
            {
                Logger.Debug("JWT token validation error: " + e.Message);
                return fail;
            }
        }

        private static List<SecurityKey> GetTrustedKeys(IReadOnlyCollection<Secret> secrets, string jwtTokenString)
        {
            var token = new JwtSecurityToken(jwtTokenString);
            var certificate = token.GetCertificateFromToken();
            if (EmbeddedCertificateIsTrusted(certificate, secrets))
            {
                return new List<SecurityKey>
                {
                    new X509SecurityKey(certificate)
                };
            }

            var trustedKeys = GetAllTrustedCertificates(secrets)
                                .Select(c => (SecurityKey)new X509SecurityKey(c))
                                .ToList();

            if (!trustedKeys.Any()
                && secrets.Any(s => s.Type == Constants.SecretTypes.X509CertificateThumbprint))
            {
                Logger.Warn("Cannot validate client assertion token that does not embed full certificate using only thumbprint.");
            }

            return trustedKeys;
        }

        private static bool EmbeddedCertificateIsTrusted(X509Certificate2 certificate, IReadOnlyCollection<Secret> secrets)
        {
            if (certificate == null || certificate.Thumbprint == null)
            {
                return false;
            }

            if (secrets.Any(s => s.Type == Constants.SecretTypes.X509CertificateThumbprint
                                 && TimeConstantComparer.IsEqual(s.Value.ToLowerInvariant(), certificate.Thumbprint.ToLowerInvariant())))
            {
                return true;
            }

            if (secrets.Any(s => s.Type == Constants.SecretTypes.X509CertificateBase64
                                 && Equals(certificate, GetCertificateFromString(s.Value))))
            {
                return true;
            }

            return false;
        }

        private static List<X509Certificate2> GetAllTrustedCertificates(IEnumerable<Secret> secrets)
        {
            return secrets
                .Where(s => s.Type == Constants.SecretTypes.X509CertificateBase64)
                .Select(s => GetCertificateFromString(s.Value))
                .Where(c => c != null)
                .ToList();
        }

        private static X509Certificate2 GetCertificateFromString(string value)
        {
            try
            {
                return new X509Certificate2(Convert.FromBase64String(value));
            }
            catch
            {
                Logger.Warn("Could not read certificate from string: " + value);
                return null;
            }
        }
    }
}
