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
  
using IdentityServer.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Xunit;

namespace IdentityServer.Tests
{
    public class InternalizedDependencyCompatibilityTests
    {
        private readonly List<Assembly> _bannedAssemblies;

        public InternalizedDependencyCompatibilityTests()
        {
            _bannedAssemblies = new List<Assembly>();
            var excludedAssemblies = new[]
            {
                "Owin"
            };
            foreach (var referencedAssembly in typeof(Constants).Assembly.GetReferencedAssemblies())
            {
                var asm = Assembly.Load(referencedAssembly);
                if (!asm.GlobalAssemblyCache && !excludedAssemblies.Any(x => string.Equals(referencedAssembly.Name, x, StringComparison.OrdinalIgnoreCase)))
                {
                    _bannedAssemblies.Add(asm);
                }
            }
            
        }
        [Fact]
        public void NoTypesShouldExposeAnyIlMergedAssemblies()
        {
            var assembly = typeof(Constants).Assembly;
            var errors = new List<string>();
            foreach (var type in assembly.GetExportedTypes())
            {
                errors.AddRange(CheckConstructor(type));
                //API controllers need to be public if we don't want to rewrite autofac/webapi controller discovery
                if (type.BaseType != typeof (ApiController))
                {
                    errors.AddRange(CheckProperties(type));
                    errors.AddRange(CheckMethods(type));
                }
            }
            Console.WriteLine(errors.Count);
            Assert.Equal(new string[]{}, errors);
        }

        private IEnumerable<string> CheckMethods(Type type)
        {
            foreach (var method in type.GetMethods())
            {
                foreach (var parameterInfo in method.GetParameters())
                {
                    if(_bannedAssemblies.Any(x => x == parameterInfo.ParameterType.Assembly))
                        yield return string.Format("ILMERGED TYPE EXPOSED(method) {3} {0}.{1}({2})",
                        type.FullName,
                        method.Name,
                        FormatParameters(method.GetParameters()), method.ReturnType.Name);
                }
                if(_bannedAssemblies.Any(x => x == method.ReturnType.Assembly))
                    yield return
                        string.Format("ILMERGED TYPE EXPOSED(method) {3} {0}.{1}({2})",
                        type.FullName,
                        method.Name,
                        FormatParameters(method.GetParameters()), method.ReturnType.Name);
            }
        }

        private IEnumerable<string> CheckProperties(Type type)
        {
            foreach (var propInfo in type.GetProperties())
            {
                if(_bannedAssemblies.Any(x => x == propInfo.PropertyType.Assembly))
                    yield return string.Format("ILMERGED TYPE EXPOSED(property): {2} : {0}.{1}", type.FullName, propInfo.Name, propInfo.PropertyType.FullName);
            }
        }

        private IEnumerable<string> CheckConstructor(Type type)
        {
            foreach (var ctor in type.GetConstructors())
            {
                foreach (var parameterInfo in ctor.GetParameters())
                {
                    if(_bannedAssemblies.Any(x => x == parameterInfo.ParameterType.Assembly))
                        yield return string.Format("ILMERGED TYPE EXPOSED {0}.ctor({1})", type.FullName,
                        FormatParameters(ctor.GetParameters()));
                }
            }
        }

        private static string FormatParameters(ParameterInfo[] parameters)
        {
            return string.Join(",", parameters.Select(x => x.ParameterType.Name));
        }
    }
}
