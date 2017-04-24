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

namespace IdentityServer.Core.Models
{
    /// <summary>
    /// Models a claim that should be emitted in a token
    /// </summary>
    public class ScopeClaim
    {
        /// <summary>
        /// Name of the claim
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the claim
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only. Defaults to false.
        /// </summary>
        public bool AlwaysIncludeInIdToken { get; set; }

        /// <summary>
        /// Creates an empty ScopeClaim
        /// </summary>
        public ScopeClaim()
        { }

        /// <summary>
        /// Creates a ScopeClaim with parameters
        /// </summary>
        /// <param name="name">Name of the claim</param>
        /// <param name="alwaysInclude">Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only.</param>
        public ScopeClaim(string name, bool alwaysInclude = false)
        {
            Name = name;
            Description = string.Empty;
            AlwaysIncludeInIdToken = alwaysInclude;
        }
    }
}