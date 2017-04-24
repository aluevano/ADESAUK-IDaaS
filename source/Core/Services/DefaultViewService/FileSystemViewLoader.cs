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
    /// View loader that loads HTML templates from the file system.
    /// </summary>
    public class FileSystemViewLoader : IViewLoader
    {
        readonly string directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemViewLoader"/> class.
        /// </summary>
        /// <param name="directory">The directory from which to load HTML templates.</param>
        /// <exception cref="System.ArgumentNullException">directory</exception>
        public FileSystemViewLoader(string directory)
        {
            if (String.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException("directory");
            
            this.directory = directory;
        }

        /// <summary>
        /// Loads the specified page.
        /// If the file "page.html" exists, then that will be used for the entire template.
        /// If the file "_layout.html" exists, then that will be used for the layout template.
        /// If the file "_page.html" exists, then that will be used for the inner template.
        /// If only one of "_layout.html" or "_page.html" exists, then the embedded assets template is used for the template missing from the file system.
        /// If none of the above files exist, then <c>null</c> is returned.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public Task<string> LoadAsync(string page)
        {
            if (Directory.Exists(directory))
            {
                var name = page + ".html";
                var path = Path.Combine(directory, name);

                // look for full file with name login.html
                if (File.Exists(path))
                {
                    return Task.FromResult(File.ReadAllText(path));
                }

                var layoutName = Path.Combine(directory, "_layout.html");
                string layout = null;
                if (File.Exists(layoutName))
                {
                    layout = File.ReadAllText(layoutName);
                }

                // look for partial with name _login.html
                name = "_" + name;
                path = Path.Combine(directory, name);
                if (File.Exists(path))
                {
                    var partial = File.ReadAllText(path);

                    if (layout != null)
                    {
                        return Task.FromResult(AssetManager.ApplyContentToLayout(layout, partial));
                    }

                    return Task.FromResult(AssetManager.LoadLayoutWithContent(partial));
                }

                // no partial, but layout might exist
                if (layout != null)
                {
                    // so load embedded asset page, but use custom layout
                    var content = AssetManager.LoadPage(page);
                    return Task.FromResult(AssetManager.ApplyContentToLayout(layout, content));
                }
            }

            return Task.FromResult<string>(null);
        }
    }
}
