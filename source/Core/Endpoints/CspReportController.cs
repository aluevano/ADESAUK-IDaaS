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
using IdentityServer.Core.Events;
using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Services;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.Core.Endpoints
{
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    internal class CspReportController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions options;
        private readonly IEventService eventService;

        public CspReportController(IdentityServerOptions options, IEventService eventService)
        {
            this.options = options;
            this.eventService = eventService;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("CSP Report endpoint requested");

            if (Request.Content.Headers.ContentLength.HasValue && 
                Request.Content.Headers.ContentLength.Value > options.InputLengthRestrictions.CspReport)
            {
                var msg = "Request content exceeds max length";
                Logger.Warn(msg);
                await eventService.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.CspReport, msg);
                return BadRequest();
            }

            var json = await Request.GetOwinContext().Request.ReadBodyAsStringAsync();
            if (json.Length > options.InputLengthRestrictions.CspReport)
            {
                var msg = "Request content exceeds max length";
                Logger.Warn(msg);
                await eventService.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.CspReport, msg);
                return BadRequest();
            }

            if (json.IsPresent())
            {
                Logger.InfoFormat("CSP Report data: {0}", json);
                await eventService.RaiseCspReportEventAsync(json, User as ClaimsPrincipal);
            }

            Logger.Info("Rendering 204");
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}