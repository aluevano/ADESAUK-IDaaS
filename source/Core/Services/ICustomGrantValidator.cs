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

using IdentityServer.Core.Validation;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services
{
    /// <summary>
    /// Handles validation of token requests using custom grant types
    /// </summary>
    public interface ICustomGrantValidator
    {
        /// <summary>
        /// Validates the custom grant request.
        /// </summary>
        /// <param name="request">The validated token request.</param>
        /// <returns>A principal</returns>
        Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request);

        /// <summary>
        /// Returns the grant type this validator can deal with
        /// </summary>
        /// <value>
        /// The type of the grant.
        /// </value>
        string GrantType { get; }
    }
}