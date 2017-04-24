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
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    internal class ScopeSecretValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IScopeStore _scopes;
        private readonly OwinEnvironmentService _environment;
        private readonly IEventService _events;
        private readonly SecretParser _parser;
        private readonly SecretValidator _validator;

        public ScopeSecretValidator(IScopeStore scopes, SecretParser parsers, SecretValidator validator, OwinEnvironmentService environment, IEventService events)
        {
            _scopes = scopes;
            _environment = environment;
            _parser = parsers;
            _validator = validator;
            _events = events;
        }

        public async Task<ScopeSecretValidationResult> ValidateAsync()
        {
            Logger.Debug("Start scope validation");

            var fail = new ScopeSecretValidationResult
            {
                IsError = true
            };

            var parsedSecret = await _parser.ParseAsync(_environment.Environment);
            if (parsedSecret == null)
            {
                await RaiseFailureEvent("unknown", "No scope id or secret found");

                Logger.Info("No scope secret found");
                return fail;
            }

            // load scope
            var scope = (await _scopes.FindScopesAsync(new[] { parsedSecret.Id })).FirstOrDefault();
            if (scope == null)
            {
                await RaiseFailureEvent(parsedSecret.Id, "Unknown scope");

                Logger.Info("No scope with that name found. aborting");
                return fail;
            }

            var result = await _validator.ValidateAsync(parsedSecret, scope.ScopeSecrets);
            if (result.Success)
            {
                Logger.Info("Scope validation success");

                var success = new ScopeSecretValidationResult
                {
                    IsError = false,
                    Scope = scope
                };

                await RaiseSuccessEvent(scope.Name);
                return success;
            }

            await RaiseFailureEvent(scope.Name, "Invalid client secret");
            Logger.Info("Scope validation failed.");

            return fail;
        }

        private async Task RaiseSuccessEvent(string clientId)
        {
            await _events.RaiseSuccessfulClientAuthenticationEventAsync(clientId, EventConstants.ClientTypes.Scope);
        }

        private async Task RaiseFailureEvent(string clientId, string message)
        {
            await _events.RaiseFailureClientAuthenticationEventAsync(message, clientId, EventConstants.ClientTypes.Scope);
        }
    }
}