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

using IdentityServer.Core.Extensions;
using IdentityServer.Core.Resources;
using IdentityServer.Core.Services;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace IdentityServer.Core.Configuration.Hosting
{
    internal class PreventUnsupportedRequestMediaTypesAttribute : AuthorizationFilterAttribute
    {
        readonly bool allowJson;
        readonly bool allowFormUrlEncoded;
        
        public PreventUnsupportedRequestMediaTypesAttribute(bool allowJson = false, bool allowFormUrlEncoded = false)
        {
            this.allowJson = allowJson;
            this.allowFormUrlEncoded = allowFormUrlEncoded;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Content != null &&
                actionContext.Request.Content.Headers.ContentType != null)
            {
                if (allowJson && 
                    actionContext.Request.Content.Headers.ContentType.MediaType == JsonMediaTypeFormatter.DefaultMediaType.MediaType)
                {
                    return;
                }
                
                if (allowFormUrlEncoded && 
                    actionContext.Request.Content.Headers.ContentType.MediaType == FormUrlEncodedMediaTypeFormatter.DefaultMediaType.MediaType)
                {
                    return;
                }

                var env = actionContext.Request.GetOwinEnvironment();
                var localization = env.ResolveDependency<ILocalizationService>();

                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.UnsupportedMediaType, 
                    new { ErrorMessage = localization.GetMessage(MessageIds.UnsupportedMediaType) }
                );
            }
        }
    }
}
