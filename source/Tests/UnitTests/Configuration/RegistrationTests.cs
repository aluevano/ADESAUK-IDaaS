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
using IdentityServer.Core.Configuration;
using IdentityServer.Core.Services;
using System;
using Xunit;

namespace IdentityServer.Tests.Configuration
{

    public class RegistrationTests
    {
        [Fact]
        public void RegisterSingleton_NullInstance_Throws()
        {
            Action act = () => new Registration<object>((object)null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("instance");
        }

        [Fact]
        public void RegisterSingleton_Instance_ReturnsSingleton()
        {
            object theSingleton = new object();
            var reg = new Registration<object>((object)theSingleton);
            var result = reg.Instance;
            result.Should().BeSameAs(theSingleton);
        }

        [Fact]
        public void RegisterFactory_NullFunc_Throws()
        {
            Action act = () => new Registration<object>((Func<IDependencyResolver, object>)null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("factory");
        }
        
        [Fact]
        public void RegisterFactory_FactoryInvokesFunc()
        {
            var wasCalled = false;
            Func<IDependencyResolver, object> f = (resolver) => { wasCalled = true; return new object(); };
            var reg = new Registration<object>(f);
            var result = reg.Factory(null);
            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void RegisterType_NullType_Throws()
        {
            Action act = () => new Registration<object>((Type)null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("type");
        }

        [Fact]
        public void RegisterType_SetsTypeOnRegistration()
        {
            var result = new Registration<object>(typeof(string));
            result.Type.Should().Be(typeof(string));
        }
    }
}
