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

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;

namespace IdentityServer.Core.Extensions
{
    internal static class ModelStateDictionaryExtensions
    {
        public static IEnumerable<string> GetErrors(this ModelStateDictionary modelState)
        {
            if (modelState == null) return Enumerable.Empty<string>();
            var errors =
                from item in modelState
                where item.Value.Errors.Any()
                from err in item.Value.Errors
                select err.ErrorMessage;
            return errors;
        }

        public static string GetError(this ModelStateDictionary modelState)
        {
            return modelState.GetErrors().FirstOrDefault();
        }

    }
}