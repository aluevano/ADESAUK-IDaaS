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

using IdentityServer.Core.Services;
using System;

namespace IdentityServer.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IdentityServer.Core.Services.ILocalizationService"/>
    /// </summary>
    public static class ILocalizationServiceExtensions
    {
        /// <summary>
        /// Gets a localized string for the message category.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetMessage(this ILocalizationService localization, string id)
        {
            if (localization == null) throw new ArgumentNullException("localization");

            return localization.GetString(Constants.LocalizationCategories.Messages, id);
        }

        /// <summary>
        /// Gets a localized string for the event category.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetEvent(this ILocalizationService localization, string id)
        {
            if (localization == null) throw new ArgumentNullException("localization");

            return localization.GetString(Constants.LocalizationCategories.Events, id);
        }

        /// <summary>
        /// Gets a localized scope display name.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetScopeDisplayName(this ILocalizationService localization, string scope)
        {
            if (localization == null) throw new ArgumentNullException("localization");
            
            return localization.GetString(Constants.LocalizationCategories.Scopes, scope + Constants.ScopeDisplayNameSuffix);
        }

        /// <summary>
        /// Gets a localized scope description.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetScopeDescription(this ILocalizationService localization, string scope)
        {
            if (localization == null) throw new ArgumentNullException("localization");
            
            return localization.GetString(Constants.LocalizationCategories.Scopes, scope + Constants.ScopeDescriptionSuffix);
        }
    }
}
