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

using System;
using System.IO;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services.Default
{
    /// <summary>
    /// View loader implementation that uses a combination of the file system view loader 
    /// and the embedded assets view loader. This allows for some templates to be defined 
    /// via the file system, while using the embedded assets templates for all others.
    /// </summary>
    public class FileSystemWithEmbeddedFallbackViewLoader : IViewLoader
    {
        readonly FileSystemViewLoader file;
        readonly EmbeddedAssetsViewLoader embedded;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemWithEmbeddedFallbackViewLoader"/> class.
        /// </summary>
        public FileSystemWithEmbeddedFallbackViewLoader()
            : this(GetDefaultDirectory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemWithEmbeddedFallbackViewLoader"/> class.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public FileSystemWithEmbeddedFallbackViewLoader(string directory)
        {
            this.file = new FileSystemViewLoader(directory);
            this.embedded = new EmbeddedAssetsViewLoader();
        }

        static string GetDefaultDirectory()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.Combine(path, "templates");
            return path;
        }

        /// <summary>
        /// Loads the HTML for the named view.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public async Task<string> LoadAsync(string name)
        {
            var value = await file.LoadAsync(name);
            if (value == null)
            {
                value = await embedded.LoadAsync(name);
            }
            return value;
        }
    }
}
