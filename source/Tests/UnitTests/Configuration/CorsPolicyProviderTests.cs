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

using FluentAssertions;
using IdentityServer.Core.Configuration.Hosting;
using IdentityServer.Core.Extensions;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CorsPolicy = System.Web.Cors.CorsPolicy;

namespace IdentityServer.Tests.Configuration
{
    internal class TestCorsPolicyProvider : CorsPolicyProvider
    {
        public TestCorsPolicyProvider(IEnumerable<string> paths)
            : base(paths)
        {
        }

        protected override Task<bool> IsOriginAllowed(string origin, IDictionary<string, object> env)
        {
            return Task.FromResult(true);
        }
    }

    public class CorsPolicyProviderTests
    {
        IOwinRequest Request(string origin = null, string path = null)
        {
            var env = new Dictionary<string, object>();
            env.Add("owin.RequestScheme", "https");
            env.Add("owin.RequestPathBase", "");
            env.Add("owin.RequestPath", path);
            env.SetIdentityServerHost("https://identityserver.io");

            var headers = new Dictionary<string, string[]>();
            headers.Add("Host", new string[]{"identityserver.io"});
            env.Add("owin.RequestHeaders", headers);

            var ctx = new OwinContext(env);
            ctx.Request.Path = new PathString(path);
            if (origin != null)
            {
                ctx.Request.Headers.Add("Origin", new string[] { origin });
            }
            return ctx.Request;
        }

        void AssertAllowed(string origin, CorsPolicy cp)
        {
            cp.AllowAnyHeader.Should().BeTrue();
            cp.AllowAnyMethod.Should().BeTrue();
            cp.Origins.Count.Should().Be(1);
            cp.Origins.Should().Contain(origin);
        }

        [Fact]
        public void ctor_NullPaths_Throws()
        {
            Action act = () => new TestCorsPolicyProvider(null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("allowedPaths");
        }

        [Fact]
        public void GetCorsPolicyAsync_MatchingPath_AllowsOrigin()
        {
            var origin = "http://foo.com";
            var path = "/bar";

            var subject = new TestCorsPolicyProvider(new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, path)).Result;
            AssertAllowed(origin, cp);
        }
        
        [Fact]
        public void GetCorsPolicyAsync_NoOrigin_DoesNotAllowrigin()
        {
            string origin = null;
            var path = "/bar";

            var subject = new TestCorsPolicyProvider(new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, path)).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_NoMatchingPath_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var path = "/bar";

            var subject = new TestCorsPolicyProvider(new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_MatchingPaths_AllowsOrigin()
        {
            var origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new string[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_NoMatchingPaths_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new string[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bad")).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_PathDoesNotStartWithSlash_NormalizesPathCorrectly()
        {
            var origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new string[] { "bar" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_PathEndsWithSlash_NormalizesPathCorrectly()
        {
            var origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new string[] { "bar/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }
    }
}
