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

using Microsoft.Owin.Logging;
using System;
using System.Diagnostics;

namespace IdentityServer.Core.Logging
{
    internal class LibLogKatanaLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string name)
        {
            return new LibLogLogger(LogProvider.GetLogger(name));
        }

        private class LibLogLogger : ILogger
        {
            private readonly ILog _logger;

            public LibLogLogger(ILog logger)
            {
                _logger = logger;
            }

            public bool WriteCore(
                TraceEventType eventType,
                int eventId,
                object state,
                Exception exception,
                Func<object, Exception, string> formatter)
            {
                return state == null
                    ? _logger.Log(Map(eventType), null) // Equivalent to IsLogLevelXEnabled 
                    //TODO What to do with eventId?
                    : _logger.Log(Map(eventType), () => formatter(state, exception), exception);
            }

            private LogLevel Map(TraceEventType eventType)
            {
                switch (eventType)
                {
                    case TraceEventType.Critical:
                        return LogLevel.Fatal;
                    case TraceEventType.Error:
                        return LogLevel.Error;
                    case TraceEventType.Warning:
                        return LogLevel.Warn;
                    case TraceEventType.Information:
                        return LogLevel.Info;
                    case TraceEventType.Verbose:
                        return LogLevel.Trace;
                    case TraceEventType.Start:
                        return LogLevel.Info;
                    case TraceEventType.Stop:
                        return LogLevel.Info;
                    case TraceEventType.Suspend:
                        return LogLevel.Info;
                    case TraceEventType.Resume:
                        return LogLevel.Info;
                    case TraceEventType.Transfer:
                        return LogLevel.Info;
                    default:
                        return LogLevel.Info;
                }
            }
        }
    }
}
