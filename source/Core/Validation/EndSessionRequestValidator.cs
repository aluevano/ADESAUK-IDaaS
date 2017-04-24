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

using IdentityServer.Core.Configuration;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Services;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    internal class EndSessionRequestValidator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ValidatedEndSessionRequest _validatedRequest;
        private readonly TokenValidator _tokenValidator;
        private readonly IRedirectUriValidator _uriValidator;
        private readonly IdentityServerOptions _options;

        public ValidatedEndSessionRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public EndSessionRequestValidator(IdentityServerOptions options, TokenValidator tokenValidator, IRedirectUriValidator uriValidator)
        {
            _tokenValidator = tokenValidator;
            _uriValidator = uriValidator;
            _options = options;

            _validatedRequest = new ValidatedEndSessionRequest
            {
                Options = options,
            };
        }

        public async Task<ValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            Logger.Info("Start end session request validation");

            _validatedRequest.Raw = parameters;
            _validatedRequest.Subject = subject;

            if (!subject.Identity.IsAuthenticated && _options.AuthenticationOptions.RequireAuthenticatedUserForSignOutMessage)
            {
                Logger.Warn("User is anonymous. Ignoring end session parameters");
                return Invalid();
            }

            var idTokenHint = parameters.Get(Constants.EndSessionRequest.IdTokenHint);
            if (idTokenHint.IsPresent())
            {
                // validate id_token - no need to validate token life time
                var tokenValidationResult = await _tokenValidator.ValidateIdentityTokenAsync(idTokenHint, null, false);
                if (tokenValidationResult.IsError)
                {
                    LogError("Error validating id token hint.");
                    return Invalid();
                }

                _validatedRequest.Client = tokenValidationResult.Client;

                // validate sub claim against currently logged on user
                var subClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
                if (subClaim != null && subject.Identity.IsAuthenticated)
                {
                    if (subject.GetSubjectId() != subClaim.Value)
                    {
                        LogError("Current user does not match identity token");
                        return Invalid();
                    }
                }

                var redirectUri = parameters.Get(Constants.EndSessionRequest.PostLogoutRedirectUri);
                if (redirectUri.IsPresent())
                {
                    _validatedRequest.PostLogOutUri = redirectUri;

                    if (await _uriValidator.IsPostLogoutRedirectUriValidAsync(redirectUri, _validatedRequest.Client) == false)
                    {
                        LogError("Invalid post logout URI");
                        return Invalid();
                    }

                    var state = parameters.Get(Constants.EndSessionRequest.State);
                    if (state.IsPresent())
                    {
                        _validatedRequest.State = state;
                    }
                }
            }

            LogSuccess();
            return Valid();
        }

        private ValidationResult Valid()
        {
            return new ValidationResult
            {
                IsError = false
            };
        }

        private ValidationResult Invalid()
        {
            return new ValidationResult
            {
                IsError = true,
                Error = "Invalid request"
            };
        }

        private void LogError(string message)
        {
            var log = new EndSessionRequestValidationLog(_validatedRequest);
            var json = LogSerializer.Serialize(log);
            
            Logger.ErrorFormat("{0}\n{1}", message, json);
        }

        private void LogSuccess()
        {
            var log = new EndSessionRequestValidationLog(_validatedRequest);
            var json = LogSerializer.Serialize(log);

            Logger.InfoFormat("{0}\n{1}", "End session request validation success", json);
        }
    }
}