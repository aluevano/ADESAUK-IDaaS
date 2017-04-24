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

using IdentityServer.Core.Events;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Services;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    internal class ClientSecretValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IClientStore _clients;
        private readonly OwinEnvironmentService _environment;
        private readonly IEventService _events;
        private readonly SecretParser _parser;
        private readonly SecretValidator _validator;

        public ClientSecretValidator(IClientStore clients, SecretParser parser, SecretValidator validator, OwinEnvironmentService environment, IEventService events)
        {
            _clients = clients;
            _parser = parser;
            _validator = validator;
            _environment = environment;
            _events = events;
        }

        public async Task<ClientSecretValidationResult> ValidateAsync()
        {
            Logger.Debug("Start client validation");

            var fail = new ClientSecretValidationResult
            {
                IsError = true
            };

            var parsedSecret = await _parser.ParseAsync(_environment.Environment);
            if (parsedSecret == null)
            {
                await RaiseFailureEvent("unknown", "No client id or secret found");

                Logger.Info("No client secret found");
                return fail;
            }

            // load client
            var client = await _clients.FindClientByIdAsync(parsedSecret.Id);
            if (client == null)
            {
                await RaiseFailureEvent(parsedSecret.Id, "Unknown client");

                Logger.Info("No client with that id found. aborting");
                return fail;
            }

            var result = await _validator.ValidateAsync(parsedSecret, client.ClientSecrets);

            if (result.Success)
            {
                Logger.Info("Client validation success");

                var success = new ClientSecretValidationResult
                {
                    IsError = false,
                    Client = client
                };

                await RaiseSuccessEvent(client.ClientId);
                return success;
            }

            await RaiseFailureEvent(client.ClientId, "Invalid client secret");
            Logger.Info("Client validation failed.");
            
            return fail;
        }

        private async Task RaiseSuccessEvent(string clientId)
        {
            await _events.RaiseSuccessfulClientAuthenticationEventAsync(clientId, EventConstants.ClientTypes.Client);
        }

        private async Task RaiseFailureEvent(string clientId, string message)
        {
            await _events.RaiseFailureClientAuthenticationEventAsync(message, clientId, EventConstants.ClientTypes.Client);
        }
    }
}