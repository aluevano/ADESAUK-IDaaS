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

namespace IdentityServer.Core.Configuration
{
    internal static class CookieOptionsExtensions
    {
        public static string GetCookieName(this CookieOptions options, string name)
        {
            return options.Prefix + name;
        }

        const string SessionCookieName = "idsvr.session";

        public static string GetSessionCookieName(this CookieOptions options)
        {
            return options.GetCookieName(SessionCookieName);
        }

        internal static bool? CalculateRememberMeFromUserInput(this CookieOptions options, bool? userInput)
        {
            // the browser will only send 'true' if the user has checked the checkbox
            // it will pass nothing if the user does not check the checkbox
            // this check here is to establish if the user deliberatly did not check the checkbox
            // or if the checkbox was not presented as an option (and thus AllowRememberMe is not allowed)
            // true means they did check it, false means they did not, null means they were not presented with the choice
            if (options.AllowRememberMe)
            {
                if (userInput != true)
                {
                    userInput = false;
                }
            }
            else
            {
                userInput = null;
            }
            
            return userInput;
        }
    }
}