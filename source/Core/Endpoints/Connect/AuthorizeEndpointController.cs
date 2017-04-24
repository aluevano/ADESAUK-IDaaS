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
using IdentityServer.Core.Configuration.Hosting;
using IdentityServer.Core.Events;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.ResponseHandling;
using IdentityServer.Core.Results;
using IdentityServer.Core.Services;
using IdentityServer.Core.Validation;
using IdentityServer.Core.ViewModels;
using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Endpoints
{
    /// <summary>
    /// OAuth2/OpenID Connect authorize endpoint
    /// </summary>
    [ErrorPageFilter]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    internal class AuthorizeEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IViewService _viewService;
        private readonly AuthorizeRequestValidator _validator;
        private readonly AuthorizeResponseGenerator _responseGenerator;
        private readonly AuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IdentityServerOptions _options;
        private readonly ILocalizationService _localizationService;
        private readonly IEventService _events;
        private readonly AntiForgeryToken _antiForgeryToken;
        private readonly ClientListCookie _clientListCookie;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeEndpointController" /> class.
        /// </summary>
        /// <param name="viewService">The view service.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="responseGenerator">The response generator.</param>
        /// <param name="interactionGenerator">The interaction generator.</param>
        /// <param name="options">The options.</param>
        /// <param name="localizationService">The localization service.</param>
        /// <param name="events">The event service.</param>
        /// <param name="antiForgeryToken">The anti forgery token.</param>
        /// <param name="clientListCookie">The client list cookie.</param>
        public AuthorizeEndpointController(
            IViewService viewService,
            AuthorizeRequestValidator validator,
            AuthorizeResponseGenerator responseGenerator,
            AuthorizeInteractionResponseGenerator interactionGenerator,
            IdentityServerOptions options,
            ILocalizationService localizationService,
            IEventService events,
            AntiForgeryToken antiForgeryToken,
            ClientListCookie clientListCookie)
        {
            _viewService = viewService;
            _options = options;

            _responseGenerator = responseGenerator;
            _interactionGenerator = interactionGenerator;
            _validator = validator;
            _localizationService = localizationService;
            _events = events;
            _antiForgeryToken = antiForgeryToken;
            _clientListCookie = clientListCookie;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            Logger.Info("Start authorize request");

            var response = await ProcessRequestAsync(request.RequestUri.ParseQueryString());

            Logger.Info("End authorize request");
            return response;
        }

        private async Task<IHttpActionResult> ProcessRequestAsync(NameValueCollection parameters, UserConsent consent = null)
        {
            // validate request
            var result = await _validator.ValidateAsync(parameters, User as ClaimsPrincipal);
            
            if (result.IsError)
            {
                return await this.AuthorizeErrorAsync(
                    result.ErrorType,
                    result.Error,
                    result.ErrorDescription,
                    result.ValidatedRequest);
            }

            var request = result.ValidatedRequest;
            var loginInteraction = await _interactionGenerator.ProcessLoginAsync(request, User as ClaimsPrincipal);

            if (loginInteraction.IsError)
            {
                return await this.AuthorizeErrorAsync(
                    loginInteraction.Error.ErrorType,
                    loginInteraction.Error.Error,
                    null,
                    request);
            }
            if (loginInteraction.IsLogin)
            {
                return this.RedirectToLogin(loginInteraction.SignInMessage, request.Raw);
            }

            // user must be authenticated at this point
            if (!User.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            request.Subject = User as ClaimsPrincipal;

            // now that client configuration is loaded, we can do further validation
            loginInteraction = await _interactionGenerator.ProcessClientLoginAsync(request);
            if (loginInteraction.IsLogin)
            {
                return this.RedirectToLogin(loginInteraction.SignInMessage, request.Raw);
            }

            var consentInteraction = await _interactionGenerator.ProcessConsentAsync(request, consent);

            if (consentInteraction.IsError)
            {
                return await this.AuthorizeErrorAsync(
                    consentInteraction.Error.ErrorType,
                    consentInteraction.Error.Error,
                    null,
                    request);
            }

            if (consentInteraction.IsConsent)
            {
                Logger.Info("Showing consent screen");
                return CreateConsentResult(request, consent, request.Raw, consentInteraction.ConsentError);
            }

            return await CreateAuthorizeResponseAsync(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IHttpActionResult> PostConsent(UserConsent model)
        {
            Logger.Info("Resuming from consent, restarting validation");
            return ProcessRequestAsync(Request.RequestUri.ParseQueryString(), model ?? new UserConsent());
        }

        [HttpGet]
        public async Task<IHttpActionResult> LoginAsDifferentUser()
        {
            var parameters = Request.RequestUri.ParseQueryString();
            parameters[Constants.AuthorizeRequest.Prompt] = Constants.PromptModes.Login;
            return await ProcessRequestAsync(parameters);
        }

        private async Task<IHttpActionResult> CreateAuthorizeResponseAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateResponseAsync(request);

            if (request.ResponseMode == Constants.ResponseModes.Query ||
                request.ResponseMode == Constants.ResponseModes.Fragment)
            {
                Logger.DebugFormat("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                _clientListCookie.AddClient(request.ClientId);

                await RaiseSuccessEventAsync();
                return new AuthorizeRedirectResult(response, _options);
            }

            if (request.ResponseMode == Constants.ResponseModes.FormPost)
            {
                Logger.DebugFormat("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                _clientListCookie.AddClient(request.ClientId);

                await RaiseSuccessEventAsync();
                return new AuthorizeFormPostResult(response, Request);
            }

            Logger.Error("Unsupported response mode. Aborting.");
            throw new InvalidOperationException("Unsupported response mode");
        }

        private IHttpActionResult CreateConsentResult(
            ValidatedAuthorizeRequest validatedRequest,
            UserConsent consent,
            NameValueCollection requestParameters,
            string errorMessage)
        {
            string loginWithDifferentAccountUrl = null;
            if (validatedRequest.HasIdpAcrValue() == false)
            {
                loginWithDifferentAccountUrl = Url.Route(Constants.RouteNames.Oidc.SwitchUser, null)
                    .AddQueryString(requestParameters.ToQueryString());
            }
            
            var env = Request.GetOwinEnvironment();
            var consentModel = new ConsentViewModel
            {
                RequestId = env.GetRequestId(),
                SiteName = _options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                ErrorMessage = errorMessage,
                CurrentUser = env.GetCurrentUserDisplayName(),
                LogoutUrl = env.GetIdentityServerLogoutUrl(),
                ClientName = validatedRequest.Client.ClientName,
                ClientUrl = validatedRequest.Client.ClientUri,
                ClientLogoUrl = validatedRequest.Client.LogoUri,
                IdentityScopes = validatedRequest.GetIdentityScopes(this._localizationService),
                ResourceScopes = validatedRequest.GetResourceScopes(this._localizationService),
                AllowRememberConsent = validatedRequest.Client.AllowRememberConsent,
                RememberConsent = consent == null || consent.RememberConsent,
                LoginWithDifferentAccountUrl = loginWithDifferentAccountUrl,
                ConsentUrl = Url.Route(Constants.RouteNames.Oidc.Consent, null).AddQueryString(requestParameters.ToQueryString()),
                AntiForgery = _antiForgeryToken.GetAntiForgeryToken()
            };

            return new ConsentActionResult(_viewService, consentModel, validatedRequest);
        }

        IHttpActionResult RedirectToLogin(SignInMessage message, NameValueCollection parameters)
        {
            message = message ?? new SignInMessage();

            var path = Url.Route(Constants.RouteNames.Oidc.Authorize, null).AddQueryString(parameters.ToQueryString());
            var host = new Uri(Request.GetOwinEnvironment().GetIdentityServerHost());
            var url = new Uri(host, path);
            message.ReturnUrl = url.AbsoluteUri;

            return new LoginResult(Request.GetOwinContext().Environment, message);
        }

        async Task<IHttpActionResult> AuthorizeErrorAsync(ErrorTypes errorType, string error, string errorDescription, ValidatedAuthorizeRequest request)
        {
            await RaiseFailureEventAsync(error);

            // show error message to user
            if (errorType == ErrorTypes.User)
            {
                var env = Request.GetOwinEnvironment();
                var errorModel = new ErrorViewModel
                {
                    RequestId = env.GetRequestId(),
                    SiteName = _options.SiteName,
                    SiteUrl = env.GetIdentityServerBaseUrl(),
                    CurrentUser = env.GetCurrentUserDisplayName(),
                    LogoutUrl = env.GetIdentityServerLogoutUrl(),
                    ErrorMessage = LookupErrorMessage(error)
                };

                var errorResult = new ErrorActionResult(_viewService, errorModel);
                return errorResult;
            }

            // return error to client
            var response = new AuthorizeResponse
            {
                Request = request,

                IsError = true,
                Error = error,
                ErrorDescription = errorDescription,
                State = request.State,
                RedirectUri = request.RedirectUri
            };

            if (request.ResponseMode == Constants.ResponseModes.FormPost)
            {
                return new AuthorizeFormPostResult(response, Request);
            }
            else
            {
                return new AuthorizeRedirectResult(response, _options);
            }
        }

        private async Task RaiseSuccessEventAsync()
        {
            await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.Authorize);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.Authorize, error);
        }

        private string LookupErrorMessage(string error)
        {
            var msg = _localizationService.GetMessage(error);
            if (msg.IsMissing())
            {
                msg = error;
            }
            return msg;
        }
    }
}