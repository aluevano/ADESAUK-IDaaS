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
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default refresh token service
    /// </summary>
    public class DefaultRefreshTokenService : IRefreshTokenService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// The refresh token store
        /// </summary>
        protected readonly IRefreshTokenStore _store;

        /// <summary>
        /// The _events
        /// </summary>
        protected readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRefreshTokenService" /> class.
        /// </summary>
        /// <param name="store">The refresh token store.</param>
        /// <param name="events">The events.</param>
        public DefaultRefreshTokenService(IRefreshTokenStore store, IEventService events)
        {
            _store = store;
            _events = events;
        }

        /// <summary>
        /// Creates the refresh token.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        public virtual async Task<string> CreateRefreshTokenAsync(ClaimsPrincipal subject, Token accessToken, Client client)
        {
            Logger.Debug("Creating refresh token");

            int lifetime;
            if (client.RefreshTokenExpiration == TokenExpiration.Absolute)
            {
                Logger.Debug("Setting an absolute lifetime: " + client.AbsoluteRefreshTokenLifetime);
                lifetime = client.AbsoluteRefreshTokenLifetime;
            }
            else
            {
                Logger.Debug("Setting a sliding lifetime: " + client.SlidingRefreshTokenLifetime);
                lifetime = client.SlidingRefreshTokenLifetime;
            }

            var handle = CryptoRandom.CreateUniqueId();
            var refreshToken = new RefreshToken
            {
                CreationTime = DateTimeOffsetHelper.UtcNow,
                LifeTime = lifetime,
                AccessToken = accessToken,
                Subject = subject
            };

            await _store.StoreAsync(handle, refreshToken);

            await RaiseRefreshTokenIssuedEventAsync(handle, refreshToken);
            return handle;
        }

        /// <summary>
        /// Updates the refresh token.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        public virtual async Task<string> UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken, Client client)
        {
            Logger.Debug("Updating refresh token");

            bool needsUpdate = false;
            string newHandle = handle;

            if (client.RefreshTokenUsage == TokenUsage.OneTimeOnly)
            {
                Logger.Debug("Token usage is one-time only. Generating new handle");

                // delete old one
                await _store.RemoveAsync(handle);

                // create new one
                newHandle = CryptoRandom.CreateUniqueId();
                needsUpdate = true;
            }

            if (client.RefreshTokenExpiration == TokenExpiration.Sliding)
            {
                Logger.Debug("Refresh token expiration is sliding - extending lifetime");

                // make sure we don't exceed absolute exp
                // cap it at absolute exp
                var currentLifetime = refreshToken.CreationTime.GetLifetimeInSeconds();
                Logger.Debug("Current lifetime: " + currentLifetime.ToString());

                var newLifetime = currentLifetime + client.SlidingRefreshTokenLifetime;
                Logger.Debug("New lifetime: " + newLifetime.ToString());

                if (newLifetime > client.AbsoluteRefreshTokenLifetime)
                {
                    newLifetime = client.AbsoluteRefreshTokenLifetime;
                    Logger.Debug("New lifetime exceeds absolute lifetime, capping it to " + newLifetime.ToString());
                }

                refreshToken.LifeTime = newLifetime;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _store.StoreAsync(newHandle, refreshToken);
                Logger.Debug("Updated refresh token in store");
            }
            else
            {
                Logger.Debug("No updates to refresh token done");
            }

            await RaiseRefreshTokenRefreshedEventAsync(handle, newHandle, refreshToken);

            return newHandle;
        }

        /// <summary>
        /// Raises the refresh token issued event.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="token">The token.</param>
        protected async Task RaiseRefreshTokenIssuedEventAsync(string handle, RefreshToken token)
        {
            await _events.RaiseRefreshTokenIssuedEventAsync(handle, token);
        }

        /// <summary>
        /// Raises the refresh token refreshed event.
        /// </summary>
        /// <param name="oldHandle">The old handle.</param>
        /// <param name="newHandle">The new handle.</param>
        /// <param name="token">The token.</param>
        protected async Task RaiseRefreshTokenRefreshedEventAsync(string oldHandle, string newHandle, RefreshToken token)
        {
            await _events.RaiseSuccessfulRefreshTokenRefreshEventAsync(oldHandle, newHandle, token);
        }
    }
}