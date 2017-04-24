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
using IdentityServer.Core.Services.Default;
using Xunit;

namespace IdentityServer.Tests.Services.Default
{
    public class DefaultCorsPolicyServiceTests
    {
        DefaultCorsPolicyService subject;

        public DefaultCorsPolicyServiceTests()
        {
            subject = new DefaultCorsPolicyService();
        }

        [Fact]
        public void IsOriginAllowed_OriginIsAllowed_ReturnsTrue()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.IsOriginAllowedAsync("http://foo").Result.Should().Be(true);
        }

        [Fact]
        public void IsOriginAllowed_OriginIsNotAllowed_ReturnsFalse()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.IsOriginAllowedAsync("http://bar").Result.Should().Be(false);
        }

        [Fact]
        public void IsOriginAllowed_OriginIsInAllowedList_ReturnsTrue()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.AllowedOrigins.Add("http://bar");
            subject.AllowedOrigins.Add("http://baz");
            subject.IsOriginAllowedAsync("http://bar").Result.Should().Be(true);
        }

        [Fact]
        public void IsOriginAllowed_OriginIsNotInAllowedList_ReturnsFalse()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.AllowedOrigins.Add("http://bar");
            subject.AllowedOrigins.Add("http://baz");
            subject.IsOriginAllowedAsync("http://quux").Result.Should().Be(false);
        }

        [Fact]
        public void IsOriginAllowed_AllowAllTrue_ReturnsTrue()
        {
            subject.AllowAll = true;
            subject.IsOriginAllowedAsync("http://foo").Result.Should().Be(true);
        }
    }
}
