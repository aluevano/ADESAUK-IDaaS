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

using Microsoft.IdentityModel.Protocols;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

namespace Host.Configuration.Extensions
{
    class SyncConfigurationManager : IConfigurationManager<WsFederationConfiguration>
    {
        private readonly IConfigurationManager<WsFederationConfiguration> _inner;

        public SyncConfigurationManager(IConfigurationManager<WsFederationConfiguration> inner)
        {
            _inner = inner;
        }

        public Task<WsFederationConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            var res = AsyncHelper.RunSync(() => _inner.GetConfigurationAsync(cancel));
            return Task.FromResult(res);
        }

        public void RequestRefresh()
        {
            _inner.RequestRefresh();
        }

        private static class AsyncHelper
        {
            private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None,
                TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

            public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            {
                var cultureUi = CultureInfo.CurrentUICulture;
                var culture = CultureInfo.CurrentCulture;
                return _myTaskFactory.StartNew(() =>
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = cultureUi;
                    return func();
                }).Unwrap().GetAwaiter().GetResult();
            }

            public static void RunSync(Func<Task> func)
            {
                var cultureUi = CultureInfo.CurrentUICulture;
                var culture = CultureInfo.CurrentCulture;
                _myTaskFactory.StartNew(() =>
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = cultureUi;
                    return func();
                }).Unwrap().GetAwaiter().GetResult();
            }
        }
    }
}