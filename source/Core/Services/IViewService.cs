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

using IdentityServer.Core.Models;
using IdentityServer.Core.Validation;
using IdentityServer.Core.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace IdentityServer.Core.Services
{
    /// <summary>
    /// Models loading the necessary HTML pages displayed by IdentityServer.
    /// </summary>
    public interface IViewService
    {
        /// <summary>
        /// Loads the HTML for the login page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>Stream for the HTML</returns>
        Task<Stream> Login(LoginViewModel model, SignInMessage message);

        /// <summary>
        /// Loads the HTML for the logout prompt page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        Task<Stream> Logout(LogoutViewModel model, SignOutMessage message);

        /// <summary>
        /// Loads the HTML for the logged out page informing the user that they have successfully logged out.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message);

        /// <summary>
        /// Loads the HTML for the user consent page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="authorizeRequest">The validated authorize request.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest);

        /// <summary>
        /// Loads the HTML for the client permissions page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Stream for the HTML</returns>
        Task<Stream> ClientPermissions(ClientPermissionsViewModel model);

        /// <summary>
        /// Loads the HTML for the error page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Stream for the HTML</returns>
        Task<Stream> Error(ErrorViewModel model);
    }
}