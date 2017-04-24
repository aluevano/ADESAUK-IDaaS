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
using System.Collections.Generic;

namespace IdentityServer.Tests.Validation
{
    class TestClients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Code Client",
                    Enabled = true,
                    ClientId = "codeclient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.AuthorizationCode,
                    AllowAccessToAllScopes = true,

                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "https://server/cb",
                    },

                    AuthorizationCodeLifetime = 60
                },

                new Client
                {
                    ClientName = "Code with Proof Key Client",
                    Enabled = true,
                    ClientId = "codewithproofkeyclient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.AuthorizationCodeWithProofKey,
                    AllowAccessToAllScopes = true,
                    AllowAccessTokensViaBrowser = true,

                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "https://server/cb",
                    },

                    AuthorizationCodeLifetime = 60
                },

                new Client
                {
                    ClientName = "Hybrid Client",
                    Enabled = true,
                    ClientId = "hybridclient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Hybrid,
                    AllowAccessTokensViaBrowser = true,
                    AllowAccessToAllScopes = true,

                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "https://server/cb",
                    },

                    AuthorizationCodeLifetime = 60
                },

                new Client
                {
                    ClientName = "Hybrid Client - No Access Token via Browser",
                    Enabled = true,
                    ClientId = "hybridclient.nobrowser",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Hybrid,
                    AllowAccessTokensViaBrowser = false,
                    AllowAccessToAllScopes = true,

                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "https://server/cb",
                    },

                    AuthorizationCodeLifetime = 60
                },

                new Client
                {
                    ClientName = "Hybrid with Proof Key Client",
                    Enabled = true,
                    ClientId = "hybridwithproofkeyclient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.HybridWithProofKey,
                    AllowAccessTokensViaBrowser = true,
                    AllowAccessToAllScopes = true,

                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "https://server/cb",
                    },

                    AuthorizationCodeLifetime = 60
                },

                new Client
                {
                    ClientName = "Implicit Client",
                    Enabled = true,
                    ClientId = "implicitclient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    AllowAccessToAllScopes = true,

                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "oob://implicit/cb"
                    },
                },
                new Client
                {
                    ClientName = "Implicit Client - No Access Token via Browser",
                    Enabled = true,
                    ClientId = "implicitclient.nobrowser",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Implicit,
                    AllowAccessTokensViaBrowser = false,
                    AllowAccessToAllScopes = true,

                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "oob://implicit/cb"
                    },
                },
                new Client
                {
                    ClientName = "Implicit and Client Credentials Client",
                    Enabled = true,
                    ClientId = "implicit_and_client_creds_client",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    AllowAccessToAllScopes = true,
                    AllowClientCredentialsOnly = true,
                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "oob://implicit/cb"
                    },
                },
                new Client
                {
                    ClientName = "Code Client with Scope Restrictions",
                    Enabled = true,
                    ClientId = "codeclient_restricted",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.AuthorizationCode,
                    RequireConsent = false,

                    AllowedScopes = new List<string>
                    {
                        "openid"
                    },

                    RedirectUris = new List<string>
                    {
                        "https://server/cb",
                    },
                },
                new Client
                {
                    ClientName = "Client Credentials Client",
                    Enabled = true,
                    ClientId = "client",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ClientCredentials,
                    AllowAccessToAllScopes = true,

                    AccessTokenType = AccessTokenType.Jwt
                },
                new Client
                {
                    ClientName = "Client Credentials Client (restricted)",
                    Enabled = true,
                    ClientId = "client_restricted",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ClientCredentials,

                    AllowedScopes = new List<string>
                    {
                        "resource"
                    },
                },
                new Client
                {
                    ClientName = "Resource Owner Client",
                    Enabled = true,
                    ClientId = "roclient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,
                    AllowAccessToAllScopes = true,
                },
                new Client
                {
                    ClientName = "Resource Owner Client",
                    Enabled = true,
                    ClientId = "roclient_absolute_refresh_expiration_one_time_only",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,
                    AllowAccessToAllScopes = true,

                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    AbsoluteRefreshTokenLifetime = 200
                },
                new Client
                {
                    ClientName = "Resource Owner Client",
                    Enabled = true,
                    ClientId = "roclient_absolute_refresh_expiration_reuse",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,
                    AllowAccessToAllScopes = true,

                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    AbsoluteRefreshTokenLifetime = 200
                },
                new Client
                {
                    ClientName = "Resource Owner Client",
                    Enabled = true,
                    ClientId = "roclient_sliding_refresh_expiration_one_time_only",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,
                    AllowAccessToAllScopes = true,

                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    AbsoluteRefreshTokenLifetime = 10,
                    SlidingRefreshTokenLifetime = 4
                },
                new Client
                {
                    ClientName = "Resource Owner Client",
                    Enabled = true,
                    ClientId = "roclient_sliding_refresh_expiration_reuse",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,
                    AllowAccessToAllScopes = true,

                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    AbsoluteRefreshTokenLifetime = 200,
                    SlidingRefreshTokenLifetime = 100
                },
                new Client
                {
                    ClientName = "Resource Owner Client (restricted)",
                    Enabled = true,
                    ClientId = "roclient_restricted",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,

                    AllowedScopes = new List<string>
                    {
                        "resource"
                    },
                },
                new Client
                {
                    ClientName = "Resource Owner Client (restricted with refresh)",
                    Enabled = true,
                    ClientId = "roclient_restricted_refresh",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,

                    AllowedScopes = new List<string>
                    {
                        "resource",
                        "offline_access"
                    },
                },
                new Client
                {
                    ClientName = "Resource Owner Client (offline only)",
                    Enabled = true,
                    ClientId = "roclient_offline_only",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,

                    AllowedScopes = new List<string>
                    {
                        "offline_access"
                    },
                },
                new Client
                {
                    ClientName = "Custom Grant Client",
                    Enabled = true,
                    ClientId = "customgrantclient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Custom,
                    AllowAccessToAllScopes = true,

                    AllowedCustomGrantTypes = new List<string>
                    {
                        "custom_grant"
                    }

                },
                new Client
                {
                    ClientName = "Disabled Client",
                    Enabled = false,
                    ClientId = "disabled",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("invalid".Sha256())
                    },

                    Flow = Flows.Custom,
                    AllowAccessToAllScopes = true,
                },
                new Client
                {
                    ClientName = "Reference Token Client",

                    Enabled = true,
                    ClientId = "referencetokenclient",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    AllowAccessToAllScopes = true,

                    AccessTokenType = AccessTokenType.Reference
                }
            };
        }
    }
}