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
using Microsoft.Owin;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    internal class EventServiceDecorator : IEventService
    {
        protected static readonly ILog Logger = LogProvider.GetLogger("Events");

        private readonly IdentityServerOptions options;
        private readonly OwinContext context;
        private readonly IEventService inner;

        public EventServiceDecorator(IdentityServerOptions options, OwinEnvironmentService owinEnvironment, IEventService inner)
        {
            this.options = options;
            this.context = new OwinContext(owinEnvironment.Environment);
            this.inner = inner;
        }

        public Task RaiseAsync<T>(Event<T> evt)
        {
            if (CanRaiseEvent(evt))
            {
                evt = PrepareEvent(evt);
                evt.Prepare();
                inner.RaiseAsync(evt);
            }

            return Task.FromResult(0);
        }

        bool CanRaiseEvent<T>(Event<T> evt)
        {
            switch(evt.EventType)
            {
                case EventTypes.Failure:
                    return options.EventsOptions.RaiseFailureEvents;
                case EventTypes.Information:
                    return options.EventsOptions.RaiseInformationEvents;
                case EventTypes.Success:
                    return options.EventsOptions.RaiseSuccessEvents;
                case EventTypes.Error:
                    return options.EventsOptions.RaiseErrorEvents;
            }

            return false;
        }

        protected virtual Event<T> PrepareEvent<T>(Event<T> evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            evt.Context = new EventContext
            {
                ActivityId = context.GetRequestId(),
                TimeStamp = DateTimeOffsetHelper.UtcNow,
                ProcessId = Process.GetCurrentProcess().Id,
                MachineName = Environment.MachineName,
                RemoteIpAddress = context.Request.RemoteIpAddress,
            };

            var principal = context.Authentication.User;
            if (principal != null && principal.Identity != null)
            {
                var subjectClaim = principal.FindFirst(Constants.ClaimTypes.Subject);
                if (subjectClaim != null)
                {
                    evt.Context.SubjectId = subjectClaim.Value;
                }
            }

            return evt;
        }
    }
}