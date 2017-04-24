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
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer.Tests.Validation
{
    static class ClientValidationTestClients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Disabled client",
                    ClientId = "disabled_client",
                    Enabled = false,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret")
                    }
                },

                new Client
                {
                    ClientName = "Client with no secret set",
                    ClientId = "no_secret_client",
                    Enabled = true
                },

                new Client
                {
                    ClientName = "Client with single secret, no protection, no expiration",
                    ClientId = "single_secret_no_protection_no_expiration",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret")
                    }
                },

                new Client
                {
                    ClientName = "Client with X509 Certificate",
                    ClientId = "certificate_valid",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret
                        {
                            Type = Constants.SecretTypes.X509CertificateThumbprint,
                            Value = TestCert.Load().Thumbprint
                        }
                    }
                },

                new Client
                {
                    ClientName = "Client with X509 Certificate",
                    ClientId = "certificate_invalid",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret
                        {
                            Type = Constants.SecretTypes.X509CertificateThumbprint,
                            Value = "invalid"
                        }
                    }
                },

                new Client
                {
                    ClientName = "Client with Base64 encoded X509 Certificate",
                    ClientId = "certificate_base64_valid",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret
                        {
                            Type = Constants.SecretTypes.X509CertificateBase64,
                            Value = Convert.ToBase64String(TestCert.Load().Export(X509ContentType.Cert))
                        }
                    }
                },

                new Client
                {
                    ClientName = "Client with Base64 encoded X509 Certificate",
                    ClientId = "certificate_base64_invalid",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret
                        {
                            Type = Constants.SecretTypes.X509CertificateBase64,
                            Value = "invalid"
                        }
                    }
                },

                new Client
                {
                    ClientName = "Client with single secret, hashed, no expiration",
                    ClientId = "single_secret_hashed_no_expiration",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        // secret
                        new Secret("secret".Sha256())
                    }
                },

                new Client
                {
                    ClientName = "Client with multiple secrets, no protection",
                    ClientId = "multiple_secrets_no_protection",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret"),
                        new Secret("foobar", "some description"),
                        new Secret("quux"),
                        new Secret("notexpired", DateTimeOffset.UtcNow.AddDays(1)),
                        new Secret("expired", DateTimeOffset.UtcNow.AddDays(-1)),
                    }
                },

                new Client
                {
                    ClientName = "Client with multiple secrets, hashed",
                    ClientId = "multiple_secrets_hashed",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        // secret
                        new Secret("secret".Sha256()),
                        // foobar
                        new Secret("foobar".Sha256(), "some description"),
                        // quux
                        new Secret("quux".Sha512()),
                        // notexpired
                        new Secret("notexpired".Sha256(), DateTimeOffset.UtcNow.AddDays(1)),
                        // expired
                        new Secret("expired".Sha512(), DateTimeOffset.UtcNow.AddDays(-1)),
                    }
                },
            };
        }
    }
}
